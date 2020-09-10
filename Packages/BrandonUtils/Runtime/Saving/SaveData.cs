using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Logging;
using Packages.BrandonUtils.Runtime.Timing;
using UnityEngine;
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
        [JsonIgnore]
        public const string SaveFolderName = "SaveData";

        [JsonIgnore]
        public const string AutoSaveName = "AutoSave";

        [JsonIgnore]
        public const string SaveFileExtension = "sav";

        [JsonIgnore]
        public const int BackupSaveSlots = 10;

        [JsonIgnore]
        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            Formatting             = Formatting.Indented
        };

        /// <summary>
        ///     The required length of timestamps in save file names generated via <see cref="GetSaveFileNameWithDate" />
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

        [JsonProperty]
        public DateTime LastSaveTime { get; set; } = RealTime.Now;

        [JsonIgnore]
        public string[] AllSaveFilePaths => GetAllSaveFilePaths(nickName);

        [JsonIgnore]
        public string LatestSaveFilePath => GetLatestSaveFilePath(nickName);

        [JsonIgnore]
        public string OldestSaveFilePath => GetOldestSaveFilePath(nickName);

        [JsonIgnore]
        public bool Exists => SaveFileExists(nickName);

        /// <summary>
        /// The time that this <see cref="SaveData{T}"/> was loaded.
        /// </summary>
        /// <remarks>
        /// Set to <see cref="RealTime.Now"/> when the data is initialized, <see cref="Load"/>-ed, or <see cref="Reload"/>-ed.
        /// </remarks>
        [JsonIgnore]
        public DateTime LastLoadTime { get; set; } = RealTime.Now;

        /// <summary>
        ///     Static initializer that makes sure the <see cref="SaveFolderPath" /> exists.
        /// </summary>
        static SaveData() {
            if (!Directory.Exists(SaveFolderPath)) {
                Log(Color.yellow, $"{nameof(SaveFolderPath)} at {SaveFolderPath} didn't exist, so it is being created...");
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        protected SaveData() { }

        public static T Load(string nickName) {
            Log($"Loading save file: {nickName}");
            if (!SaveFileExists(nickName)) {
                throw new SaveDataException<T>($"Attempt to load {typeof(T)} failed: No save files with the nickname {nickName} exist!");
            }

            var latestSaveFilePath = GetAllSaveFilePaths(nickName).Last();
            Log($"Found latest save file for {nickName} at path: {latestSaveFilePath}");

            var deserializedSaveFile = DeserializeByPath(latestSaveFilePath);
            deserializedSaveFile.OnLoadPrivate();
            return deserializedSaveFile;
        }

        public static T LoadByPath(string path) {
            Log($"Loading save file at path: {path}");
            var deserializedSaveFile = DeserializeByPath(path);
            deserializedSaveFile.OnLoadPrivate();
            return deserializedSaveFile;
        }

        /// <summary>
        /// Loads the most recent version of the save file.
        /// </summary>
        /// <remarks>
        /// This utilizes <see cref="JsonConvert.PopulateObject(string,object)"/> rather than <see cref="JsonConvert.DeserializeObject(string)"/>.
        /// <p/>
        /// <see cref="JsonConvert.DeserializeObject(string)"/> has wrappers that throw <see cref="SaveDataException{T}"/>s - <see cref="DeserializeByPath"/>, etc. - so I considered creating analogous methods for <see cref="JsonConvert.PopulateObject(string,object)"/>, e.g. "PopulateByPath".
        /// <p/>
        /// However, the intricacies of <see cref="JsonConvert.PopulateObject(string,object)"/> - for example, why is it able to populate the <c>target</c> object without using a <see langword="ref"/> parameter - didn't seem practical to tease out.
        /// </remarks>
        /// <returns></returns>
        public T Reload() {
            Log($"Reloading save file: {nickName}");
            JsonConvert.PopulateObject(GetSaveFileContent(LatestSaveFilePath), this);
            OnLoadPrivate();
            return (T) this;
        }

        private static T DeserializeByContent(string saveFileContent) {
            try {
                return JsonConvert.DeserializeObject<T>(saveFileContent);
            }
            catch (JsonException e) {
                throw new SaveDataException<T>(
                    $"Unable to {nameof(DeserializeByContent)} the provided {nameof(saveFileContent)}!\n\tContent:{saveFileContent}\n",
                    e
                );
            }
        }

        private static string GetSaveFileContent(string saveFilePath) {
            try {
                return File.ReadAllText(saveFilePath);
            }
            catch (FileNotFoundException e) {
                throw new SaveDataException<T>($"No save file exists at the path {saveFilePath}");
            }
        }

        private static T DeserializeByPath(string saveFilePath) {
            try {
                return DeserializeByContent(GetSaveFileContent(saveFilePath));
            }
            catch (FileNotFoundException e) {
                throw new SaveDataException<T>($"No save file exists to {nameof(DeserializeByPath)} at path {saveFilePath}", e);
            }
        }

        /// <summary>
        /// Non-overrideable method called whenever a <see cref="Load"/> (or related method, like <see cref="Reload"/>) is called.
        /// </summary>
        /// <remarks>
        /// This contains logic that <b>must never be overriden</b> - preventing errors in case an inheritor forgets to call <c>base.OnLoad()</c> in their overload of <see cref="OnLoad"/>.
        /// </remarks>
        /// <seealso cref="OnLoad"/>
        private void OnLoadPrivate() {
            LastLoadTime = RealTime.Now;
            OnLoad();
        }

        /// <summary>
        /// Overrideable method called whenever a <see cref="Load"/> (or related method, like <see cref="Reload"/>) is called.
        /// </summary>
        protected virtual void OnLoad() {
            //no-op
        }

        /// <summary>
        ///     Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and <see cref="DateTime" />-stamp.
        ///     <br />
        ///     This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public static string GetSaveFilePath(string nickName, DateTime dateTime) {
            return Path.ChangeExtension(Path.Combine(SaveFolderPath, GetSaveFileNameWithDate(nickName, dateTime)), SaveFileExtension);
        }

        /// <summary>
        ///     Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and a <see cref="DateTime" />-stamp of <see cref="RealTime.Now" /> via <see cref="GetSaveFilePath" />.
        ///     <br />
        ///     This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public static string GetNewSaveFilePath(string nickName) {
            return GetSaveFilePath(nickName, RealTime.Now);
        }

        [UsedImplicitly]
        public static bool SaveFileExists(string nickName) {
            return GetAllSaveFilePaths(nickName).Any(File.Exists);
        }

        /// <summary>
        ///     Creates a new, blank <see cref="SaveData{T}" /> of type <see cref="T" />, and <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string,bool)" />s it as a new file with the <see cref="nickName" /> <paramref name="nickname" />.
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns>the newly created <see cref="SaveData{T}" /></returns>
        public static T NewSaveFile(string nickname) {
            Log($"Creating a new save file: {nickname} ({GetNewSaveFilePath(nickname)}), of type {typeof(T)}");
            //create the save folder if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(GetNewSaveFilePath(nickname)) ?? throw new SaveDataException<T>($"The path {GetNewSaveFilePath(nickname)} didn't have a valid directory name!", new DirectoryNotFoundException()));

            //create a new, blank save data, and save it as the new file
            return Save(new T(), nickname, false);
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

            var previousFileCount = saveData.AllSaveFilePaths.Length;

            var saveTime = RealTime.Now;

            //throw an error if ReSaveDelay hasn't elapsed since the last time the file was saved
            if (useReSaveDelay && saveTime - saveData.LastSaveTime < ReSaveDelay) {
                throw new ReSaveDelayException<T>(
                    saveData,
                    $"The save file {nickName} was saved too recently!" +
                    $"\n\t{nameof(saveData.LastSaveTime)}: {saveData.LastSaveTime}" +
                    $"\n\tNew {nameof(saveTime)}: {saveTime}" +
                    $"\n\t{nameof(ReSaveDelay)}: {ReSaveDelay}" +
                    $"\n\tDelta: {saveTime - saveData.LastSaveTime}"
                );
            }

            saveData.nickName     = nickName;
            saveData.LastSaveTime = saveTime;

            var newFilePath = GetNewSaveFilePath(nickName);

            //Make sure that the file we're trying to create doesn't already exist
            if (File.Exists(newFilePath)) {
                throw new SaveDataException<T>(saveData, $"Couldn't save {nickName} because there was already a save file at the path {newFilePath}");
            }

            //Write to the new save file
            File.WriteAllText(newFilePath, saveData.ToJson());
            FileAssert.Exists(newFilePath);

            if (GetAllSaveFilePaths(nickName).Length <= previousFileCount) {
                throw new SaveDataException<T>(saveData, $"When saving {nickName}, we failed to create a new file!");
            }

            Log($"Finished saving {nickName}! Trimming previous saves down to {BackupSaveSlots}...");
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

        /// <summary>
        /// Overrideable method called whenever the file is <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string,bool)"/>ed.
        /// </summary>
        protected virtual void OnSave() {
            //no-op
        }

        public static void TrimSaves(string nickName, int trimTo = BackupSaveSlots) {
            LogUtils.Log($"Trimming save files named '{nickName}' down to {trimTo} saves");
            var saveFiles = GetAllSaveFilePaths(nickName);
            if (saveFiles.Length <= trimTo) {
                LogUtils.Log($"There were {saveFiles.Length} files, which is less than the requested trim count of {trimTo}, so we aren't going to trim anything.");
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
        ///     Returns the <b><see cref="string" /> paths</b> to all of the save files for the given <paramref name="nickName"/> that <b>currently exist</b>.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static string[] GetAllSaveFilePaths(string nickName) {
            //This used to use "" as a default value for nickName, and I don't know why...
            var saveFiles = Directory.GetFiles(SaveFolderPath, $"{nickName}*{SaveFileExtension}");
            SortSaveFilePaths(saveFiles);
            return saveFiles;
        }

        public static string GetLatestSaveFilePath(string nickName) {
            var allPaths = GetAllSaveFilePaths(nickName);
            if (allPaths.Length == 0) {
                throw new SaveDataException<T>($"Unable to retrieve the latest save file path because no files exist with the {nameof(SaveData<T>.nickName)} {nickName}!");
            }

            return allPaths.Last();
        }

        public static string GetOldestSaveFilePath(string nickName) {
            var allPaths = GetAllSaveFilePaths(nickName);
            if (allPaths.Length == 0) {
                throw new SaveDataException<T>($"Unable to retrieve the oldest save file path because no files exist with the {nameof(SaveData<T>.nickName)} {nickName}!");
            }

            return allPaths.First();
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
                Log(Color.yellow, "About to delete the save file: " + nickName + "!!");
                File.Delete(GetNewSaveFilePath(nickName));
                return true;
            }

            Log("Can't delete the save file " + nickName + " because it doesn't exist!");
            return false;
        }

        /// <summary>
        ///     The canon way to convert a <see cref="SaveData{T}"/> to a json.
        /// </summary>
        /// <returns></returns>
        public string ToJson() {
            return JsonConvert.SerializeObject(this, JsonSerializerSettings);
        }

        public override string ToString() {
            return ToJson();
        }
    }
}