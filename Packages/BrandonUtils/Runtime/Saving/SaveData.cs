using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Packages.BrandonUtils.Runtime.Saving {
    /// <summary>
    ///     A single "Save File", containing data
    /// </summary>
    /// <remarks>
    ///     <para>Implementations will serialize all <see cref="SerializeField" />s in both <see cref="SaveData{T}" /> and the inheritor, such as <see cref="SaveDataTestImpl" />.</para>
    ///     <para>Uses the <a href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">Curiously Repeating Template Pattern</a>, where <typeparamref name="T" /> references the inheritor type, e.g. <see cref="SaveDataTestImpl" />.</para>
    /// </remarks>
    /// <typeparam name="T">The inheriting class, e.g. <see cref="SaveDataTestImpl" /></typeparam>
    /// <seealso cref="SaveDataTestImpl" />
    public abstract class SaveData<T> where T : SaveData<T>, new() {
        public const string SaveFolderName    = "SaveData";
        public const string AutoSaveName      = "AutoSave";
        public const string SaveFileExtension = "sav";
        public const int    BackupSaveSlots   = 10;

        /// <summary>
        ///     The length required length of timestamps in save file names generated via <see cref="GetSaveFileNameWithDate" />
        /// </summary>
        private const int TimeStampLength = 18;

        public const int LoadRetryLimit = 10;

        /// <summary>
        ///     Timestamps will be serialized into file names using their <see cref="DateTime.Ticks" /> value, which will have 18 digits until 11/16/3169 09:46:40.
        /// </summary>
        public static readonly string TimeStampPattern = @"\d{" + TimeStampLength + "}";

        public static readonly string   SaveFilePattern = $@"(?<nickName>.*)_(?<date>{TimeStampPattern})";
        public static readonly string   SaveFolderPath  = Path.Combine(Application.persistentDataPath, SaveFolderName);
        public static readonly TimeSpan ReSaveDelay     = TimeSpan.FromSeconds(1);

        [JsonProperty]
        public string nickName;

        /// <summary>
        ///     Static initializer that makes sure the <see cref="SaveFolderPath" /> exists.
        /// </summary>
        static SaveData() {
            if (!Directory.Exists(SaveFolderPath)) {
                Debug.LogWarning($"{nameof(SaveFolderPath)} at {SaveFolderPath} didn't exist, so it is being created...");
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        protected SaveData() { }

        [JsonProperty]
        public DateTime LastSaveTime;

        [JsonIgnore]
        public string[] AllSaveFilePaths => GetAllSaveFilePaths(nickName);

        [JsonIgnore]
        public string LatestSaveFilePath => GetAllSaveFilePaths(nickName).Last();

        [JsonIgnore]
        public string OldestSaveFilePath => GetAllSaveFilePaths(nickName).First();

        [JsonIgnore]
        public bool Exists => SaveFileExists(nickName);

        public static T Load(string nickName) {
            Debug.Log("Loading save file: " + nickName);
            var attemptCount = 0;
            while (!SaveFileExists(nickName)) {
                if (attemptCount >= LoadRetryLimit) {
                    throw new SaveDataException<T>($"Unable to load the any save files for {nickName} after {attemptCount} attempts!");
                }

                attemptCount++;

                Debug.LogWarning($"[Attempt {attemptCount}] No save files for {nickName} exist! Attempting to create a new one...");
                NewSaveFile(nickName);
            }


            var latestSaveFilePath = GetAllSaveFilePaths(nickName).Last();
            Log($"Found latest save file for {nickName} at path: {latestSaveFilePath}");
            return FromSaveFile(latestSaveFilePath);
        }

        private static T FromSaveFile(string saveFilePath) {
            return JsonUtility.FromJson<T>(File.ReadAllText(saveFilePath));
        }

        /// <summary>
        ///     Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and <see cref="DateTime" />-stamp.
        ///     <br />
        ///     This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetSaveFilePath(string nickName, DateTime dateTime) {
            return Path.ChangeExtension(Path.Combine(SaveFolderPath, GetSaveFileNameWithDate(nickName, dateTime)), SaveFileExtension);
        }

        /// <summary>
        ///     Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and a <see cref="DateTime" />-stamp of <see cref="DateTime.Now" /> via <see cref="GetSaveFilePath" />.
        ///     <br />
        ///     This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static string GetNewSaveFilePath(string nickName) {
            return GetSaveFilePath(nickName, DateTime.Now);
        }

        public static bool SaveFileExists(string nickName) {
            return GetAllSaveFilePaths(nickName).Any(File.Exists);
        }

        /// <summary>
        ///     Creates a new, blank <see cref="SaveData{T}" /> of type <see cref="T" />, and <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string,bool)" />s it as a new file with the <see cref="nickName" /> <paramref name="nickname" />.
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns>the newly created <see cref="SaveData{T}" /></returns>
        public static T NewSaveFile(string nickname) {
            Debug.Log($"Creating a new save file: {nickname} ({GetNewSaveFilePath(nickname)}), of type {typeof(T)}");
            //create the save folder if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(GetNewSaveFilePath(nickname)) ?? throw new SaveDataException<T>($"The path {GetNewSaveFilePath(nickname)} didn't have a valid directory name!", new DirectoryNotFoundException()));

            //create a new, blank save data, and save it as the new file
            return Save(new T(), nickname);
        }

        /// <summary>
        ///     Serializes <paramref name="saveData" /> to a new <see cref="File" />.
        /// </summary>
        /// <remarks>
        ///     <para>The new file will be located at <see cref="GetNewSaveFilePath" />.</para>
        ///     <para>Retains previous saves with the same <paramref name="nickName" />, up to <see cref="BackupSaveSlots" />, via <see cref="TrimSaves" />.</para>
        ///     <para>May update fields in <paramref name="saveData" />, such as <see cref="LastSaveTime" />.</para>
        /// </remarks>
        /// <param name="saveData">The <see cref="SaveData{T}" /> inheritor to be saved.</param>
        /// <param name="nickName">The <see cref="nickName" /> that the <see cref="saveData" /> should be given.</param>
        /// <param name="useReSaveDelay">If <c>true</c>, check if <see cref="ReSaveDelay" /> has elapsed since <see cref="LastSaveTime" />.</param>
        /// <returns>The passed <paramref name="saveData" /> for method chaining.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SaveDataException{T}">If <paramref name="nickName" /> <see cref="string.IsNullOrWhiteSpace" />.</exception>
        /// <exception cref="ReSaveDelayException{t}">If <paramref name="useReSaveDelay" /> is <c>true</c> and <see cref="ReSaveDelay" /> hasn't elapsed since <see cref="LastSaveTime" />.</exception>
        private static T Save([NotNull] SaveData<T> saveData, [NotNull] string nickName, bool useReSaveDelay = true) {
            if (saveData == null) {
                throw new ArgumentNullException(nameof(saveData));
            }

            if (string.IsNullOrWhiteSpace(nickName)) {
                var argException = new ArgumentException("Value cannot be null or whitespace.", nameof(nickName));
                throw new SaveDataException<T>(saveData, $"The name of the file you tried to save was '{nickName}', which is null, blank, or whitespace, so we can't save it!", argException);
            }

            var saveTime = DateTime.Now;

            //throw an error if ReSaveDelay hasn't elapsed since the last time the file was saved
            if (useReSaveDelay && saveTime - saveData.LastSaveTime < ReSaveDelay) {
                throw new ReSaveDelayException<T>(saveData, $"The save file {nickName} was saved too recently!" + $"\n\t{nameof(saveData.LastSaveTime)}: {saveData.LastSaveTime}" + $"\n\tNew saveTime: {saveTime}" + $"\n\t{nameof(ReSaveDelay)}: {ReSaveDelay}" + $"\n\tDelta: {saveTime - saveData.LastSaveTime}");
            }

            saveData.nickName     = nickName;
            saveData.LastSaveTime = saveTime;

            var newFilePath = GetNewSaveFilePath(nickName);
            File.WriteAllText(newFilePath, saveData.ToJson());
            Assert.IsTrue(File.Exists(newFilePath));

            Debug.Log($"Finished saving {nickName}! Trimming previous saves down to {BackupSaveSlots}...");
            TrimSaves(nickName);

            return saveData as T;
        }

        /// <summary>
        ///     Calls the static <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string,bool)" /> with this <see cref="SaveData{T}" />'s <see cref="nickName" />.
        ///     <br />
        /// </summary>
        /// <param name="useReSaveDelay">If <c>true</c>, check if <see cref="ReSaveDelay" /> has elapsed since <see cref="LastSaveTime" />.</param>
        /// <exception cref="ReSaveDelayException{t}">If <paramref name="useReSaveDelay" /> is <c>true</c> and <see cref="ReSaveDelay" /> hasn't elapsed since <see cref="LastSaveTime" />.</exception>
        public void Save(bool useReSaveDelay = true) {
            Save(this, nickName, useReSaveDelay);
        }

        public static void TrimSaves(string nickName, int trimTo = BackupSaveSlots) {
            Debug.Log($"Trimming save files named '{nickName}' down to {trimTo} saves");
            var saveFiles = GetAllSaveFilePaths(nickName);
            if (saveFiles.Length <= trimTo) {
                Debug.Log($"There were {saveFiles.Length} files, which is less than the requested trim count of {trimTo}, so we aren't going to trim anything.");
                return;
            }

            //A for loop is used here instead of a while loop in order to guard against infinite loops (basically, while loops scare me)
            var toDelete = saveFiles.Length - trimTo;
            for (var i = 0; i < toDelete; i++) {
                Delete(GetAllSaveFilePaths(nickName).First());
                saveFiles = GetAllSaveFilePaths(nickName);
            }
        }

        /// <summary>
        ///     Returns the <b><see cref="string" /> paths</b> to all of the save files for the given <c>nickName</c>.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static string[] GetAllSaveFilePaths(string nickName = "") {
            var saveFiles = Directory.GetFiles(SaveFolderPath, $"{nickName}*{SaveFileExtension}");
            SortSaveFilePaths(saveFiles);
            return saveFiles;
        }

        /// <summary>
        ///     Sorts the given save file paths<b><i>CHRONOLOGICALLY</i></b>by their <see cref="GetSaveDate" />.
        /// </summary>
        /// <param name="saveFilePaths"></param>
        private static void SortSaveFilePaths(string[] saveFilePaths) {
            Array.Sort(saveFilePaths, (save1, save2) => GetSaveDate(save1).CompareTo(GetSaveDate(save2)));
        }

        public static string GetSaveFileNameWithDate(string nickName, DateTime saveDate) {
            return nickName + "_" + GetTimeStamp(saveDate);
        }

        /// <summary>
        ///     Converts a <see cref="DateTime" /> to a standardized, file-name-friendly "Time Stamp".
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime" /> to be converted.</param>
        /// <returns>A file-name-friendly "Time Stamp" string.</returns>
        /// <seealso cref="TimeStampLength" />
        /// <seealso cref="TimeStampPattern" />
        public static string GetTimeStamp(DateTime dateTime) {
            return dateTime.Ticks.ToString().PadLeft(18, '0');
        }

        public static string GetNickname(string saveFileName) {
            return Regex.Match(saveFileName, SaveFilePattern).Groups["nickName"].Value;
        }

        public static DateTime GetSaveDate(string saveFileName) {
            var dateString = Regex.Match(saveFileName, SaveFilePattern).Groups["date"].Value;

            try {
                var saveDate = new DateTime(long.Parse(dateString));
                return saveDate;
            }
            catch (Exception e) {
                throw new SaveDataException<T>($"Could not parse the time stamp from {nameof(saveFileName)} {saveFileName}!" + $"\n\t{nameof(dateString)} was extracted as: [{dateString}]", e);
            }
        }

        public static bool Delete(string nickName) {
            if (File.Exists(GetNewSaveFilePath(nickName))) {
                Debug.LogWarning("About to delete the save file: " + nickName + "!!");
                File.Delete(GetNewSaveFilePath(nickName));
                return true;
            }

            Debug.Log("Can't delete the save file " + nickName + " because it doesn't exist!");
            return false;
        }

        /// <summary>
        ///     The canon way to convert a <see cref="SaveData{T}"/> to a json.
        /// </summary>
        /// <returns></returns>
        public string ToJson() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override string ToString() {
            return ToJson();
        }
    }
}