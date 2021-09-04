using System;
using System.Diagnostics.CodeAnalysis;
using BrandonUtils.Standalone.Randomization;
using BrandonUtils.Testing;
using NUnit.Framework;
using Runtime.Utils;
using Is = BrandonUtils.Testing.Is;

namespace Tests.Runtime {
    [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
    [TestOf(typeof(Incrementer))]
    public class IncrementerTests {
        [Test]
        public void IncrementerCannotGoBackwards([Values(-1, -0.0001, -99)] double amount) {
            Assert.Catch(() => new Incrementer().AddIncrements(amount));
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(0.2, 5)]
        [TestCase(10, 0.1)]
        [TestCase(0.001, 1 / 0.001)]
        [TestCase(0, double.PositiveInfinity)]
        public void SetPeriodGetHertz(double setPeriodInSeconds, double expectedHertz) {
            var period = TimeSpan.FromSeconds(setPeriodInSeconds);
            var incrementer = new Incrementer() {
                Period = period
            };

            AssertAll.Of(
                $"After setting {nameof(incrementer.Period)}",
                incrementer,
                Has.Property(nameof(incrementer.Period)).EqualTo(period),
                Has.Property(nameof(incrementer.Hertz)).EqualTo(expectedHertz)
            );
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(0.2, 5)]
        [TestCase(10, 0.1)]
        [TestCase(0.001, 1 / 0.001)]
        [TestCase(double.PositiveInfinity, 0)]
        public void SetHertzGetPeriod(double setHertz, double expectedPeriodInSeconds) {
            var incrementer = new Incrementer() {
                Hertz = setHertz
            };

            AssertAll.Of(
                $"After setting {nameof(incrementer.Hertz)}",
                incrementer,
                Has.Property(nameof(incrementer.Hertz)).EqualTo(setHertz),
                Has.Property(nameof(incrementer.Period)).EqualTo(TimeSpan.FromSeconds(expectedPeriodInSeconds))
            );
        }

        [Test]
        public void ZeroHertz() {
            var incrementer = new Incrementer() {
                Hertz = 0
            };

            Assert.That(incrementer, Has.Property(nameof(incrementer.Period)).Null);
        }

        [Test]
        public void ZeroPeriod() {
            var incrementer = new Incrementer() {
                Period = TimeSpan.Zero
            };

            Assert.That(incrementer, Has.Property(nameof(incrementer.Hertz)).EqualTo(double.PositiveInfinity));
        }

        [Test]
        [TestCase(10, 100)]
        [TestCase(999, 0.000001)]
        public void ReductionWorks(int iterations, double maxIncrementsPerIteration) {
            var incrementer = new Incrementer();

            for (int i = 0; i < iterations; i++) {
                incrementer.AddIncrements(Brandom.Gen.NextDouble() * maxIncrementsPerIteration);
            }

            var exactBeforeReduction = incrementer.ExactIncrements;
            var fullBeforeReduction = incrementer.FullIncrements;
            var partialBeforeReduction = incrementer.PartialIncrements;

            Assert.That(exactBeforeReduction - fullBeforeReduction, Is.EqualTo(partialBeforeReduction),
                $"Before {nameof(incrementer.Reduce)}: {nameof(incrementer.ExactIncrements)} - {nameof(incrementer.FullIncrements)} == {nameof(incrementer.PartialIncrements)}");

            var reductionAmount = incrementer.Reduce();
            AssertAll.Of(
                $"After calling {nameof(incrementer.Reduce)}",
                () => Assert.That(reductionAmount, Is.EqualTo(fullBeforeReduction),
                    $"{nameof(incrementer.Reduce)} should have returned the previous {nameof(incrementer.FullIncrements)}"),
                () => Assert.That(incrementer.ExactIncrements, Is.EqualTo(partialBeforeReduction),
                    $"{nameof(incrementer.ExactIncrements)} should equal the previous {nameof(incrementer.PartialIncrements)}"),
                () => Assert.That(incrementer.ExactIncrements, Is.Positive.And.LessThan(1),
                    $"1 > {nameof(incrementer.ExactIncrements)} >= 0")
            );
        }

        [Test]
        [TestCase(1, 1)]
        [TestCase(0.5, 3)]
        [TestCase(10, 2)]
        [TestCase(0, 99)]
        [TestCase(99, 0)]
        public void AddTime(double periodInSeconds, double elapsedTimeInSeconds) {
            var incrementer = new Incrementer() {
                Period = TimeSpan.FromSeconds(periodInSeconds)
            };

            incrementer.AddSeconds(elapsedTimeInSeconds);

            AssertAll.Of(
                incrementer,
                Has.Property(nameof(incrementer.Period)).Approximately(TimeSpan.FromSeconds(periodInSeconds)),
                Has.Property(nameof(incrementer.ExactIncrements)).CloseTo(elapsedTimeInSeconds / periodInSeconds)
            );
        }

        [Test]
        [TestCase(1, 1, 1)]
        [TestCase(2, 1, 0.5)]
        [TestCase(0.1, 3, 30)]
        [TestCase(0, 0, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 1, double.PositiveInfinity)]
        public void ComputeElapsedIncrements(double periodInSeconds, double elapsedTimeInSeconds,
            double expectedIntervalAmount) {
            var incrementer = new Incrementer() {
                Period = TimeSpan.FromSeconds(periodInSeconds)
            };

            var elapsedTime = TimeSpan.FromSeconds(elapsedTimeInSeconds);

            Assert.That(incrementer.ComputeElapsedIncrements(elapsedTime), Is.EqualTo(expectedIntervalAmount));
        }
    }
}