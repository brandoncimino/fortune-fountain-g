﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Testing;
using Runtime;
using Runtime.Saving;
using Runtime.Utils;
using Runtime.Valuables;
using UnityEngine;

namespace Tests.Runtime {
    public class PlayerValuableTests {
        [Test]
        public void TestFirstValuableEnabledOnNewSave() {
            const string nickName                = nameof(TestFirstValuableEnabledOnNewSave);
            var          fortuneFountainSaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            const ValuableType expectedFirstValuableType = 0;

            Assert.That(fortuneFountainSaveData.PlayerValuables,            Contains.Key(expectedFirstValuableType), $"The new save file didn't contain PlayerValuable type 0 ({expectedFirstValuableType})!");
            Assert.That(fortuneFountainSaveData.PlayerValuables.Keys.Count, Is.EqualTo(1),                           "The save file contained extra player valuables!");
        }

        [Test]
        public void TestGenerateIsLimitedByRate() {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            var generateCounter = 0;
            GameManager.SaveData.PlayerValuables[0].GeneratePlayerValuableEvent += (valuable, amount) => generateCounter++;

            for (var i = 0; i < 10; i++) {
                GameManager.SaveData.PlayerValuables[0].CheckGenerate();
                Assert.That(generateCounter, Is.EqualTo(0));
            }
        }

        [Test]
        public void GenerateAllViaCollectionExtension() {
            const string nickName = nameof(TestGenerateIsLimitedByRate);
            GameManager.SaveData                 = FortuneFountainSaveData.NewSaveFile(nickName);
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables();

            Assume.That(GameManager.SaveData.PlayerValuables.Count, Is.GreaterThan(1),                            "We need to test more than 1 valuable type!");
            Assume.That(GameManager.SaveData.PlayerValuables,       Has.All.Values().Property("Rate").EqualTo(1), "All valuables should have a generation rate of 1!");

            //Add counter events for each of the valuables
            var generateCounters = CreateValuableGenerationCounters();

            //Set the LastGenerateTime for each valuable to be their previous interval (that way, they are ready to generate)
            foreach (var playerValuable in GameManager.SaveData.PlayerValuables.Values) {
                playerValuable.LastGenerateTime = DateTime.Now.Subtract(playerValuable.GenerateInterval);
            }

            //Check that nothing has been generated yet
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 0, Amount = 0}));

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 1, Amount = 1}));

            //sleep so we can expect to generate another item
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval);

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 2, Amount = 2}));

            //sleep so that we can expect to generate _2_ more items
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval.Multiply(2));

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 3, Amount = 4}));
        }

        /// <summary>
        /// A simple counter for the number of times a <see cref="PlayerValuable.GeneratePlayerValuableEvent"/> was triggered (<see cref="Events"/>) and the total number of <see cref="PlayerValuable"/>s generated by those events (<see cref="Amount"/>)
        /// </summary>
        private class ValuableGenerationCounter : IEquatable<ValuableGenerationCounter> {
            public int Events { get; set; }
            public int Amount { get; set; }

            public override string ToString() {
                return JsonUtility.ToJson(this);
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
        /// <returns>A dictionary linking <see cref="ValuableType"/>s to the number of times their respective <see cref="PlayerValuable.GeneratePlayerValuableEvent"/>s were triggered (<see cref="ValuableGenerationCounter.Events"/>) and the number of <see cref="PlayerValuable"/>s that were generated by those events (<see cref="ValuableGenerationCounter.Amount"/>)</returns>
        private static Dictionary<ValuableType, ValuableGenerationCounter> CreateValuableGenerationCounters() {
            var generateCounters = new Dictionary<ValuableType, ValuableGenerationCounter>();

            foreach (var playerValuable in GameManager.SaveData.PlayerValuables.Values) {
                generateCounters.Add(playerValuable.ValuableType, new ValuableGenerationCounter());
                playerValuable.GeneratePlayerValuableEvent += (valuable, amount) => {
                    generateCounters[valuable.ValuableType].Events += 1;
                    generateCounters[valuable.ValuableType].Amount += amount;
                };
            }

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
            999
        };

        [Test, Combinatorial]
        [TestCase(5,     1,    20)]
        [TestCase(2,     2,    5)]
        [TestCase(3878,  23,   88)]
        [TestCase(10,    3,    0)]
        [TestCase(500,   5,    500)]
        [TestCase(100,   100,  100)]
        [TestCase(1000,  1000, 1000)]
        [TestCase(10000, 1000, 10000)]
        [TestCase(1,     23,   50)]
        [TestCase(500,   23,   78)]
        [TestCase(500,   23,   1)]
        [TestCase(500,   22,   1)]
        [TestCase(500,   21,   1)]
        [TestCase(500,   20,   1)]
        [TestCase(500,   19,   1)]
        [TestCase(50,    19,   1)]
        [TestCase(5,     19,   1)]
        public void GenerateEventsLimitedByMaxGenerateTime([ValueSource(nameof(seconds))] double generateTimeLimitInSeconds, [ValueSource(nameof(rates))] double itemsPerSecond, [ValueSource(nameof(seconds)), Values(0)]
                                                           double extraGenerationSeconds) {
            Assume.That(generateTimeLimitInSeconds, Is.GreaterThan(0),          $"{nameof(generateTimeLimitInSeconds)} must be greater than 0!");
            Assume.That(itemsPerSecond,             Is.GreaterThan(0),          $"{nameof(itemsPerSecond)} must be greater than 0!");
            Assume.That(extraGenerationSeconds,     Is.GreaterThanOrEqualTo(0), $"{nameof(extraGenerationSeconds)} must be positive!");

            const string nickName = nameof(GenerateEventsLimitedByMaxGenerateTime);
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nickName);

            //the number of items we expect to generate - which always be limited by maxGenerationSeconds
            var expectedItemsGenerated = generateTimeLimitInSeconds * itemsPerSecond;
            Assume.That(expectedItemsGenerated, Is.GreaterThanOrEqualTo(1), $"{nameof(expectedItemsGenerated)} must be at least 1!");

            //Setting up the player data
            GameManager.SaveData.PlayerValuables        = TestData.GetUniformPlayerValuables(itemsPerSecond);
            GameManager.SaveData.Hand.LastThrowTime     = DateTime.Now - TimeSpan.FromSeconds(generateTimeLimitInSeconds + extraGenerationSeconds);
            GameManager.SaveData.Hand.GenerateTimeLimit = TimeSpan.FromSeconds(generateTimeLimitInSeconds);

            Assert.That(GameManager.SaveData.PlayerValuables[0].GenerateInterval, Is.LessThanOrEqualTo(GameManager.SaveData.Hand.GenerateTimeLimit), $"We must have enough time to generate at least one item - i.e. {nameof(PlayerValuable.GenerateInterval)} <= {nameof(Hand.GenerateTimeLimit)}!");

            var genCounters = CreateValuableGenerationCounters();

            //Generate the items
            GameManager.SaveData.PlayerValuables.CheckGenerate();

            //Check for the proper number of events & actual items generated.
            //NOTE: For the tolerance of expectedItemsGenerated, we are intentionally limiting the lower bound to the floor of expectedItemsGenerated because, ideally, errors should favor the player over the system (i.e., for a rate of 99/second we prefer to generate 100 pennies than 98)
            Assert.That(genCounters, Has.All.Values().Property(nameof(ValuableGenerationCounter.Events)).EqualTo(1));
            Assert.That(genCounters, Has.All.Values().Property(nameof(ValuableGenerationCounter.Amount)).InRange(Math.Floor(expectedItemsGenerated), expectedItemsGenerated * 1.0000001));
        }

        [Test]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(19)]
        public void TestGenerateInterval([ValueSource(nameof(rates))] double rateInItemsPerSecond) {
            var pv = new PlayerValuable(ValuableType.Coin) {Rate = rateInItemsPerSecond};

            Assert.That(TimeSpan.FromSeconds(1).Divide(pv.GenerateInterval), Is.InRange(Math.Floor(rateInItemsPerSecond), rateInItemsPerSecond * 1.00001));

            Assert.That(pv.GenerateInterval, Is.EqualTo(TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / rateInItemsPerSecond))));
        }
    }
}