using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Runtime.Saving;
using Runtime.Valuables;

namespace Tests.Runtime {
    public class HandTests {
        [Test]
        public void LastGrabTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(
                fortuneFountainSaveData.Hand.LastGrabTime,
                Is.EqualTo(new DateTime())
            );

            fortuneFountainSaveData.Hand.Grab(ValuableType.Coin, 10);
            Assert.That(
                fortuneFountainSaveData.Hand.LastGrabTime,
                Is.EqualTo(DateTime.Now)
            );
        }

        [Test]
        public void LastThrowTime() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            Assert.That(
                fortuneFountainSaveData.Hand.LastThrowTime,
                Is.EqualTo(new DateTime())
            );

            fortuneFountainSaveData.Hand.Throw();
            Assert.That(
                fortuneFountainSaveData.Hand.LastThrowTime,
                Is.EqualTo(DateTime.Now)
            );
        }

        [Test]
        public void GrabValuable() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();

            foreach (var valuableType in ValuableDatabase.ValuableTypes) {
                //assert that it doesn't contain the item before we grab it
                Assert.That(
                    fortuneFountainSaveData.Hand.throwables.Select(it => it.ValuableType),
                    new NotConstraint(Contains.Item(valuableType))
                );

                //grab the item
                fortuneFountainSaveData.Hand.Grab(valuableType, 10);

                //assert that we're now holding it
                Assert.That(
                    fortuneFountainSaveData.Hand.throwables.Select(it => it.ValuableType),
                    Contains.Item(valuableType)
                );
            }
        }

        [Test]
        public void PostThrowKarma() {
            FortuneFountainSaveData fortuneFountainSaveData = new FortuneFountainSaveData();
            var kValues = new List<double> {
                10,
                54,
                87,
                98,
                12341435,
                7845687,
                0,
                -123,
                1.5,
                Math.PI,
                Math.E,
                -238475.52349578,
            };

            double karmaTotal = 0;

            foreach (var kValue in kValues) {
                karmaTotal += kValue;

                //make sure the hand is empty
                Assert.That(fortuneFountainSaveData.Hand.throwables, Is.Empty,
                            "The hand should be empty before we grab anything");

                fortuneFountainSaveData.Hand.Grab(ValuableType.Coin, kValue);
                //make sure contains the thing we grabbed
                Assert.That(fortuneFountainSaveData.Hand.throwables.Select(it => it.ValuableType),
                            Contains.Item(ValuableType.Coin));

                fortuneFountainSaveData.Hand.Throw();

                //make sure the hand is empty after we've thrown it
                Assert.That(fortuneFountainSaveData.Hand.throwables, Is.Empty,
                            "The hand should be empty after we've thrown everything");

                Assert.That(fortuneFountainSaveData.Karma, Is.EqualTo(karmaTotal));
            }
        }
    }
}