using System.Threading;
using NUnit.Framework;
using Runtime.Saving;
using Runtime.Valuables;
using UnityEngine;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Tests.Runtime {
    public class FortuneFountainSaveDataTests {
        [Test]
        public void TestSerializeEmptyHand() {
            const string nickName = nameof(TestSerializeEmptyHand);
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            Assert.That(fortuneFountainSaveData.ToJson(), Contains.Substring($"\"{nameof(Hand)}\": {{"));
        }

        [Test]
        public void TestSerializeThrowables() {
            const string nickName = nameof(TestSerializeThrowables);
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            for (int i = 0; i < ValuableDatabase.ValuableTypes.Length; i++) {
                var karmaValue = Random.Range(1, 25);
                var valuableType = ValuableDatabase.ValuableTypes[i];
                Log($"Grabbing a {valuableType} with a value of {karmaValue}");
                fortuneFountainSaveData.Hand.Grab(new Throwable(valuableType, karmaValue));

                Log($"Waiting for {nameof(FortuneFountainSaveData.ReSaveDelay)} ({FortuneFountainSaveData.ReSaveDelay})");
                Thread.Sleep(FortuneFountainSaveData.ReSaveDelay);
                Log($"Done waiting - re-saving {nickName}...");
                fortuneFountainSaveData.Save();

                //load the save data we created
                FortuneFountainSaveData loadedSaveData = FortuneFountainSaveData.Load(nickName);

                try {
                    Assert.That(loadedSaveData.ToJson(), Contains.Substring($"\"{nameof(Hand.throwables)}\":"));
                    Assert.That(loadedSaveData.Hand.throwables.Count, Is.EqualTo(i + 1));
                    Assert.That(loadedSaveData.Hand.throwables[i].ValuableType, Is.EqualTo(valuableType));
                    Assert.That(loadedSaveData.Hand.throwables[i].ThrowValue, Is.EqualTo(karmaValue));
                }
                catch (AssertionException e) {
                    Log($"Failed an exception for the save data:\n{loadedSaveData}");
                    Log(e.StackTrace);
                    throw;
                }
            }
        }
    }
}