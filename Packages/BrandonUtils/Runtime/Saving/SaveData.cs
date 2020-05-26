using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Packages.BrandonUtils.Runtime.Saving {
    /// <summary>
    /// A single "Save File", containing data
    /// </summary>
    /// <remarks>
    ///     <para>Implementations will serialize all <see cref="SerializeField"/>s in both <see cref="SaveData{T}"/> and the inheritor, such as <see cref="SaveDataTestImpl"/>.</para>
    ///     <para>Uses the <a href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">Curiously Repeating Template Pattern</a>, where <typeparamref name="T"/> references the inheritor type, e.g. <see cref="SaveDataTestImpl"/>.</para>
    /// </remarks>
    /// <typeparam name="T">The inheriting class, e.g. <see cref="SaveDataTestImpl"/></typeparam>
    /// <seealso cref="SaveDataTestImpl"/>
    public abstract class SaveData<T> where T : SaveData<T>, new() {
        /// <summary>
        ///     The time of the last <see cref="Save"/>
        /// </summary>
        [SerializeField] private long lastSaveTime;

        public DateTime LastSaveTime {
            get => new DateTime(lastSaveTime);
            private set => lastSaveTime = value.Ticks;
        }

        [SerializeField] private long saveCreatedTime;

        private DateTime SaveCreatedTime {
            get => new DateTime(saveCreatedTime);
            set => saveCreatedTime = value.Ticks;
        }

        public string nickName;

        public string[] AllSaveFilePaths => GetAllSaveFilePaths(this.nickName);
        public string LatestSaveFilePath => GetLatestSaveFilePath(this.nickName);
        public bool Exists => SaveFileExists(this.nickName);

        public const string SaveFolderName = "SaveData";
        public const string AutoSaveName = "AutoSave";
        public const string SaveFileExtension = "sav";
        public const int BackupSaveSlots = 10;
        public const string DateFormat = "MMddyyyy_HHmmss";
        public const string DatePattern = @"\d{8}_\d{6}";
        public static readonly string SaveFilePattern = $@"(?<nickName>.*)_(?<date>{DatePattern})";
        public static readonly string SaveFolderPath = Path.Combine(Application.persistentDataPath, SaveFolderName);
        public static readonly TimeSpan ReSaveDelay = TimeSpan.FromSeconds(1);
        public const int LoadRetryLimit = 10;

        /// <summary>
        /// Static initializer that makes sure the <see cref="SaveFolderPath"/> exists.
        /// </summary>
        static SaveData() {
            if (!Directory.Exists(SaveFolderPath)) {
                Debug.LogWarning(
                    $"{nameof(SaveFolderPath)} at {SaveFolderPath} didn't exist, so it is being created...");
                Directory.CreateDirectory(SaveFolderPath);
            }
        }

        protected SaveData() {
            this.SaveCreatedTime = DateTime.Now;
        }

        public static T Load(string nickName) {
            Debug.Log("Loading save file: " + nickName);
            int attemptCount = 0;
            while (!SaveFileExists(nickName)) {
                if (attemptCount == LoadRetryLimit) {
                    throw new SaveDataException<T>($"Unable to load the any save files for {nickName} after {attemptCount} attempts!");
                }
                else {
                    attemptCount++;
                }

                Debug.LogWarning($"[Attempt {attemptCount}] No save files for {nickName} exist! Attempting to create a new one...");
                NewSaveFile(nickName);
            }

            return FromSaveFile(GetLatestSaveFilePath(nickName));
        }

        private static T FromSaveFile(string saveFilePath) {
            return JsonUtility.FromJson<T>(File.ReadAllText(saveFilePath));
        }

        /// <summary>
        /// Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and <see cref="DateTime"/>-stamp.
        /// <br/>
        /// This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private static string GetSaveFilePath(string nickName, DateTime dateTime) {
            return Path.ChangeExtension(
                Path.Combine(
                    SaveFolderPath,
                    GetSaveFileNameWithDate(nickName, dateTime)
                ), SaveFileExtension
            );
        }

        /// <summary>
        /// Gets the path to a <b>theoretical</b> save file with the given <c>nickName</c> and a <see cref="DateTime"/>-stamp of <see cref="DateTime.Now"/> via <see cref="GetSaveFilePath"/>.
        /// <br/>
        /// This method does <b>not</b> know or care if the save file exists!
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        private static string GetNewSaveFilePath(string nickName) {
            return GetSaveFilePath(nickName, DateTime.Now);
        }

        public static bool SaveFileExists(string nickName) {
            return GetAllSaveFilePaths(nickName).Any(File.Exists);
        }

        /// <summary>
        /// Creates a new, blank <see cref="SaveData{T}"/> of type <see cref="T"/>, and <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string)"/>s it as a new file with the <see cref="nickName"/> "<c>nickname</c>".
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns>the newly created <see cref="SaveData{T}"/></returns>
        public static T NewSaveFile(string nickname) {
            Debug.Log("Creating a new save file: " + nickname + " (" + GetNewSaveFilePath(nickname) + "), of type " +
                      typeof(T));
            //create the save folder if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(GetNewSaveFilePath(nickname)) ??
                                      throw new SaveDataException<T>(
                                          $"The path {GetNewSaveFilePath(nickname)} didn't have a valid directory name!",
                                          new DirectoryNotFoundException()));

            //create a new, blank save data, and save it to as the new file
            return Save(new T(), nickname);
        }

        /// <summary>
        ///     Serializes <paramref name="saveData"/> to a new <see cref="File"/>.
        /// </summary>
        /// <remarks>
        ///     <para>The new file will be located at <see cref="GetSaveFilePath(string,System.DateTime)"/>.</para>
        ///     <para>Retains previous saves with the same <paramref name="nickName"/>, up to <see cref="BackupSaveSlots"/>, via <see cref="TrimSaves"/>.</para>
        ///     <para>May update fields in <paramref name="saveData"/>, such as <see cref="LastSaveTime"/>.</para>
        /// </remarks>
        /// <param name="saveData">The <see cref="SaveData{T}"/> inheritor to be saved.</param>
        /// <param name="nickName">The <see cref="nickName"/> that the <see cref="saveData"/> should be given.</param>
        /// <returns>The passed <paramref name="saveData"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="SaveDataException{T}"></exception>
        private static T Save([NotNull] SaveData<T> saveData, [NotNull] string nickName) {
            if (saveData == null) throw new ArgumentNullException(nameof(saveData));
            if (string.IsNullOrWhiteSpace(nickName)) {
                var argException = new ArgumentException("Value cannot be null or whitespace.", nameof(nickName));
                throw new SaveDataException<T>(saveData,
                                               $"The name of the file you tried to save was '{nickName}', which is null, blank, or whitespace, so we can't save it!",
                                               argException);
            }

            var saveTime = DateTime.Now;
            if (saveTime - saveData.LastSaveTime < ReSaveDelay) {
                throw new SaveDataException<T>(saveData,
                                               $"The save file {nickName} was saved too recently!" +
                                               $"\n\t{nameof(saveData.LastSaveTime)}: {saveData.LastSaveTime}" +
                                               $"\n\tNew saveTime: {saveTime}" +
                                               $"\n\t{nameof(ReSaveDelay)}: {ReSaveDelay}" +
                                               $"\n\tDelta: {saveTime - saveData.LastSaveTime}");
            }

            saveData.nickName = nickName;
            saveData.LastSaveTime = saveTime;

            string newFilePath = GetNewSaveFilePath(nickName);
            File.WriteAllText(newFilePath, saveData.ToJson());
            Assert.IsTrue(File.Exists(newFilePath));

            Debug.Log($"Finished saving {nickName}! Trimming previous saves down to {BackupSaveSlots}...");
            TrimSaves(nickName);

            return saveData as T;
        }

        /// <summary>
        ///     Calls the static <see cref="Save(Packages.BrandonUtils.Runtime.Saving.SaveData{T},string)"/> with this <see cref="SaveData{T}"/>'s <see cref="nickName"/>.
        /// </summary>
        public void Save() {
            Save(this, this.nickName);
        }

        public static void TrimSaves(string nickName, int trimTo = BackupSaveSlots) {
            Debug.Log($"Trimming save files named '{nickName}' down to {trimTo} saves");
            var saveFiles = GetAllSaveFilePaths(nickName);
            if (saveFiles.Length <= trimTo) {
                Debug.Log(
                    $"There were {saveFiles.Length} files, which is less than the requested trim count of {trimTo}, so we aren't going to trim anything.");
                return;
            }

            for (int i = trimTo; i < saveFiles.Length; i++) {
                Debug.Log($"Trimming the save file at index [{i}]: {saveFiles[i]}");
                Delete(saveFiles[i]);
            }
        }

        /// <summary>
        /// Returns the <b><see cref="string"/> paths</b> to all of the save files for the given <c>nickName</c>.
        /// </summary>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public static string[] GetAllSaveFilePaths(string nickName = "") {
            var saveFiles = Directory.GetFiles(SaveFolderPath, $"{nickName}*{SaveFileExtension}");
            SortSaveFilePaths(saveFiles);
            return saveFiles;
        }

        private static void SortSaveFilePaths(string[] saveFiles) {
            Array.Sort(saveFiles, (save1, save2) => GetSaveDate(save1).CompareTo(GetSaveDate(save2)));
        }

        public static string GetLatestSaveFilePath(string nickName = "") {
            return GetAllSaveFilePaths(nickName)[0];
        }

        public static string GetSaveFileNameWithDate(String saveFile, DateTime saveDate) {
            return saveFile + "_" + saveDate.ToString(DateFormat);
        }

        public static string GetNickname(string saveFile) {
            return Regex.Match(saveFile, SaveFilePattern).Groups["nickName"].Value;
        }

        public static DateTime GetSaveDate(string saveFile) {
            string dateString = Regex.Match(saveFile, SaveFilePattern).Groups["date"].Value;
            DateTime saveDate = DateTime.ParseExact(dateString, DateFormat, new DateTimeFormatInfo());
            return saveDate;
        }

        public static bool Delete(string nickName) {
            if (File.Exists(GetNewSaveFilePath(nickName))) {
                Debug.LogWarning("About to delete the save file: " + nickName + "!!");
                File.Delete(GetNewSaveFilePath(nickName));
                return true;
            }
            else {
                Debug.Log("Can't delete the save file " + nickName + " because it doesn't exist!");
                return false;
            }
        }

        /// <summary>
        /// Contains the serialization of the <see cref="SaveData{T}"/>.
        ///
        /// While this is currently a simple <see cref="JsonUtility.ToJson(object)"/>, putting it inside of a method ensures that if/when this ever changes, everything is using the same method.
        /// </summary>
        /// <returns></returns>
        public string ToJson() {
            return JsonUtility.ToJson(this, true);
        }

        public override string ToString() {
            return ToJson();
        }
    }
}