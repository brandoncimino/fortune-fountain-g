using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Packages.BrandonUtils.Runtime.Collections;
using Runtime.Saving;
using Runtime.Valuables;
using static Packages.BrandonUtils.Runtime.Logging.LogUtils;

namespace Tests.Runtime {
    public class HandTests {
        [Test]
        public void LastGrabTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(fortuneFountainSaveData.Hand.LastGrabTime, Is.EqualTo(new DateTime()));

            fortuneFountainSaveData.Hand.Grab(new Throwable(ValuableType.Coin, 10));
            Assert.That(fortuneFountainSaveData.Hand.LastGrabTime, Is.EqualTo(DateTime.Now));
        }

        [Test]
        public void LastThrowTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.EqualTo(new DateTime()));

            var initialTime = fortuneFountainSaveData.Hand.LastThrowTime;

            fortuneFountainSaveData.Hand.Throw();

            //Because of the milliseconds of time it takes to do these assertions and stuff, the LastThrowTime isn't going to be exactly DateTime.Now, so we're instead asserting that the time is between the range of initialTime (exclusive) and DateTime.Now (inclusive).
            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.InRange(initialTime, DateTime.Now));
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
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData();

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
                SimpleSaveData(),
                UglySaveData()
            }.ToList().ForEach(save => Assert.That(save.Hand.KarmaInHand, Is.EqualTo(save.Hand.throwables.Sum(it => it.ThrowValue))));
        }

        [Test]
        public void ThrowOneTwice() {
            var save1 = new FortuneFountainSaveData() {
                Hand = {
                    throwables = {
                        new Throwable(ValuableType.Coin, 1)
                    }
                }
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
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData();

            Assume.That(fortuneFountainSaveData.Karma, Is.EqualTo(0));

            var expectedPostThrowKarma = fortuneFountainSaveData.Hand.KarmaInHand;

            Log($"Before throwing, there is {fortuneFountainSaveData.Karma} karma");

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(expectedPostThrowKarma));

            Log(fortuneFountainSaveData.Hand.throwables.Count);
        }

        private static FortuneFountainSaveData SimpleSaveData() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData {
                Hand = {
                    throwables = new List<Throwable>() {
                        new Throwable(ValuableType.Coin, 10d),
                        new Throwable(ValuableType.Coin, 20d),
                        new Throwable(ValuableType.Coin, 30d)
                    }
                }
            };

            Assume.That(fortuneFountainSaveData.Hand.throwables, Is.Not.Empty);

            return fortuneFountainSaveData;
        }

        private static FortuneFountainSaveData UglySaveData() {
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
                }
            };

            Assume.That(fortuneFountainSaveData.Hand.throwables, Is.Not.Empty);

            return fortuneFountainSaveData;
        }

        [Test]
        public void ThrowHandCausesThrowSingle() {
            var fortuneFountainSaveData = SimpleSaveData();

            int expectedThrowEvents = fortuneFountainSaveData.Hand.throwables.Count;
            int actualThrowEvents   = 0;

            //register an anonymous method to the ThrowSingleEvent that will count the number of events
            Throwable.ThrowSingleEvent += throwable => actualThrowEvents++;
            fortuneFountainSaveData.Hand.Throw();

            Assert.That(actualThrowEvents, Is.EqualTo(expectedThrowEvents));
        }

        [Test]
        public void ThrowEmptiesHand() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData();

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Hand.throwables, Is.Empty);
        }

        [Test]
        public void ThrowSingleRemovesThrowable() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData();

            var toBeThrown = fortuneFountainSaveData.Hand.throwables[0];

            toBeThrown.Throw();

            CollectionAssert.DoesNotContain(fortuneFountainSaveData.Hand.throwables, toBeThrown);
        }
    }
}