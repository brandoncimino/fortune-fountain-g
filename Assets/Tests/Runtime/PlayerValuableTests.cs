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
            public int Events;
            public int Amount;

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
    }
}