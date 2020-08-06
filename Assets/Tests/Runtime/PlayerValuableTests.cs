using NUnit.Framework;
using Runtime.Saving;
using Runtime.Valuables;

namespace Tests.Runtime {
    public class PlayerValuableTests {
        [Test]
        public void TestFirstValuableEnabledOnNewSave() {
            var nickName = nameof(TestFirstValuableEnabledOnNewSave);
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            Assert.That(fortuneFountainSaveData.PlayerValuables.Keys, Contains.Key(0), $"The new save file didn't contain PlayerValuable type 0 ({(ValuableType) 0})!");
            Assert.That(fortuneFountainSaveData.PlayerValuables.Keys.Count, Is.EqualTo(1), "The save file contained extra player valuables!");
        }
    }
}