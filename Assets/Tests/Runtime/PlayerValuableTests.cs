using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;
using Runtime;
using Runtime.Saving;
using Runtime.Valuables;

namespace Tests.Runtime {
    public class PlayerValuableTests {
        [Test]
        public void TestFirstValuableEnabledOnNewSave() {
            const string nickName = nameof(TestFirstValuableEnabledOnNewSave);
            var fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            const ValuableType expectedFirstValuableType = 0;

            Assert.That(fortuneFountainSaveData.PlayerValuables, Contains.Key(expectedFirstValuableType), $"The new save file didn't contain PlayerValuable type 0 ({expectedFirstValuableType})!");
            Assert.That(fortuneFountainSaveData.PlayerValuables.Keys.Count, Is.EqualTo(1), "The save file contained extra player valuables!");
        }

        [Test]
        public void TestGenerateIsLimitedByRate() {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            var generateCounter = 0;
            GameManager.SaveData.PlayerValuables[0].GeneratePlayerValuableEvent += valuable => generateCounter++;

            for (var i = 0; i < 10; i++) {
                GameManager.SaveData.PlayerValuables[0].CheckGenerate();
                Assert.That(generateCounter, Is.EqualTo(0));
            }
        }

        [Test]
        public void GenerateAllViaCollectionExtension() {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            const int valuableRate = 1;
            GameManager.SaveData.PlayerValuables.ForEach(it => it.Rate = valuableRate);

            var generateCounters = AddValuableCounters();
            foreach (var playerValuable in GameManager.SaveData.PlayerValuables.Values) playerValuable.LastGenerateTime = DateTime.Now.Subtract(playerValuable.GenerateInterval);

            GameManager.SaveData.PlayerValuables.ForEach(it => it.CheckGenerate());

            foreach (var gCounter in generateCounters) Assert.That(gCounter.Value, Is.EqualTo(1), $"The valuable {gCounter.Key} was generated {gCounter.Value} times!");

            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval);

            GameManager.SaveData.PlayerValuables.ForEach(it => it.CheckGenerate());

            foreach (var gCounter in generateCounters) Assert.That(gCounter.Value, Is.EqualTo(2), $"The valuable {gCounter.Key} was generated {gCounter.Value} times!");
        }

        private Dictionary<ValuableType, int> AddValuableCounters() {
            var generateCounters = new Dictionary<ValuableType, int>();

            foreach (var playerValuable in GameManager.SaveData.PlayerValuables.Values) {
                generateCounters.Add(playerValuable.ValuableType, 0);
                playerValuable.GeneratePlayerValuableEvent += valuable => generateCounters[valuable.ValuableType] = generateCounters[valuable.ValuableType + 1];
            }

            return generateCounters;
        }
    }
}