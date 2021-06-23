using System;
using System.Collections;
using System.Threading;

using BrandonUtils.Standalone;
using BrandonUtils.Testing;
using BrandonUtils.Timing;

using NUnit.Framework;

using Runtime.Saving;
using Runtime.Valuables;

using UnityEngine;
using UnityEngine.TestTools;

using static BrandonUtils.Logging.LogUtils;

using Random = UnityEngine.Random;

namespace Tests.Runtime {
    public class FortuneFountainSaveDataTests {
        public const double EstimatedLoadDuration_InSeconds = 0.01;

        [Test]
        public void TestSerializeEmptyHand() {
            const string            nickName                = nameof(TestSerializeEmptyHand);
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            Assert.That(fortuneFountainSaveData.ToJson(), Contains.Substring($"\"{nameof(Hand)}\": {{"));
        }

        [Test]
        public void TestSerializeThrowables() {
            const string            nickName                = nameof(TestSerializeThrowables);
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            for (int i = 0; i < ValuableDatabase.ValuableTypes.Length; i++) {
                var karmaValue   = Random.Range(1, 25);
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
                    Assert.That(loadedSaveData.ToJson(),                        Contains.Substring($"\"{nameof(Hand.throwables)}\":"));
                    Assert.That(loadedSaveData.Hand.throwables.Count,           Is.EqualTo(i + 1));
                    Assert.That(loadedSaveData.Hand.throwables[i].ValuableType, Is.EqualTo(valuableType));
                    Assert.That(loadedSaveData.Hand.throwables[i].ThrowValue,   Is.EqualTo(karmaValue));
                }
                catch (AssertionException e) {
                    Log($"Failed an exception for the save data:\n{loadedSaveData}");
                    Log(e.StackTrace);
                    throw;
                }
            }
        }

        private static double[] seconds = {
            1,
            2,
            3,
            Math.PI,
            Math.E,
            123198.2543,
            7181.1000291,
            0.00000001
        };

        [Test]
        public void TestInGameTimeSinceLastThrowWithoutSaving(
            [ValueSource(nameof(seconds))]
            double secondsInGame
        ) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestInGameTimeSinceLastThrowWithoutSaving));

            var inGameTime = TimeSpan.FromSeconds(secondsInGame);
            fortuneFountainSaveData.Hand.LastThrowTime = FrameTime.Now - inGameTime;
            Assert.That(fortuneFountainSaveData.InGameTimeSinceLastThrow, Is.InRange(inGameTime.Multiply(0.999), inGameTime.Multiply(1.001)));
        }

        [Test]
        [TestCase(3, 1, Math.PI)]
        public void TestInGameTimeSinceLastThrowWithSaving(double secondsInPreviousSession, double secondsOutOfGame, double secondsInCurrentSession) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestInGameTimeSinceLastThrowWithSaving));

            var previousSession = TimeSpan.FromSeconds(secondsInPreviousSession);
            var outOfGameTime   = TimeSpan.FromSeconds(secondsOutOfGame);
            var currentSession  = TimeSpan.FromSeconds(secondsInCurrentSession);
            var inGameTime      = previousSession + currentSession;

            fortuneFountainSaveData.Hand.LastThrowTime = FrameTime.Now - previousSession;
            fortuneFountainSaveData.Save(false);

            Thread.Sleep(outOfGameTime);

            fortuneFountainSaveData.Reload();

            Thread.Sleep(currentSession);

            Assert.That(fortuneFountainSaveData.InGameTimeSinceLastThrow, Is.InRange(inGameTime.Multiply(0.999), inGameTime.Multiply(1.001)));
        }

        private static readonly double[] RealSeconds = {
            0.1,
            1,
            0.5,
            0.1003,
            Math.PI,
            Math.E,
            0.99,
            0.212387687,
            1.1,
            1.01,
            0,
            0.000001
        };

        [UnityTest]
        public IEnumerator OutOfGameTime_UpdatesOnReload(
            [ValueSource(nameof(RealSeconds))]
            double secondsOutOfGame
        ) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_UpdatesOnReload));

            //Go to the next frame and then save (to make sure we "discard" the time spent creating the save file)
            yield return null;
            fortuneFountainSaveData.Save(false);

            //record the time we saved at
            var saveTime = FrameTime.Now;

            var outOfGameTime = TimeSpan.FromSeconds(secondsOutOfGame);

            yield return TestUtils.WaitFor(outOfGameTime);
            fortuneFountainSaveData.Reload();

            //record the time we loaded at
            var loadTime = FrameTime.Now;

            //make sure that the OOG-time is the difference between loading and saving
            //NOTE: This value will always be slightly larger than secondsOutOfGame due to the time actually spend saving & loading, etc.
            Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(loadTime - saveTime));
        }

        [UnityTest]
        public IEnumerator OutOfGameTime_UpdatesOnLoad(
            [ValueSource(nameof(RealSeconds))]
            double secondsOutOfGame
        ) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_UpdatesOnLoad));

            yield return null;
            fortuneFountainSaveData.Save(false);

            var saveTime = FrameTime.Now;

            var outOfGameSpan = TimeSpan.FromSeconds(secondsOutOfGame);

            yield return TestUtils.WaitForRealtime(outOfGameSpan);
            var loadedSaveData = FortuneFountainSaveData.Load(fortuneFountainSaveData.nickName);

            var loadTime = FrameTime.Now;

            Assert.That(loadedSaveData, Has.Property(nameof(loadedSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(loadTime - saveTime));
        }

        [UnityTest]
        public IEnumerator OutOfGameTime_DoesNotUpdateOnSave(
            [ValueSource(nameof(RealSeconds))]
            double secondsOutOfGame
        ) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_DoesNotUpdateOnSave));

            var outOfGameSpan = TimeSpan.FromSeconds(secondsOutOfGame);

            //Set the OutOfGameTimeSinceLastThrow (using reflection since the setter is private)
            fortuneFountainSaveData.GetType().GetProperty(nameof(FortuneFountainSaveData.OutOfGameTimeSinceLastThrow))?.SetValue(fortuneFountainSaveData, outOfGameSpan);

            Assert.That(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow, Is.EqualTo(outOfGameSpan));

            const int repetitions = 5;
            for (int rep = 0; rep < repetitions; rep++) {
                yield return new WaitForSecondsRealtime(0.01f);
                fortuneFountainSaveData.Save(false);

                Assert.That(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow, Is.EqualTo(outOfGameSpan), $"[{nameof(rep)}: {rep}] The {nameof(FortuneFountainSaveData.OutOfGameTimeSinceLastThrow)} should not have changed when we {nameof(FortuneFountainSaveData.Save)}-ed!");
            }
        }

        [UnityTest]
        public IEnumerator OutOfGameTime_MultipleSessionsWithoutThrowing() {
            const float sessionSeconds   = 2f;
            const int   numberOfSessions = 2;
            var         sessionSpan      = TimeSpan.FromSeconds(sessionSeconds);
            locations = Locations.None;

            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_MultipleSessionsWithoutThrowing));

            yield return null;
            fortuneFountainSaveData.Save(false);
            var realTimeNowAtSave = FrameTime.Now;

            Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(TimeSpan.Zero), $"We haven't waited yet, so there shouldn't be any {nameof(FortuneFountainSaveData.OutOfGameTimeSinceLastThrow)}!");

            Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.LastSaveTime)).EqualTo(realTimeNowAtSave));

            var expectedOutOfGameTime = TimeSpan.Zero;

            for (int session = 0; session < numberOfSessions; session++) {
                yield return TestUtils.WaitForRealtime(sessionSpan);
                fortuneFountainSaveData.Reload();
                var realTimeNowAtReload = FrameTime.Now;

                Assert.That(realTimeNowAtReload, Is.Not.EqualTo(realTimeNowAtSave));

                var frameTimeDuration = realTimeNowAtReload - realTimeNowAtSave;

                expectedOutOfGameTime += frameTimeDuration;

                Assert.That(
                    fortuneFountainSaveData,
                    Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(expectedOutOfGameTime)
                );

                yield return TestUtils.WaitForRealtime(sessionSpan);
                fortuneFountainSaveData.Save(false);
                realTimeNowAtSave = FrameTime.Now;
            }
        }
    }
}