using NUnit.Framework;
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
    }
}