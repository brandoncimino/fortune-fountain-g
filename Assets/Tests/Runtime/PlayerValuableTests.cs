using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Collections;
using Packages.BrandonUtils.Runtime.Logging;
using Packages.BrandonUtils.Runtime.Testing;
using Runtime;
using Runtime.Saving;
using Runtime.Utils;
using Runtime.Valuables;

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
            PlayerValuable.GeneratePlayerValuableEvent += (valuable, amount) => generateCounter++;

            LogUtils.Log(GameManager.SaveData.PlayerValuables[0]);
            LogUtils.Log(JsonConvert.SerializeObject(GameManager.SaveData.PlayerValuables[0]));

            for (var i = 0; i < 10; i++) {
                GameManager.SaveData.PlayerValuables[0].CheckGenerate();
                Assert.That(generateCounter, Is.EqualTo(0), $"Error on {i + 1}th generation!");
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GenerateAllViaCollectionExtension(bool checkThrowables) {
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

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.throwables.Count, Is.EqualTo(0));
            }

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 1, Amount = 1}));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.throwables.Count, Is.EqualTo(generateCounters.Sum(it => it.Value.Amount)));
            }

            //sleep so we can expect to generate another item
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval);

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 2, Amount = 2}));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.throwables.Count, Is.EqualTo(generateCounters.Sum(it => it.Value.Amount)));
            }

            //sleep so that we can expect to generate _2_ more items
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval.Multiply(2));

            //Generate stuff
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Events = 3, Amount = 4}));

            if (checkThrowables) {
                Assert.That(GameManager.SaveData.Hand.throwables, Has.Property(nameof(Hand.throwables.Count)).EqualTo(generateCounters.Sum(it => it.Value.Amount)));
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
        /// <returns>A dictionary linking <see cref="ValuableType"/>s to the number of times their respective <see cref="PlayerValuable.GeneratePlayerValuableEvent"/>s were triggered (<see cref="ValuableGenerationCounter.Events"/>) and the number of <see cref="PlayerValuable"/>s that were generated by those events (<see cref="ValuableGenerationCounter.Amount"/>)</returns>
        private static Dictionary<ValuableType, ValuableGenerationCounter> CreateValuableGenerationCounters() {
            var generateCounters = new Dictionary<ValuableType, ValuableGenerationCounter>();

            foreach (var playerValuable in GameManager.SaveData.PlayerValuables.Values) {
                generateCounters.Add(playerValuable.ValuableType, new ValuableGenerationCounter());
            }

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
        public void GenerateEventsLimitedByMaxGenerateTime([ValueSource(                                   nameof(seconds))]
                                                           double generateTimeLimitInSeconds, [ValueSource(nameof(rates))]
                                                           double itemsPerSecond,             [ValueSource(nameof(seconds)), Values(0)]
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
        public void TestGenerateInterval([ValueSource(nameof(rates))]
                                         double rateInItemsPerSecond) {
            var pv = new PlayerValuable(ValuableType.Coin) {Rate = rateInItemsPerSecond};

            Assert.That(TimeSpan.FromSeconds(1).Divide(pv.GenerateInterval), Is.InRange(Math.Floor(rateInItemsPerSecond), rateInItemsPerSecond * 1.00001));

            Assert.That(pv.GenerateInterval, Is.EqualTo(TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / rateInItemsPerSecond))));
        }

        /// <summary>
        /// TODO: This test...is a nightmare. It definitely should be broken up.
        /// </summary>
        /// <param name="firstEventAmount"></param>
        /// <param name="afkSeconds"></param>
        /// <param name="secondEventAmount"></param>
        [Test]
        [TestCase(2, 6, 4)]
        public void TestSerializeLastGenerateTime(int firstEventAmount, double afkSeconds, int secondEventAmount) {
            var saveInitializeBefore = DateTime.Now;
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestSerializeLastGenerateTime));
            var initializeBefore = DateTime.Now;
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables();
            var initializeAfter = DateTime.Now;
            LogUtils.Log($"Initialize time: {initializeBefore} + {initializeAfter - initializeBefore}");

            var generateCounters = CreateValuableGenerationCounters();

            Assert.That(GameManager.SaveData.PlayerValuables, Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(initializeBefore, initializeAfter));
            Assert.That(GameManager.SaveData.Hand,            Has.Property(nameof(Hand.LastThrowTime)).InRange(saveInitializeBefore, initializeBefore));

            //wait & check generate
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval.Multiply(firstEventAmount));

            var beforeGenerate1 = DateTime.Now;
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            var afterGenerate1 = DateTime.Now;
            LogUtils.Log($"Generate time: {beforeGenerate1} + {afterGenerate1 - beforeGenerate1}");

            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = firstEventAmount, Events = 1}));

            //Assert that the LastGenerateTimes DID CHANGE
            Assert.That(GameManager.SaveData.PlayerValuables, Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).Not.InRange(initializeBefore, initializeAfter));

            var genInterval     = GameManager.SaveData.PlayerValuables[0].GenerateInterval;
            var expectedGenTime = GameManager.SaveData.Hand.LastThrowTime + genInterval.Multiply(firstEventAmount);

            /* A bunch of nicely formatted logging statements for debugging
            var now = DateTime.Now;
            LogUtils.Log($"{nameof(now)}             {now}");
            LogUtils.Log($"{nameof(expectedGenTime)} {expectedGenTime.Ticks} ({now.Ticks - expectedGenTime.Ticks} ticks ago; {((double) now.Ticks - expectedGenTime.Ticks)/genInterval.Ticks} intervals ago)");
            LogUtils.Log($"{nameof(afterGenerate1)}  {afterGenerate1.Ticks}  ({now.Ticks - afterGenerate1.Ticks}  ticks ago; {((double) now.Ticks - afterGenerate1.Ticks)/genInterval.Ticks} intervals ago)");
            LogUtils.Log($"Interval = {genInterval} ({genInterval.TotalSeconds} seconds, {genInterval.Ticks} ticks)");
            */

            Assert.That(GameManager.SaveData.Hand.throwables.Count, Is.GreaterThan(0));
            Assert.That(GameManager.SaveData.PlayerValuables,       Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(beforeGenerate1 - genInterval,               afterGenerate1));
            Assert.That(GameManager.SaveData.PlayerValuables,       Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(expectedGenTime - genInterval.Multiply(0.5), expectedGenTime + genInterval.Multiply(0.5)));

            //Save the file
            var beforeSave = DateTime.Now;
            GameManager.SaveData.Save(false);
            var afterSave = DateTime.Now;
            LogUtils.Log($"Save time: {beforeSave} + {afterSave - beforeSave}");

            //Wait a bit & reload
            Thread.Sleep(TimeSpan.FromSeconds(afkSeconds));

            var beforeReload = DateTime.Now;
            GameManager.SaveData.Reload();
            var afterReload = DateTime.Now;
            LogUtils.Log($"Reload time: {beforeReload} + {afterReload - beforeReload}");

            Assert.That(GameManager.SaveData.PlayerValuables, Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(beforeGenerate1 - genInterval, afterGenerate1));

            LogUtils.Log($"{nameof(generateCounters)} after reload: {JsonConvert.SerializeObject(generateCounters, Formatting.Indented)}");

            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = firstEventAmount, Events = 1}));

            var beforeGenerate2 = DateTime.Now;
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            var afterGenerate2 = DateTime.Now;
            LogUtils.Log($"generate2 time: {beforeGenerate2} + {afterGenerate2 - beforeGenerate2}");

            Assert.That(generateCounters, Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = firstEventAmount, Events = 1}));

            Assert.That(GameManager.SaveData.PlayerValuables, Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(beforeGenerate1 - genInterval, afterGenerate1));

            //Wait to generate the second event
            Thread.Sleep(GameManager.SaveData.PlayerValuables[0].GenerateInterval.Multiply(secondEventAmount));

            //Check generation (which should be enough to trigger the second event)
            var beforeGenerate3 = DateTime.Now;
            GameManager.SaveData.PlayerValuables.CheckGenerate();
            var afterGenerate3 = DateTime.Now;
            LogUtils.Log($"generate3 time: {beforeGenerate3} + {afterGenerate3 - beforeGenerate3}");

            Assert.That(generateCounters,                     Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = firstEventAmount + secondEventAmount, Events = 2}));
            Assert.That(GameManager.SaveData.PlayerValuables, Has.All.Values().Property(nameof(PlayerValuable.LastGenerateTime)).InRange(beforeGenerate3, afterGenerate3));
        }

        [Test]
        [TestCase(4, 2)]
        public void TestCompleteButUncheckedGenDuringPreviousSession(int uniformValuableGenerationRate, int itemsPerWait) {
            GameManager.SaveData = FortuneFountainSaveData.NewSaveFile(nameof(TestCompleteButUncheckedGenDuringPreviousSession));

            //Set the uniform gen rates to a decent number, so that we don't have to wait very long to get meaningful results
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables(uniformValuableGenerationRate);
            var genInterval = GameManager.SaveData.PlayerValuables[0].GenerateInterval;

            //Create the generation counters
            var genCounters = CreateValuableGenerationCounters();

            var oldLastGenTimes = GameManager.SaveData.PlayerValuables.Select(it => it.Value.LastGenerateTime);

            //Wait long enough to generate something (but DON'T check for it)
            Thread.Sleep(genInterval.Multiply(itemsPerWait));

            //Save the game - still without checking for any generation
            GameManager.SaveData.Save(false);

            //Wait long enough that we COULD have generated something if we were playing; then reload
            Thread.Sleep(genInterval.Multiply(itemsPerWait));

            GameManager.SaveData.Reload();

            //Wait long enough to generate something during THIS session as well
            Thread.Sleep(genInterval.Multiply(itemsPerWait));

            //Make sure that we still haven't generated anything, and that that LastGenerationTime values haven't changed
            Assert.That(genCounters,                                                                  Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = 0, Events = 0}));
            Assert.That(GameManager.SaveData.PlayerValuables.Select(it => it.Value.LastGenerateTime), Is.EqualTo(oldLastGenTimes));

            //Check for generation
            var itemsGenerated = GameManager.SaveData.PlayerValuables.CheckGenerate();

            //Assert that we've now had exactly 1 generation event, where we generated 2 * itemsPerWait
            Assert.That(genCounters,    Has.All.Values().EqualTo(new ValuableGenerationCounter() {Amount = 2 * itemsPerWait, Events = 1}));
            Assert.That(itemsGenerated, Has.All.EqualTo(2 * itemsPerWait));
        }

        /// <summary>
        /// This is a stripped-down version of <see cref="TestCompleteButUncheckedGenDuringPreviousSession"/>
        /// </summary>
        /// <param name="firstEventAmount"></param>
        /// <param name="secondEventAmount"></param>
        [Test]
        [TestCase(2, 5)]
        public void EventSubscribersPersistAfterReloadMultiple(int firstEventAmount, int secondEventAmount) {
            GameManager.SaveData                 = FortuneFountainSaveData.NewSaveFile(nameof(EventSubscribersPersistAfterReloadMultiple));
            GameManager.SaveData.PlayerValuables = TestData.GetUniformPlayerValuables();

            PlayerValuable.GeneratePlayerValuableEvent += EventSubscriber;

            GameManager.SaveData.PlayerValuables.ForEach(it => it.LastGenerateTime = DateTime.Now - it.GenerateInterval.Multiply(firstEventAmount));

            GameManager.SaveData.PlayerValuables.CheckGenerate();

            Assert.That(_eventCounter,  Is.EqualTo(GameManager.SaveData.PlayerValuables.Count));
            Assert.That(_amountCounter, Is.EqualTo(GameManager.SaveData.PlayerValuables.Count * firstEventAmount));

            GameManager.SaveData.Save(false);
            GameManager.SaveData.Reload();

            GameManager.SaveData.PlayerValuables.ForEach(it => it.LastGenerateTime = DateTime.Now - it.GenerateInterval.Multiply(secondEventAmount + 0.1));

            GameManager.SaveData.PlayerValuables.CheckGenerate();

            Assert.That(_eventCounter,  Is.EqualTo(GameManager.SaveData.PlayerValuables.Count * 2));
            Assert.That(_amountCounter, Is.EqualTo(GameManager.SaveData.PlayerValuables.Count * (firstEventAmount + secondEventAmount)));
        }

        private static int _eventCounter  = 0;
        private static int _amountCounter = 0;

        private static void EventSubscriber(PlayerValuable playerValuable, int amount) {
            _eventCounter  += 1;
            _amountCounter += amount;
        }
    }
}