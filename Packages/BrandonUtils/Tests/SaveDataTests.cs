using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Saving;
using Packages.BrandonUtils.Runtime.Testing;
using UnityEngine;
using UnityEngine.TestTools;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Packages.BrandonUtils.Tests {
    public class SaveDataTests {
        private const  string       DummyNickName = "DummySaveFile";
        private static List<string> DummySaveFiles;

        private static readonly Dictionary<string, DateTime> DummySaveDates = new Dictionary<string, DateTime>() {
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
            Log($"About to create {nameof(DummySaveFiles)}, which are EMPTY (in retrospect, I probably should've named them \"empty save files\" instead...)");
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

            var newSave = new SaveDataTestImpl {nickName = nickName};

            Assume.That(newSave.AllSaveFilePaths, Is.Empty);

            for (int numberOfSaveFiles = 1; numberOfSaveFiles < SaveDataTestImpl.BackupSaveSlots * 2; numberOfSaveFiles++) {
                newSave.Save(false);

                Log($"Created new save file:[{numberOfSaveFiles}] {newSave}");

                Assert.That(newSave.AllSaveFilePaths.Length, Is.LessThanOrEqualTo(SaveDataTestImpl.BackupSaveSlots), $"There should never be more than {SaveDataTestImpl.BackupSaveSlots} save files!");

                Assert.That(
                    newSave.AllSaveFilePaths.Length,
                    Is.EqualTo(
                        Math.Min(numberOfSaveFiles, SaveDataTestImpl.BackupSaveSlots)
                    ),
                    $"Didn't find the correct number of saves!" +
                    $"\n\t{string.Join("\n\t", SaveDataTestImpl.GetAllSaveFilePaths(newSave.nickName))}"
                );
            }
        }

        [Test]
        public void TestToStringMatchesToJson() {
            var newSave    = SaveDataTestImpl.NewSaveFile(MethodBase.GetCurrentMethod().Name);
            var saveJson   = newSave.ToJson();
            var saveString = newSave.ToString();
            Assert.That(saveString, Is.EqualTo(saveJson));
        }

        [UnityTest]
        public IEnumerator TestLoadMostRecentSaveFile() {
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
                yield return new WaitForSecondsRealtime(0.1f);
                saveData.Save(false);

                Log($"Saved {nickName} #{i}:\n{saveData}");

                //Assert that the timestamp in the filename matches the lastSaveTime
                Assert.That(
                    saveData.LastSaveTime_Exact.Ticks,
                    Is.EqualTo(
                        SaveDataTestImpl.GetSaveDate(saveData.LatestSaveFilePath).Ticks
                    ),
                    $"Incorrect timestamp pulled from path {saveData.LatestSaveFilePath}"
                );

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

        [Test]
        public void LoadMostRecentFromInstance() {
            var saveData  = SaveDataTestImpl.NewSaveFile(nameof(LoadMostRecentFromInstance));
            var firstEdit = "First Edit";
            saveData.Word = firstEdit;
            saveData.Save(false);

            var saveData2  = SaveDataTestImpl.NewSaveFile(saveData.nickName);
            var secondEdit = "Second Edit";
            saveData2.Word = secondEdit;
            saveData2.Save(false);

            Assert.That(saveData.Word, Is.EqualTo(firstEdit));
            Assert.That(saveData,      Is.Not.SameAs(saveData2));

            var loadedSave = saveData.Reload();

            Assert.That(loadedSave,    Is.SameAs(saveData));
            Assert.That(saveData.Word, Is.Not.EqualTo(firstEdit));
            Assert.That(saveData.Word, Is.EqualTo(secondEdit));
        }

        [UnityTest]
        public IEnumerator SaveTimeUnaffectedByLoading() {
            DateTime beforeNewSave = DateTime.Now;
            yield return null;

            var      saveData     = SaveDataTestImpl.NewSaveFile(nameof(SaveTimeUnaffectedByLoading));
            DateTime afterNewSave = DateTime.Now;

            Assert.That(saveData.LastSaveTime, Is.InRange(beforeNewSave, afterNewSave));
            Assert.That(saveData.LastLoadTime, Is.InRange(beforeNewSave, afterNewSave));

            var oldSaveTime = saveData.LastSaveTime;
            var oldLoadTime = saveData.LastLoadTime;

            yield return new WaitForSecondsRealtime(1);

            DateTime beforeReload = DateTime.Now;
            yield return null;

            saveData.Reload();
            DateTime afterReload = DateTime.Now;

            Assert.That(saveData.LastSaveTime, Is.EqualTo(oldSaveTime), $"The {nameof(SaveDataTestImpl.LastSaveTime)} should not have changed, because we {nameof(SaveDataTestImpl.Reload)}-ed without {nameof(SaveDataTestImpl.Save)}-ing!");

            Assert.That(saveData.LastLoadTime, Is.Not.EqualTo(oldLoadTime), $"The {nameof(SaveDataTestImpl.LastLoadTime)} should have changed, because we {nameof(SaveDataTestImpl.Reload)}-ed!");

            Assert.That(saveData.LastLoadTime, Is.InRange(beforeReload, afterReload));
        }

        [UnityTest]
        public IEnumerator LoadTimeUnaffectedBySaving() {
            var beforeNewSave = DateTime.Now;
            yield return null;

            var saveData     = SaveDataTestImpl.NewSaveFile(nameof(LoadTimeUnaffectedBySaving));
            var afterNewSave = DateTime.Now;

            Assert.That(saveData.LastSaveTime, Is.InRange(beforeNewSave, afterNewSave));
            Assert.That(saveData.LastLoadTime, Is.InRange(beforeNewSave, afterNewSave));

            var oldSaveTime = saveData.LastSaveTime;
            var oldLoadTime = saveData.LastLoadTime;

            yield return new WaitForSecondsRealtime(1);

            var beforeReSave = DateTime.Now;
            yield return null;

            saveData.Save(false);
            var afterReSave = DateTime.Now;

            Assert.That(saveData.LastSaveTime, Is.InRange(beforeReSave, afterReSave));
            Assert.That(saveData.LastSaveTime, Is.Not.EqualTo(oldSaveTime));
            Assert.That(saveData.LastLoadTime, Is.EqualTo(oldLoadTime));
        }

        [Test]
        public void LoadingMissingNickNameThrowsSaveDataException() {
            const string nickName = nameof(LoadingMissingNickNameThrowsSaveDataException);
            Assume.That(SaveDataTestImpl.GetAllSaveFilePaths(nickName), Is.Empty, $"Save files with the nickname {nickName} were found - please delete them, then run this test again.");

            Assert.Throws<SaveDataException<SaveDataTestImpl>>(() => SaveDataTestImpl.Load(nickName));
        }

        [Test]
        public void ReloadingMissingSavePathThrowsSaveDataException() {
            const string nickName = nameof(ReloadingMissingSavePathThrowsSaveDataException);
            var          saveData = new SaveDataTestImpl {nickName = nickName};
            Assume.That(
                saveData.Exists,
                Is.False,
                $"{nameof(saveData)} (with the {nameof(saveData.nickName)} {saveData.nickName}) .{nameof(saveData.Exists)} should be FALSE!"
            );

            Assume.That(
                SaveDataTestImpl.GetAllSaveFilePaths(nickName),
                Is.Empty,
                $"Save files with the nickname {nickName} were found - please delete them, then run this test again."
            );

            Assume.That(saveData.AllSaveFilePaths, Is.Empty);

            Assert.Throws<SaveDataException<SaveDataTestImpl>>(() => saveData.Reload());
        }

        [Test]
        public void LoadingInvalidJsonContentByPathThrowsSaveDataException() {
            Assert.Throws<SaveDataException<SaveDataTestImpl>>(() => SaveDataTestImpl.LoadByPath(DummySaveFiles[0]));
        }

        [Test]
        public void LoadingInvalidContentByNicknameThrowsSaveDataException() {
            Assert.Throws<SaveDataException<SaveDataTestImpl>>(() => SaveDataTestImpl.Load(DummyNickName));
        }
    }
}