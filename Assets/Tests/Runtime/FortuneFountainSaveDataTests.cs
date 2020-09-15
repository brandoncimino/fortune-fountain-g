using System;
using System.Collections;
using System.Threading;

using NUnit.Framework;

using Packages.BrandonUtils.Runtime.Logging;
using Packages.BrandonUtils.Runtime.Testing;
using Packages.BrandonUtils.Runtime.Timing;

using Runtime.Saving;
using Runtime.Valuables;

using UnityEngine;
using UnityEngine.TestTools;

using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

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
            fortuneFountainSaveData.Hand.LastThrowTime = RealTime.Now - inGameTime;
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

            fortuneFountainSaveData.Hand.LastThrowTime = RealTime.Now - previousSession;
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
            1.01
        };

        [UnityTest]
        public IEnumerator OutOfGameTime_UpdatesOnReload(
            [ValueSource(nameof(RealSeconds))]
            double secondsOutOfGame
        ) {
            Assume.That(secondsOutOfGame, Is.GreaterThanOrEqualTo((double) EstimatedLoadDuration_InSeconds), $"it takes ~{EstimatedLoadDuration_InSeconds} seconds to save & reload, meaning that out of game time will essentially never be less than {EstimatedLoadDuration_InSeconds} - as a result, we want to ignore any tests checking for intervals smaller than {EstimatedLoadDuration_InSeconds}");

            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_UpdatesOnReload));

            //Go to the next frame and then save (to make sure we "discard" the time spent creating the save file)
            yield return null;
            fortuneFountainSaveData.Save(false);

            var outOfGameTime = TimeSpan.FromSeconds(secondsOutOfGame);

            yield return TestUtils.WaitFor(outOfGameTime);
            fortuneFountainSaveData.Reload();

            Assert.That(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow, new ApproximationConstraint(outOfGameTime, TestUtils.ApproximationTimeThreshold));
        }

        [UnityTest]
        public IEnumerator OutOfGameTime_UpdatesOnLoad(
            [ValueSource(nameof(RealSeconds))]
            double secondsOutOfGame
        ) {
            //Disable logging, to squeeze that extra performance from loading
            LogUtils.locations = Locations.None;

            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(OutOfGameTime_UpdatesOnLoad));

            yield return null;
            fortuneFountainSaveData.Save(false);

            var outOfGameSpan = TimeSpan.FromSeconds(secondsOutOfGame);

            yield return TestUtils.WaitForRealtime(outOfGameSpan);
            var loadedSaveData = FortuneFountainSaveData.Load(fortuneFountainSaveData.nickName);

            Assert.That(loadedSaveData.OutOfGameTimeSinceLastThrow, new ApproximationConstraint(outOfGameSpan, TestUtils.ApproximationTimeThreshold + TimeSpan.FromSeconds(EstimatedLoadDuration_InSeconds)));
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

        [Test]
        [TestCase(5, 3, 1)]
        public void TestMultipleOutOfGameSessions(params double[] sessions) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestMultipleOutOfGameSessions));
            throw new Exception();
            fortuneFountainSaveData.Hand.LastThrowTime = RealTime.Now; //to "discard" the time spent creating the save file
            fortuneFountainSaveData.Save(false);

            var totalSessionTime = TimeSpan.Zero;

            for (var index = 0; index < sessions.Length; index++) {
                var sessionSeconds = sessions[index];
                var sessionSpan    = TimeSpan.FromSeconds(sessionSeconds);
                totalSessionTime += sessionSpan;

                Thread.Sleep(sessionSpan);
                fortuneFountainSaveData.Reload();

                Thread.Sleep(TimeSpan.FromSeconds(sessionSeconds));
                fortuneFountainSaveData.Save(false);

                //threshold increases each time due to the overall slowness of saving/loading repeatedly
                var threshold = TimeSpan.FromSeconds(0.1 * (index + 1) * 2);

                Assert.That(fortuneFountainSaveData, Has.Property(nameof(FortuneFountainSaveData.InGameTimeSinceLastThrow)).Approximately(totalSessionTime, threshold));
                Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).Approximately(totalSessionTime, threshold));
            }
        }
    }
}