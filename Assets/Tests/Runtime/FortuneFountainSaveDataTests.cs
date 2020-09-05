﻿using System;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Testing;
using Packages.BrandonUtils.Runtime.Timing;
using Runtime.Saving;
using Runtime.Valuables;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;
using Random = UnityEngine.Random;

namespace Tests.Runtime {
    public class FortuneFountainSaveDataTests {
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

        [Test]
        [TestCase(0.1)]
        [TestCase(1)]
        [TestCase(0.5)]
        [TestCase(0)]
        [TestCase(Math.PI)]
        [TestCase(Math.E)]
        [TestCase(0.99)]
        [TestCase(0.212387687)]
        [TestCase(1.1)]
        [TestCase(1.01)]
        public void TestOutOfGameTime(double secondsOutOfGame) {
            FortuneFountainSaveData fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestOutOfGameTime));
            fortuneFountainSaveData.Save(false); //For some reason, the first test fails if we don't save again here - something with NUnit causing slowdown for the first test, maybe?

            var outOfGameTime = TimeSpan.FromSeconds(secondsOutOfGame);

            Thread.Sleep(outOfGameTime);

            fortuneFountainSaveData.Reload();

            var tolerance = TimeSpan.FromSeconds(0.1);

            Assert.That(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow, Is.InRange(outOfGameTime - tolerance, outOfGameTime + tolerance));
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