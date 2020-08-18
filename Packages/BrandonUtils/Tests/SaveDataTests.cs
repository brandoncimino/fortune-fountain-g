using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Saving;
using Packages.BrandonUtils.Runtime.Testing;
using UnityEngine;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Packages.BrandonUtils.Tests {
    public class SaveDataTests {
        const  string       DummyNickName = "DummySaveFile";
        static List<string> DummySaveFiles;

        static readonly Dictionary<string, DateTime> DummySaveDates = new Dictionary<string, DateTime>() {
            [DummyNickName + "_000000000000000000"] = new DateTime(1,    1,  1,  0,  0,  0,  0),
            [DummyNickName + "_000000036610010000"] = new DateTime(1,    1,  1,  1,  1,  1,  1),
            [DummyNickName + "_628771195208760000"] = new DateTime(1993, 7,  1,  10, 32, 0,  876),
            [DummyNickName + "_628771627200000000"] = new DateTime(1993, 7,  1,  22, 32, 0,  0),
            [DummyNickName + "_628771627200010000"] = new DateTime(1993, 7,  1,  22, 32, 0,  1),
            [DummyNickName + "_630822815999990000"] = new DateTime(1999, 12, 31, 23, 59, 59, 999),
            [DummyNickName + "_630822816000000000"] = new DateTime(2000, 1,  1,  0,  0,  0,  0),
            [DummyNickName + "_630877604740270000"] = new DateTime(2000, 3,  4,  9,  54, 34, 27),
            [DummyNickName + "_637082333146780000"] = new DateTime(2019, 11, 1,  19, 28, 34, 678),
            [DummyNickName + "_637082333151010000"] = new DateTime(2019, 11, 1,  19, 28, 35, 101),
            [DummyNickName + "_637263971400020000"] = new DateTime(2020, 5,  30, 0,  59, 0,  2),
            [DummyNickName + "_645217199999990000"] = new DateTime(2045, 08, 12, 3,  59, 59, 999),
        };

        [OneTimeSetUp]
        public void SetUp() {
            Log("Beginning one-time setup...");
            Log("persistent data path: " + Application.persistentDataPath);
            DummySaveFiles = MakeDummyFiles();
        }

        private static List<string> GetExistingSaveFiles(string nickName = DummyNickName) {
            return Directory.GetFiles(SaveDataTestImpl.SaveFolderPath, $"{nickName}*{SaveDataTestImpl.SaveFileExtension}").ToList();
        }

        [Test]
        public void TestGenerateAutoSaveName() {
            foreach (var saveDate in DummySaveDates) {
                Log("Parsed to: " + SaveDataTestImpl.GetSaveFileNameWithDate(DummyNickName, saveDate.Value));
                Assert.AreEqual(saveDate.Key, SaveDataTestImpl.GetSaveFileNameWithDate(DummyNickName, saveDate.Value));
            }
        }

        [Test]
        public void TestParseAutoSaveName() {
            foreach (var saveDate in DummySaveDates) {
                Assert.AreEqual(saveDate.Value, SaveDataTestImpl.GetSaveDate(saveDate.Key));
            }
        }

        private static string MakeDummyFile(string fileName) {
            Log("Creating dummy file: " + fileName);
            string newFilePath = Path.ChangeExtension(Path.Combine(SaveDataTestImpl.SaveFolderPath, fileName), SaveDataTestImpl.SaveFileExtension);

            //create the save folder if it doesn't already exist:
            Directory.CreateDirectory(SaveDataTestImpl.SaveFolderPath);

            File.WriteAllText(newFilePath, "I am a dummy test save with the name " + fileName);
            Assume.That(File.Exists(newFilePath), "The file " + newFilePath + " doesn't exist!");
            return newFilePath;
        }

        private static void DeleteSaveFiles(string nickName = DummyNickName) {
            Log($"Clearing old save files named {nickName}...");
            GetExistingSaveFiles(nickName).ForEach(File.Delete);
            Assume.That(GetExistingSaveFiles(nickName), Is.Empty, "There were still dummy files left after we tried to delete them all!");
        }

        /*
         * TODO: This will remove the ENTIRE SAVE FOLDER!!!
         *     It should be updated to instead RENAME the old folder and then rename it BACK after testing is completed!!
         */
        private static void NukeSaveFolder() {
            if (Directory.Exists(SaveDataTestImpl.SaveFolderPath)) {
                Log(Color.red, $"Removing the entire save folder at {SaveDataTestImpl.SaveFolderPath}");
                Directory.Delete(SaveDataTestImpl.SaveFolderPath, true);
            }
            else {
                Log(Color.green, $"No need to nuke the save folder, because it didn't exist at {SaveDataTestImpl.SaveFolderPath}");
            }
        }

        static List<string> MakeDummyFiles() {
            Log($"About to create dummy save files, which are EMPTY (in retrospect, I probably should've named them \"empty save files\" instead...)");
            NukeSaveFolder();
            var allSaves = new List<string>();
            foreach (var saveDate in DummySaveDates) {
                allSaves.Add(MakeDummyFile(saveDate.Key));
            }

            Log("Finished creating dummy save files:\n" + string.Join("\n", allSaves));
            Assume.That(GetExistingSaveFiles().Count, Is.EqualTo(DummySaveDates.Count), "Didn't generate the proper number of save files!");
            return allSaves;
        }

        [Test]
        public void TestSaveFilePathsAreSortedChronologically() {
            var loadedSaves = SaveDataTestImpl.GetAllSaveFilePaths(DummyNickName);
            Assert.AreNotEqual(0, loadedSaves.Length, "No saves were actually loaded!");
            TestUtils.AreEqual(DummySaveFiles, loadedSaves);
        }

        [Test]
        public void TestGetNickname() {
            foreach (var saveDate in DummySaveDates) {
                Assert.AreEqual(SaveDataTestImpl.GetNickname(saveDate.Key), DummyNickName);
            }
        }

        [Test(TestOf = typeof(SaveData<>))]
        public void TestTrimSaveFiles() {
            var testTrimValues = new List<int>() {0, 1, 5, 10};
            foreach (var t in testTrimValues) {
                _TestTrimSaveFiles(t);
            }
        }

        private static void _TestTrimSaveFiles(int trimTo) {
            Assume.That(trimTo, Is.LessThanOrEqualTo(DummySaveDates.Count));
            MakeDummyFiles();
            SaveDataTestImpl.TrimSaves(DummyNickName, trimTo);
            Assert.That(GetExistingSaveFiles().Count, Is.EqualTo(trimTo), "The incorrect number of files remained after trimming!");
        }

        [Test]
        public void TestBackupSaveSlots() {
            const string nickName = nameof(TestBackupSaveSlots);
            DeleteSaveFiles(nickName);
            var newSave = SaveDataTestImpl.NewSaveFile(nameof(TestBackupSaveSlots));

            for (int numberOfSaveFiles = 1; numberOfSaveFiles < SaveDataTestImpl.BackupSaveSlots * 2; numberOfSaveFiles++) {
                Thread.Sleep(SaveDataTestImpl.ReSaveDelay);
                newSave.Save();
                Log($"Created new save file:[{numberOfSaveFiles}] {newSave}");
                Assert.AreEqual(Math.Min(numberOfSaveFiles + 1, SaveDataTestImpl.BackupSaveSlots), SaveDataTestImpl.GetAllSaveFilePaths(newSave.nickName).Length, $"Didn't find the correct number of saves!\n\t{string.Join("\n\t", SaveDataTestImpl.GetAllSaveFilePaths(newSave.nickName))}");
            }
        }

        /// <summary>
        /// Tests that the save file is serialized with the <see cref="DateTime.Ticks"/> of the save time, not the <see cref="DateTime"/>.
        /// </summary>
        [Test]
        public void TestSerializeLastSaveTime() {
            var newSave  = SaveDataTestImpl.NewSaveFile(nameof(TestSerializeLastSaveTime));
            var saveJson = JsonUtility.ToJson(newSave, true);
            Log($"{nameof(saveJson)}: {saveJson}");
            Assert.IsTrue(saveJson.Contains($"\"lastSaveTime\": {newSave.LastSaveTime.Ticks}"));
        }

        [Test]
        public void TestToStringMatchesToJson() {
            var newSave    = SaveDataTestImpl.NewSaveFile(MethodBase.GetCurrentMethod().Name);
            var saveJson   = JsonUtility.ToJson(newSave, true);
            var saveString = newSave.ToString();
            Assert.That(saveString, Is.EqualTo(saveJson));
        }

        [Test]
        public void TestLoadMostRecentSaveFile() {
            //save a few files
            int    saveCount = 3;
            string nickName  = nameof(TestLoadMostRecentSaveFile);

            //create a new save file with the desired nickname
            SaveDataTestImpl saveData = SaveDataTestImpl.NewSaveFile(nickName);
            for (int i = 0; i < saveCount; i++) {
                if (i != 0) {
                    Log($"Waiting {SaveDataTestImpl.ReSaveDelay} before continuing...");
                }

                //add a unique value into the save data
                saveData.Word = $"SAVE_{i}";

                //re-save the save data
                saveData.Save(false);

                Log($"Saved {nickName} #{i}:\n{saveData}");

                //Assert that the timestamp in the filename matches the lastsavetime
                Assert.That(saveData.LastSaveTime, Is.EqualTo(SaveDataTestImpl.GetSaveDate(saveData.LatestSaveFilePath)));

                //load the save data and check the unique value
                Assert.That(SaveDataTestImpl.Load(nickName).Word, Is.EqualTo(saveData.Word));
            }
        }

        [Test]
        public void TestUseReSaveDelay() {
            SaveDataTestImpl saveDataTestImpl = SaveDataTestImpl.NewSaveFile(nameof(TestUseReSaveDelay));

            var message = $"{nameof(ReSaveDelayException<SaveDataTestImpl>)} when saved again within the {nameof(SaveDataTestImpl.ReSaveDelay)} ({SaveDataTestImpl.ReSaveDelay})!";

            try {
                saveDataTestImpl.Save(true);
            }
            catch (ReSaveDelayException<SaveDataTestImpl> e) {
                Assert.Pass($"Properly threw a {message}\nException: {e.Message}");
            }

            Assert.Fail($"Did NOT throw a {message}");
        }

        [Test]
        public void TestIgnoreReSaveDelay() {
            SaveDataTestImpl saveDataTestImpl = new SaveDataTestImpl {nickName = nameof(TestIgnoreReSaveDelay)};

            for (int i = 0; i < 5; i++) {
                try {
                    saveDataTestImpl.Save(false);
                }
                catch (ReSaveDelayException<SaveDataTestImpl> e) {
                    throw new AssertionException($"When useReSaveDelay is FALSE, we shouldn't throw a {nameof(ReSaveDelayException<SaveDataTestImpl>)}, but we threw {e.Message}!", e);
                }
            }
        }
    }
}