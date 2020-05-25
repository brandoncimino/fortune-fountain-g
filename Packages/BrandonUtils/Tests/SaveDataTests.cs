using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Saving;
using UnityEngine;

namespace Packages.BrandonUtils.Tests
{
    public class SaveDataTests
    {
        const string DummyNickName = "DummySaveFile";
        static List<string> DummySaveFiles;

        static readonly Dictionary<string, DateTime> DummySaveDates = new Dictionary<string, DateTime>()
        {
            [DummyNickName + "_01010001_000000"] = new DateTime(1, 1, 1, 0, 0, 0),
            [DummyNickName + "_01010001_010101"] = new DateTime(1, 1, 1, 1, 1, 1),
            [DummyNickName + "_07011993_103200"] = new DateTime(1993, 7, 1, 10, 32, 0),
            [DummyNickName + "_07011993_223200"] = new DateTime(1993, 7, 1, 22, 32, 0),
            [DummyNickName + "_07011993_223201"] = new DateTime(1993, 7, 1, 22, 32, 1),
            [DummyNickName + "_12311999_235959"] = new DateTime(1999, 12, 31, 23, 59, 59),
            [DummyNickName + "_01012000_000000"] = new DateTime(2000, 1, 1, 0, 0, 0),
            [DummyNickName + "_03042000_095434"] = new DateTime(2000, 3, 4, 9, 54, 34),
            [DummyNickName + "_11012019_192834"] = new DateTime(2019, 11, 1, 19, 28, 34),
            [DummyNickName + "_11012019_192835"] = new DateTime(2019, 11, 1, 19, 28, 35),
            [DummyNickName + "_05302020_005900"] = new DateTime(2020, 5, 30, 0, 59, 0),
            [DummyNickName + "_08122045_035959"] = new DateTime(2045, 08, 12, 3, 59, 59),
        };

        [OneTimeSetUp]
        public void SetUp()
        {
            Debug.Log("Beginning one-time setup...");
            Debug.Log("persistent data path: " + Application.persistentDataPath);
            DummySaveFiles = MakeDummyFiles();
        }

        private static List<string> GetExistingSaveFiles(string nickName = DummyNickName)
        {
            return Directory
                .GetFiles(SaveDataTestImpl.SaveFolderPath, $"{nickName}*{SaveDataTestImpl.SaveFileExtension}")
                .ToList();
        }

        [Test]
        public void TestGenerateAutoSaveName()
        {
            foreach (var saveDate in DummySaveDates)
            {
                Debug.Log("Parsed to: " + SaveDataTestImpl.GetSaveFileNameWithDate(DummyNickName, saveDate.Value));
                Assert.AreEqual(saveDate.Key, SaveDataTestImpl.GetSaveFileNameWithDate(DummyNickName, saveDate.Value));
            }
        }

        [Test]
        public void TestParseAutoSaveName()
        {
            foreach (var saveDate in DummySaveDates)
            {
                Assert.AreEqual(saveDate.Value, SaveDataTestImpl.GetSaveDate(saveDate.Key));
            }
        }

        private static string MakeDummyFile(string fileName)
        {
            Debug.Log("Creating dummy file: " + fileName);
            string newFilePath = Path.ChangeExtension(Path.Combine(SaveDataTestImpl.SaveFolderPath, fileName),
                SaveDataTestImpl.SaveFileExtension);
            File.WriteAllText(newFilePath, "I am a dummy test save with the name " + fileName);
            Assume.That(File.Exists(newFilePath), "The file " + newFilePath + " doesn't exist!");
            return newFilePath;
        }

        private static void DeleteSaveFiles(string nickName = DummyNickName)
        {
            Debug.Log($"Clearing old save files named {nickName}...");
            GetExistingSaveFiles(nickName).ForEach(File.Delete);
            Assume.That(GetExistingSaveFiles(nickName), Is.Empty,
                "There were still dummy files left after we tried to delete them all!");
        }


        static List<string> MakeDummyFiles()
        {
            Debug.Log($"About to create dummy save files...");
            DeleteSaveFiles();
            var allSaves = new List<string>();
            foreach (var saveDate in DummySaveDates)
            {
                allSaves.Add(MakeDummyFile(saveDate.Key));
            }

            Debug.Log("Finished creating dummy save files:\n" + string.Join("\n", allSaves));
            Assume.That(
                GetExistingSaveFiles().Count,
                Is.EqualTo(DummySaveDates.Count),
                "Didn't generate the proper number of save files!"
            );
            return allSaves;
        }

        [Test]
        public void TestSortAutoSaveFiles()
        {
            var loadedSaves = SaveDataTestImpl.GetAllSaves(DummyNickName);
            Assert.AreNotEqual(0, loadedSaves.Length, "No saves were actually loaded!");
            TestUtils.AreEqual(DummySaveFiles, loadedSaves);
        }

        [Test]
        public void TestGetNickname()
        {
            foreach (var saveDate in DummySaveDates)
            {
                Assert.AreEqual(SaveDataTestImpl.GetNickname(saveDate.Key), DummyNickName);
            }
        }

        [Test(TestOf = typeof(SaveData<>))]
        public void TestTrimSaveFiles()
        {
            var testTrimValues = new List<int>() {0, 1, 5, 10};
            foreach (var t in testTrimValues)
            {
                _TestTrimSaveFiles(t);
            }
        }

        private static void _TestTrimSaveFiles(int trimTo)
        {
            Assume.That(trimTo, Is.LessThanOrEqualTo(DummySaveDates.Count));
            MakeDummyFiles();
            SaveDataTestImpl.TrimSaves(DummyNickName, trimTo);
            Assert.That(
                GetExistingSaveFiles().Count,
                Is.EqualTo(trimTo),
                "The incorrect number of files remained after trimming!"
            );
        }

        [Test]
        public void TestBackupSaveSlots()
        {
            const string nickName = nameof(TestBackupSaveSlots);
            DeleteSaveFiles(nickName);
            var newSave = SaveDataTestImpl.NewSaveFile(nameof(TestBackupSaveSlots));

            for (int numberOfSaveFiles = 1;
                numberOfSaveFiles < SaveDataTestImpl.BackupSaveSlots * 2;
                numberOfSaveFiles++)
            {
                Thread.Sleep(SaveDataTestImpl.ReSaveDelay);
                newSave.Save();
                Debug.Log($"Created new save file:[{numberOfSaveFiles}] {newSave}");
                Assert.AreEqual(
                    Math.Min(numberOfSaveFiles + 1, SaveDataTestImpl.BackupSaveSlots),
                    SaveDataTestImpl.GetAllSaves(newSave.nickName).Length,
                    $"Didn't find the correct number of saves!\n\t{string.Join("\n\t", SaveDataTestImpl.GetAllSaves(newSave.nickName))}"
                );
            }
        }

        /// <summary>
        /// Tests that the save file is serialized with the <see cref="DateTime.Ticks"/> of the save time, not the <see cref="DateTime"/>.
        /// </summary>
        [Test]
        public void TestSerializeLastSaveTime()
        {
            var newSave = SaveDataTestImpl.NewSaveFile(nameof(TestSerializeLastSaveTime));
            var saveJson = JsonUtility.ToJson(newSave, true);
            Debug.Log($"{nameof(saveJson)}: {saveJson}");
            Assert.IsTrue(saveJson.Contains($"\"lastSaveTime\": {newSave.LastSaveTime.Ticks}"));
        }

        [Test]
        public void TestToStringMatchesToJson()
        {
            var newSave = SaveDataTestImpl.NewSaveFile(MethodBase.GetCurrentMethod().Name);
            var saveJson = JsonUtility.ToJson(newSave, true);
            var saveString = newSave.ToString();
            Assert.That(saveString, Is.EqualTo(saveJson));
        }
    }
}