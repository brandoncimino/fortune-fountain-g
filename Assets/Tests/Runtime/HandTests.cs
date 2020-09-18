using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Collections;
using Packages.BrandonUtils.Runtime.Timing;

using Runtime.Saving;
using Runtime.Valuables;

using UnityEngine;
using UnityEngine.TestTools;

using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Tests.Runtime {
    public class HandTests {
        [UnityTest]
        public IEnumerator LastGrabTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(fortuneFountainSaveData.Hand.LastGrabTime, Is.EqualTo(FrameTime.Now));
            var previousGrabTime = fortuneFountainSaveData.Hand.LastGrabTime;

            yield return new WaitForSecondsRealtime(0.001f);

            fortuneFountainSaveData.Hand.Grab(new Throwable(ValuableType.Coin, 10));
            Assert.That(fortuneFountainSaveData.Hand.LastGrabTime, Is.EqualTo(FrameTime.Now));
            Assert.That(fortuneFountainSaveData.Hand.LastGrabTime, Is.Not.EqualTo(previousGrabTime));
        }

        [UnityTest]
        public IEnumerator LastThrowTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.EqualTo(FrameTime.Now));

            var initialTime = fortuneFountainSaveData.Hand.LastThrowTime;

            yield return new WaitForSecondsRealtime(0.001f);

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.EqualTo(FrameTime.Now));
            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.Not.EqualTo(initialTime));
        }

        [Test]
        public void GrabValuable() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            foreach (var valuableType in ValuableDatabase.ValuableTypes) {
                //assert that it doesn't contain the item before we grab it
                Assert.That(fortuneFountainSaveData.Hand.throwables.Select(it => it.ValuableType), new NotConstraint(Contains.Item(valuableType)));

                //grab the item
                fortuneFountainSaveData.Hand.Grab(new Throwable(valuableType, 10));

                //assert that we're now holding it
                Assert.That(fortuneFountainSaveData.Hand.throwables.Select(it => it.ValuableType), Contains.Item(valuableType));
            }
        }

        /// <summary>
        /// Tests the <see cref="FortuneFountainSaveData.Karma"/> after calling <see cref="Throwable.Throw"/> directly on individual <see cref="Throwable"/>s, one-at-a-time.
        /// </summary>
        [Test]
        public void PostThrowSingleKarma() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(PostThrowSingleKarma));

            double expectedKarmaTotal = 0;

            var throwablesCopy = fortuneFountainSaveData.Hand.throwables.Copy();

            foreach (var throwable in throwablesCopy) {
                expectedKarmaTotal += throwable.ThrowValue;

                throwable.Throw();

                Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(expectedKarmaTotal));
            }
        }

        [Test]
        public void KarmaInHandIsAccurate() {
            new[] {
                    SimpleSaveData(nameof(KarmaInHandIsAccurate)),
                    UglySaveData(nameof(KarmaInHandIsAccurate))
                }.ToList()
                 .ForEach(save => Assert.That(save.Hand.KarmaInHand, Is.EqualTo(save.Hand.throwables.Sum(it => it.ThrowValue))));
        }

        [Test]
        public void ThrowOneTwice() {
            var save1 = new FortuneFountainSaveData() {
                Hand = {
                    throwables = {
                        new Throwable(ValuableType.Coin, 1)
                    }
                },
                nickName = nameof(ThrowOneTwice)
            };

            int throwCount = 0;
            Throwable.ThrowSingleEvent += throwable => throwCount++;

            save1.Hand.Throw();
            Assert.That(throwCount,            Is.EqualTo(1));
            Assert.That(save1.Hand.throwables, Is.Empty);

            save1.Hand.Throw();
            Assert.That(throwCount, Is.EqualTo(1));
        }

        [Test]
        public void PostThrowHandKarma() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(PostThrowHandKarma));

            Assume.That(fortuneFountainSaveData.Karma,           Is.EqualTo(0));
            Assume.That(fortuneFountainSaveData.Hand.throwables, Is.Not.Empty);

            var expectedPostThrowKarma = fortuneFountainSaveData.Hand.KarmaInHand;

            Log(
                $"Before throwing, there is {fortuneFountainSaveData.Karma} karma",
                $"In my hand, there is {fortuneFountainSaveData.Hand.KarmaInHand} karma"
            );

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(expectedPostThrowKarma));

            Assert.That(fortuneFountainSaveData.Hand.throwables, Is.Empty);
        }

        private static FortuneFountainSaveData SimpleSaveData(string nickName) {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData {
                Hand = {
                    throwables = new List<Throwable>() {
                        new Throwable(ValuableType.Coin, 10d),
                        new Throwable(ValuableType.Coin, 20d),
                        new Throwable(ValuableType.Coin, 30d)
                    }
                },
                nickName = nickName
            };

            Assume.That(fortuneFountainSaveData.Hand.throwables, Is.Not.Empty);

            return fortuneFountainSaveData;
        }

        private static FortuneFountainSaveData UglySaveData(string nickName) {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData {
                Hand = {
                    throwables = new List<Throwable>() {
                        new Throwable(ValuableType.Coin,        10),
                        new Throwable(ValuableType.Fiduciary,   54),
                        new Throwable(ValuableType.Coin,        87),
                        new Throwable(ValuableType.Gem,         98),
                        new Throwable(ValuableType.Livestock,   12341435),
                        new Throwable(ValuableType.Collectible, 7845687),
                        new Throwable(ValuableType.Metal,       0),
                        new Throwable(ValuableType.Scrip,       -123),
                        new Throwable(ValuableType.Coin,        1.5),
                        new Throwable(ValuableType.Scrip,       Math.PI),
                        new Throwable(ValuableType.Fiduciary,   Math.E),
                        new Throwable(ValuableType.Fiduciary,   -238475.52349578),
                    }
                },
                nickName = nickName
            };

            Assume.That(fortuneFountainSaveData.Hand.throwables, Is.Not.Empty);

            return fortuneFountainSaveData;
        }

        [Test]
        public void ThrowHandCausesThrowSingle() {
            var fortuneFountainSaveData = SimpleSaveData(nameof(ThrowHandCausesThrowSingle));

            int expectedThrowEvents = fortuneFountainSaveData.Hand.throwables.Count;
            int actualThrowEvents   = 0;

            //register an anonymous method to the ThrowSingleEvent that will count the number of events
            Throwable.ThrowSingleEvent += throwable => actualThrowEvents++;
            fortuneFountainSaveData.Hand.Throw();

            Assert.That(actualThrowEvents, Is.EqualTo(expectedThrowEvents));
        }

        [Test]
        public void ThrowEmptiesHand() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData(nameof(ThrowEmptiesHand));

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Hand.throwables, Is.Empty);
        }

        [Test]
        public void ThrowSingleRemovesThrowable() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData(nameof(ThrowSingleRemovesThrowable));

            var toBeThrown = fortuneFountainSaveData.Hand.throwables[0];

            toBeThrown.Throw();

            CollectionAssert.DoesNotContain(fortuneFountainSaveData.Hand.throwables, toBeThrown);
        }

        [Test]
        public void ThrowResetsOutOfGameTime() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(ThrowResetsOutOfGameTime));

            var outOfGameSpan = TimeSpan.FromDays(500);

            ReflectionUtils.SetVariable(fortuneFountainSaveData, nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow), outOfGameSpan);

            Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(outOfGameSpan));

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData, Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(TimeSpan.Zero));
        }
    }
}