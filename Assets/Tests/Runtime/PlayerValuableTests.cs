﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BrandonUtils.Logging;
using BrandonUtils.Standalone;
using BrandonUtils.Standalone.Chronic;
using BrandonUtils.Standalone.Collections;
using BrandonUtils.Standalone.Reflection;
using BrandonUtils.Testing;
using BrandonUtils.Timing;
using Newtonsoft.Json;
using NUnit.Framework;
using Runtime;
using Runtime.Saving;
using Runtime.Utils;
using Runtime.Valuables;
using UnityEngine;
using UnityEngine.TestTools;
using Is = BrandonUtils.Testing.Is;

namespace Tests.Runtime {
    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
    public class PlayerValuableTests {
        [Test]
        public void TestFirstValuableEnabledOnNewSave() {
            const string nickName = nameof(TestFirstValuableEnabledOnNewSave);
            var fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            const ValuableType expectedFirstValuableType = 0;

            Assert.That(fortuneFountainSaveData.PlayerValuables,
                Has.Some.Property(nameof(PlayerValuable.ValuableType)).EqualTo(expectedFirstValuableType),
                $"The new save file didn't contain PlayerValuable type 0 ({expectedFirstValuableType})!");

            Assert.That(fortuneFountainSaveData.PlayerValuables.Count, Is.EqualTo(1),
                "The save file contained extra player valuables!");
        }

        [Test]
        public void TestGenerateIsLimitedByRate() {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            var generateCounter = 0;
            PlayerValuable.GeneratePlayerValuableEvent += (valuable, amount) => generateCounter++;

            LogUtils.Log(GameManager.SaveData.PlayerValuables[0]);
            LogUtils.Log(JsonConvert.SerializeObject(GameManager.SaveData.PlayerValuables[0]));

            for (var i = 0; i < 10; i++) {
                GameManager.SaveData.PlayerValuables.CheckGenerate();
                Assert.That(generateCounter, Is.EqualTo(0), $"Error on {i + 1}th generation!");
            }
        }

        [UnityTest]
        public IEnumerator GenerateAllViaCollectionExtension(
            [Values(true, false)] bool checkThrowables
        ) {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables(GameManager.SaveData);

            Assume.That(GameManager.SaveData.PlayerValuables.Count, Is.GreaterThan(1),
                "We need to test more than 1 valuable type!");
            Assume.That(GameManager.SaveData.PlayerValuables, Has.All.Property("Rate").EqualTo(1),
                "All valuables should have a generation rate of 1!");

            //Add counter events for each of the valuables
            var generateCounters = CreateValuableGenerationCounters(GameManager.SaveData.PlayerValuables);

            //Set the LastGenerateTime for each valuable to be their previous interval (that way, they are ready to generate)
            var setTime = FrameTime.Now - GameManager.SaveData.PlayerValuables[0].GenerateInterval;

            GameManager.SaveData.Hand.LastThrowTime = setTime;

            GameManager.SaveData.PlayerValuables.ForEach(it => it.LastGenerateCheckTime = setTime);

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.Throwables.Count, Is.EqualTo(0));
            }

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter() { Events = 1, Amount = 1 }));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.Throwables.Count,
                    Is.EqualTo(generateCounters.Sum(it => it.Value.Amount)));
            }

            //sleep so we can expect to generate another item
            yield return TestUtils.WaitForRealtime(GameManager.SaveData.PlayerValuables[0].GenerateInterval);

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter() { Events = 2, Amount = 2 }));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.Throwables.Count,
                    Is.EqualTo(generateCounters.Sum(it => it.Value.Amount)));
            }

            //sleep so that we can expect to generate _2_ more items
            yield return
                TestUtils.WaitForRealtime(GameManager.SaveData.PlayerValuables[0].GenerateInterval.Multiply(2));

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter() { Events = 3, Amount = 4 }));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.Throwables,
                    Has.Property(nameof(Hand.Throwables.Count)).EqualTo(generateCounters.Sum(it => it.Value.Amount)));
            }
        }

        /// <summary>
        /// A simple counter for the number of times a <see cref="PlayerValuable.GeneratePlayerValuableEvent"/> was triggered (<see cref="Events"/>) and the total number of <see cref="PlayerValuable"/>s generated by those events (<see cref="Amount"/>)
        /// </summary>
        private class ValuableGenerationCounter : IEquatable<ValuableGenerationCounter> {
            public int Events { get; set; }
            public int Amount { get; set; }

            public override string ToString() {
                return JsonConvert.SerializeObject(this);
            }

            public bool Equals(ValuableGenerationCounter other) {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return Events == other.Events && Amount == other.Amount;
            }
        }

        /// <summary>
        /// Creates <see cref="PlayerValuable.GeneratePlayerValuableEvent"/>s that increment simple counters.
        /// </summary>
        /// <param name="playerValuables"></param>
        /// <returns>A dictionary linking <see cref="ValuableType"/>s to the number of times their respective <see cref="PlayerValuable.GeneratePlayerValuableEvent"/>s were triggered (<see cref="ValuableGenerationCounter.Events"/>) and the number of <see cref="PlayerValuable"/>s that were generated by those events (<see cref="ValuableGenerationCounter.Amount"/>)</returns>
        private static Dictionary<ValuableType, ValuableGenerationCounter> CreateValuableGenerationCounters(
            KeyedList<ValuableType, PlayerValuable> playerValuables) {
            var generateCounters = playerValuables.ToDictionary(
                playerValuable => playerValuable.ValuableType,
                playerValuable => new ValuableGenerationCounter()
            );

            PlayerValuable.GeneratePlayerValuableEvent += (valuable, amount) => {
                generateCounters[valuable.ValuableType].Events += 1;
                generateCounters[valuable.ValuableType].Amount += amount;
            };

            return generateCounters;
        }

        private static double[] seconds = {
            5,
            1,
            20,
            999,
            0.5,
            Math.PI,
            double.Epsilon,
            double.Epsilon * 2,
            TimeSpan.MaxValue.Seconds / 2d,
            Math.E
        };

        private static double[] rates = {
            1,
            2,
            50,
            3.4,
            Math.PI,
            0.5,
            0.1,
            double.Epsilon,
        };

        [Test]
        [TestCase(10, 0.1, 10)]
        [TestCase(1, 10, 10)]
        [TestCase(0.1, 0.001, 5)]
        public void GenerateEventsLimitedByMaxGenerateTime(
            double generateTimeLimitInSeconds,
            double itemsPerSecond,
            double extraGenerationSeconds
        ) {
            //the number of items we expect to generate - which should always be limited by maxGenerationSeconds
            var expectedItemsGenerated = generateTimeLimitInSeconds * itemsPerSecond;

            Ignore.Unless(
                () => Assume.That(generateTimeLimitInSeconds, Is.GreaterThan(0),
                    $"{nameof(generateTimeLimitInSeconds)} must be greater than 0!"),
                () => Assume.That(itemsPerSecond, Is.GreaterThan(0),
                    $"{nameof(itemsPerSecond)} must be greater than 0!"),
                () => Assume.That(extraGenerationSeconds, Is.GreaterThanOrEqualTo(0),
                    $"{nameof(extraGenerationSeconds)} must be positive!"),
                () => Assume.That(expectedItemsGenerated, Is.GreaterThanOrEqualTo(1),
                    $"{nameof(expectedItemsGenerated)} must be at least 1!")
            );

            //this test is very quick and runs a LOT of times, so we should disable logging to prevent it from being crazy slow
            // LogUtils.locations = LogUtils.Locations.None;

            const string nickName = nameof(GenerateEventsLimitedByMaxGenerateTime);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);


            //Setting up the player data
            GameManager.SaveData.PlayerValuables =
                TestData.GetUniformPlayerValuables(GameManager.SaveData, itemsPerSecond);
            GameManager.SaveData.Hand.LastThrowTime = FrameTime.Now -
                                                      TimeSpan.FromSeconds(generateTimeLimitInSeconds +
                                                                           extraGenerationSeconds);
            GameManager.SaveData.PlayerValuables.ForEach(it =>
                it.LastGenerateCheckTime = GameManager.SaveData.Hand.LastThrowTime);
            GameManager.SaveData.Hand.GenerateTimeLimit = TimeSpan.FromSeconds(generateTimeLimitInSeconds);

            Assert.That(GameManager.SaveData.PlayerValuables[0].GenerateInterval,
                Is.LessThanOrEqualTo(GameManager.SaveData.Hand.GenerateTimeLimit),
                $"We must have enough time to generate at least one item - i.e. {nameof(PlayerValuable.GenerateInterval)} <= {nameof(Hand.GenerateTimeLimit)}!");

            var genCounters = CreateValuableGenerationCounters(GameManager.SaveData.PlayerValuables);

            //Generate the items
            GameManager.SaveData.PlayerValuables.CheckGenerate();

            //Check for the proper number of events & actual items generated
            AssertAll.Of(
                () => Assert.That(
                    GroupThrowables(GameManager.SaveData.Hand.Throwables),
                    Has.All.Values().With.Count.InRange(Math.Floor(expectedItemsGenerated),
                        Math.Ceiling(expectedItemsGenerated))
                ),
                () => Assert.That(genCounters,
                    Has.All.Values().With.Property(nameof(ValuableGenerationCounter.Events)).EqualTo(1)),
                () => Assert.That(genCounters,
                    Has.All.Values().With.Property(nameof(ValuableGenerationCounter.Amount))
                        .InRange(Math.Floor(expectedItemsGenerated), Math.Ceiling(expectedItemsGenerated)))
            );
        }

        private Dictionary<ValuableType, List<ThrowableValuable>> GroupThrowables(IList<Throwable> throwables) {
            return throwables.Select(it => it as ThrowableValuable)
                .Where(it => it != null)
                .GroupBy(it => it.ValuableType)
                .ToDictionary(
                    it => it.Key,
                    it => it.ToList()
                );
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(19)]
        public void TestGenerateInterval(
            [ValueSource(nameof(rates))] double rateInItemsPerSecond
        ) {
            var saveData = new FortuneFountainSaveData(nameof(TestGenerateInterval));
            var pv = new PlayerValuable(saveData, ValuableType.Coin) { Rate = rateInItemsPerSecond };

            Assert.That(TimeSpan.FromSeconds(1).Divide(pv.GenerateInterval),
                Is.InRange(Math.Floor(rateInItemsPerSecond), rateInItemsPerSecond * 1.00001));

            Assert.That(pv.GenerateInterval,
                Is.EqualTo(TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / rateInItemsPerSecond))));
        }

        [UnityTest]
        public IEnumerator TestCompleteButUncheckedGenDuringPreviousSession(
            [Values(4)] int uniformValuableGenerationRate,
            [Values(2)] int itemsPerWait
        ) {
            GameManager.SaveData =
                FortuneFountainSaveData.NewSaveFile(nameof(TestCompleteButUncheckedGenDuringPreviousSession));

            //Set the uniform gen rates to a decent number, so that we don't have to wait very long to get meaningful results
            GameManager.SaveData.PlayerValuables =
                TestData.GetUniformPlayerValuables(GameManager.SaveData, uniformValuableGenerationRate);
            var genInterval = GameManager.SaveData.PlayerValuables[0].GenerateInterval;
            float genWait = (float)genInterval.TotalSeconds;

            //Create the generation counters
            var genCounters = CreateValuableGenerationCounters(GameManager.SaveData.PlayerValuables);

            //Wait long enough to generate something (but DON'T check for it)
            yield return new WaitForSecondsRealtime(genWait * itemsPerWait);

            //Save the game - still without checking for any generation
            GameManager.SaveData.Save(false);

            //Wait long enough that we COULD have generated something if we were playing; then reload
            yield return new WaitForSecondsRealtime(genWait * itemsPerWait);

            GameManager.SaveData.Reload();

            //Wait long enough to generate something during THIS session as well
            yield return new WaitForSecondsRealtime(genWait * itemsPerWait);

            //Make sure that we still haven't generated anything, and that that LastGenerationTime values haven't changed
            Assert.That(genCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter() { Amount = 0, Events = 0 }));

            //Check for generation
            var itemsGenerated = GameManager.SaveData.PlayerValuables.CheckGenerate();

            //Assert that we've now had exactly 1 generation event, where we generated 2 * itemsPerWait
            Assert.That(genCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter() { Amount = 2 * itemsPerWait, Events = 1 }));
            Assert.That(itemsGenerated, Has.All.EqualTo(2 * itemsPerWait));
        }

        [UnityTest]
        public IEnumerator EventSubscribersPersistAfterReloadMultiple() {
            GameManager.SaveData =
                FortuneFountainSaveData.NewSaveFile(nameof(EventSubscribersPersistAfterReloadMultiple));
            var eventWasTriggered = false;
            PlayerValuable.GeneratePlayerValuableEvent += (valuable, amount) => eventWasTriggered = true;

            yield return new WaitForSecondsRealtime((float)GameManager.SaveData.PlayerValuables[0].GenerateInterval
                .TotalSeconds);

            GameManager.SaveData.Save(false);
            GameManager.SaveData.Reload();
            GameManager.SaveData.Reload();
            GameManager.SaveData.Reload();
            GameManager.SaveData.PlayerValuables.CheckGenerate();

            Assert.True(eventWasTriggered);
        }

        [UnityTest]
        public IEnumerator TestPartialUncheckedItemsGeneratedDuringLastSession(
            [Values(0.5)] double previousGenIntervals,
            [Values(3)] double totalItemsToGenerate
        ) {
            Assume.That(previousGenIntervals, Is.LessThan(1));
            Assume.That(totalItemsToGenerate, Is.GreaterThan(previousGenIntervals));

            GameManager.SaveData =
                FortuneFountainSaveData.NewSaveFile(nameof(TestPartialUncheckedItemsGeneratedDuringLastSession));
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables(GameManager.SaveData);

            yield return TestUtils.WaitForRealtime(GameManager.SaveData.PlayerValuables[0].GenerateInterval,
                previousGenIntervals);

            GameManager.SaveData.Save(false);

            var genCounters = CreateValuableGenerationCounters(GameManager.SaveData.PlayerValuables);

            yield return TestUtils.WaitForRealtime(GameManager.SaveData.PlayerValuables[0].GenerateInterval, 2);

            GameManager.SaveData.Reload();

            var sleepIntervals = totalItemsToGenerate - previousGenIntervals + 0.5;
            LogUtils.Log($"{nameof(sleepIntervals)} = {sleepIntervals}");
            yield return TestUtils.WaitForRealtime(GameManager.SaveData.PlayerValuables[0].GenerateInterval,
                sleepIntervals);

            var itemsGenerated = GameManager.SaveData.PlayerValuables.CheckGenerate();

            Assert.That(itemsGenerated, Has.All.EqualTo(totalItemsToGenerate));
            Assert.That(genCounters,
                Has.All.Values().EqualTo(new ValuableGenerationCounter()
                    { Amount = (int)totalItemsToGenerate, Events = 1 }));
        }


        [Test]
        [TestCase(10, 5.6)]
        [TestCase(0.1, 598.234)]
        [TestCase(11293.34, 0.2)]
        [TestCase(899, 5)]
        [TestCase(Math.PI, Math.PI)]
        public void TestSimpleGeneration(double rate, double secondsSinceLastGenerateCheckAndThrow) {
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestSimpleGeneration));
            var startTime = FrameTime.Now.AddSeconds(-secondsSinceLastGenerateCheckAndThrow);
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables(GameManager.SaveData, rate);
            GameManager.SaveData.Hand.LastThrowTime = startTime;
            GameManager.SaveData.LastLoadTime = startTime;
            ReflectionUtils.SetVariableValue(GameManager.SaveData, nameof(GameManager.SaveData.LastSaveTime),
                startTime);
            GameManager.SaveData.PlayerValuables.ForEach(it => it.LastGenerateCheckTime = startTime);

            var generatedItems = GameManager.SaveData.PlayerValuables.CheckGenerate(TimeSpan.MaxValue);
            //TODO: This test probably needs to be revisited so that this can be a cleaner assertion
            var expectedItemsGenerated = secondsSinceLastGenerateCheckAndThrow * rate;
            AssertAll.Of(
                generatedItems,
                Has.All.EqualTo(generatedItems[0]),
                Is.All.InRange(expectedItemsGenerated.Floor(), expectedItemsGenerated.Ceiling())
            );
        }

        [Test]
        public void ThrowResetsGenerateTimeUtilized() {
            FortuneFountainSaveData fortuneFountainSaveData =
                FortuneFountainSaveData.NewSaveFile(nameof(ThrowResetsGenerateTimeUtilized));
            fortuneFountainSaveData.PlayerValuables = TestData.GetUniformPlayerValuables(fortuneFountainSaveData);

            var utilizedTime = TimeSpan.FromSeconds(1);
            fortuneFountainSaveData.PlayerValuables.ForEach(it => it.GenerateTimeUtilized = utilizedTime);

            Assert.That(fortuneFountainSaveData.PlayerValuables,
                Has.All.Property(nameof(PlayerValuable.GenerateTimeUtilized)).EqualTo(utilizedTime));

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.PlayerValuables,
                Has.All.Property(nameof(PlayerValuable.GenerateTimeUtilized)).EqualTo(TimeSpan.Zero));
        }
    }
}