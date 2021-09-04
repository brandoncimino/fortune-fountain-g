using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrandonUtils.Logging;
using BrandonUtils.Standalone;
using BrandonUtils.Standalone.Collections;
using BrandonUtils.Testing;
using BrandonUtils.Timing;
using JetBrains.Annotations;
using NUnit.Framework;
using Runtime.Saving;
using Runtime.Valuables;
using UnityEngine;
using UnityEngine.TestTools;
using Is = NUnit.Framework.Is;

namespace Tests.Runtime {
    public class HandTests {
        [UnityTest]
        public IEnumerator LastThrowTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData(nameof(LastThrowTime));

            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.EqualTo(FrameTime.Now));

            var initialTime = fortuneFountainSaveData.Hand.LastThrowTime;

            yield return new WaitForSecondsRealtime(0.001f);

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.EqualTo(FrameTime.Now));
            Assert.That(fortuneFountainSaveData.Hand.LastThrowTime, Is.Not.EqualTo(initialTime));
        }

        [Test]
        public void GrabValuable() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData(nameof(GrabValuable));

            foreach (var valuableType in ValuableDatabase.ValuableTypes) {
                //assert that it doesn't contain the item before we grab it
                var heldValuableTypes = fortuneFountainSaveData.Hand.Throwables.Select(it => it as ThrowableValuable)
                    .Where(it => it != null)
                    .Select(it => it.ValuableType);
                Assert.That(heldValuableTypes, Has.None.EqualTo(valuableType));

                //grab the item
                fortuneFountainSaveData.Hand.AddToHand(new ThrowableValuable(valuableType, 10));

                //assert that we're now holding it
                Assert.That(
                    fortuneFountainSaveData.Hand.Throwables.Select(it => it as ThrowableValuable)
                        .Where(it => it != null)
                        .Select(it => it.ValuableType),
                    Has.Some.EqualTo(valuableType)
                );
            }
        }

        /// <summary>
        /// Tests the <see cref="FortuneFountainSaveData.Karma"/> after calling <see cref="Throwable.Throw"/> directly on individual <see cref="Throwable"/>s, one-at-a-time.
        /// </summary>
        [Test]
        public void PostThrowSingleKarma() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(PostThrowSingleKarma));

            double expectedKarmaTotal = 0;

            var throwablesCopy = fortuneFountainSaveData.Hand.ThrowableValuables.Copy();

            foreach (var throwable in throwablesCopy) {
                expectedKarmaTotal += throwable.PresentValue;

                throwable.Flick();

                Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(expectedKarmaTotal));
            }
        }

        [Test]
        public void KarmaInHandIsAccurate() {
            new[] {
                    SimpleSaveData(nameof(KarmaInHandIsAccurate)),
                    UglySaveData(nameof(KarmaInHandIsAccurate))
                }.ToList()
                .ForEach(save => Assert.That(save.Hand.KarmaInHand,
                    Is.EqualTo(save.Hand.ThrowableValuables.Sum(it => it.PresentValue))));
        }

        [Test]
        public void ThrowOneTwice() {
            var save1 = new FortuneFountainSaveData(nameof(ThrowOneTwice));

            save1.Hand.AddToHand(new ThrowableValuable(ValuableType.Coin, 1));

            save1.Hand.Throw();
            Assert.That(save1.Karma, Is.EqualTo(1));
            Assert.That(save1.Hand.Throwables, Is.Empty);

            save1.Hand.Throw();
            Assert.That(save1.Karma, Is.EqualTo(1));
        }

        [Test]
        public void PostThrowHandKarma() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(PostThrowHandKarma));

            AssertAll.Of(
                () => Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(0)),
                () => Assert.That(fortuneFountainSaveData.Hand.Throwables, Is.Not.Empty)
            );

            var expectedPostThrowKarma = fortuneFountainSaveData.Hand.KarmaInHand;

            LogUtils.Log(
                $"Before throwing, there is {fortuneFountainSaveData.Karma} karma",
                $"In my hand, there is {fortuneFountainSaveData.Hand.KarmaInHand} karma"
            );

            fortuneFountainSaveData.Hand.Throw();

            AssertAll.Of(
                () => Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(expectedPostThrowKarma)),
                () => Assert.That(fortuneFountainSaveData.Hand.Throwables, Is.Empty)
            );
        }

        private static FortuneFountainSaveData SimpleSaveData(string nickName) {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData(nickName);

            fortuneFountainSaveData.Hand.AddToHand(new ThrowableValuable(ValuableType.Coin, 10));
            fortuneFountainSaveData.Hand.AddToHand(new ThrowableValuable(ValuableType.Coin, 20));
            fortuneFountainSaveData.Hand.AddToHand(new ThrowableValuable(ValuableType.Coin, 30));

            Assume.That(fortuneFountainSaveData.Hand.Throwables, Is.Not.Empty);

            return fortuneFountainSaveData;
        }

        [NotNull]
        private static FortuneFountainSaveData UglySaveData([NotNull] string nickName) {
            var throwables = new List<Throwable>() {
                new ThrowableValuable(ValuableType.Coin, 10),
                new ThrowableValuable(ValuableType.Fiduciary, 54),
                new ThrowableValuable(ValuableType.Coin, 87),
                new ThrowableValuable(ValuableType.Gem, 98),
                new ThrowableValuable(ValuableType.Livestock, 12341435),
                new ThrowableValuable(ValuableType.Collectible, 7845687),
                new ThrowableValuable(ValuableType.Metal, 0),
                new ThrowableValuable(ValuableType.Scrip, -123),
                new ThrowableValuable(ValuableType.Coin, 1.5),
                new ThrowableValuable(ValuableType.Scrip, Math.PI),
                new ThrowableValuable(ValuableType.Fiduciary, Math.E),
                new ThrowableValuable(ValuableType.Fiduciary, -238475.52349578),
            };

            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData(nickName) {
                Hand = {
                    _throwables = throwables
                }
            };

            fortuneFountainSaveData.Hand.FixHierarchy();

            AssertAll.Of(
                fortuneFountainSaveData.Hand.Throwables,
                Is.Not.Empty,
                Has.All.Property(nameof(Throwable.MyHand)).EqualTo(fortuneFountainSaveData.Hand)
            );

            return fortuneFountainSaveData;
        }

        [Test]
        public void ThrowEmptiesHand() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData(nameof(ThrowEmptiesHand));

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData.Hand.Throwables, Is.Empty);
        }

        [Test]
        public void ThrowSingleRemovesThrowable() {
            FortuneFountainSaveData fortuneFountainSaveData = SimpleSaveData(nameof(ThrowSingleRemovesThrowable));

            var toBeThrown = fortuneFountainSaveData.Hand.Throwables[0];

            toBeThrown.Flick();

            CollectionAssert.DoesNotContain(fortuneFountainSaveData.Hand.Throwables, toBeThrown);
        }

        [Test]
        public void ThrowResetsOutOfGameTime() {
            FortuneFountainSaveData fortuneFountainSaveData = UglySaveData(nameof(ThrowResetsOutOfGameTime));

            var outOfGameSpan = TimeSpan.FromDays(500);

            ReflectionUtils.SetVariableValue(fortuneFountainSaveData,
                nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow), outOfGameSpan);

            Assert.That(fortuneFountainSaveData,
                Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(outOfGameSpan));

            fortuneFountainSaveData.Hand.Throw();

            Assert.That(fortuneFountainSaveData,
                Has.Property(nameof(fortuneFountainSaveData.OutOfGameTimeSinceLastThrow)).EqualTo(TimeSpan.Zero));
        }
    }
}