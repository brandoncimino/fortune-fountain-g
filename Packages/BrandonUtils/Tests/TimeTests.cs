using System;
using System.Collections.Generic;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Time;

namespace Packages.BrandonUtils.Tests {
    public class TimeTests {
        private static double[] valuesInSeconds = {
            5d,
            0.53,
            2,
            10,
            264576.523,
            7801.623,
            15.623,
            0.123,
            234678.234,
            345.4 * 645.2,
            Math.PI,
            0.1,
            0.01,
            0.001,
            0.00001,
        };

        [Test]
        [Combinatorial]
        public void TestDivision(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))]
            double divisorSeconds
        ) {
            dividendSeconds = TimeUtils.NormalizeSeconds(dividendSeconds);
            divisorSeconds  = TimeUtils.NormalizeSeconds(divisorSeconds);

            Assume.That(divisorSeconds, Is.Not.EqualTo(0), "Checking for division by zero is a different test!");

            var dividend = TimeSpan.FromSeconds(dividendSeconds);
            var divisor  = TimeSpan.FromSeconds(divisorSeconds);

            var expectedDivisionResult = (double) dividend.Ticks / divisor.Ticks;

            Assert.That(dividend.Divide(divisor), Is.EqualTo(expectedDivisionResult));
        }

        [Test]
        public void TestDivisionByZero(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds
        ) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Divide(TimeSpan.Zero));
        }

        [Test]
        [Combinatorial]
        public void TestQuotient(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))]
            double divisorSeconds
        ) {
            dividendSeconds = TimeUtils.NormalizeSeconds(dividendSeconds);
            divisorSeconds  = TimeUtils.NormalizeSeconds(divisorSeconds);

            Assume.That(divisorSeconds, Is.Not.EqualTo(0), "Checking for division by zero is a different test!");

            var dividend = TimeSpan.FromSeconds(dividendSeconds);
            var divisor  = TimeSpan.FromSeconds(divisorSeconds);

            Assert.That(dividend.Quotient(divisor), Is.EqualTo(dividend.Ticks / divisor.Ticks));
        }

        [Test]
        public void TestQuotientByZero(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds
        ) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Quotient(TimeSpan.Zero));
        }

        [Test]
        [Combinatorial]
        public void TestModulusCombinatorial(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))]
            double divisorSeconds
        ) {
            dividendSeconds = TimeUtils.NormalizeSeconds(dividendSeconds);
            divisorSeconds  = TimeUtils.NormalizeSeconds(divisorSeconds);

            Assume.That(divisorSeconds, Is.Not.EqualTo(0), "Checking for division by zero is a different test!");

            var dividend         = TimeSpan.FromSeconds(dividendSeconds);
            var divisor          = TimeSpan.FromSeconds(divisorSeconds);
            var dividendTicks    = dividend.Ticks;
            var divisorTicks     = divisor.Ticks;
            var expectedModTicks = dividendTicks % divisorTicks;
            var expectedModSpan  = TimeSpan.FromTicks(expectedModTicks);

            Assert.That(dividend.Modulus(divisor), Is.EqualTo(expectedModSpan), $"The modulus of {dividend} % {divisor} = {expectedModSpan}\n\tIn Ticks: {dividendTicks} % {divisorTicks} = {expectedModTicks}");
        }

        [Test]
        public void TestModulusByZero(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double dividendSeconds
        ) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Modulus(TimeSpan.Zero));
        }

        [Test]
        public void TestMultiply(
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double multiplicandInSeconds,
            [ValueSource(nameof(valuesInSeconds)), Values(0)]
            double multiplier
        ) {
            var multiplicand                 = TimeSpan.FromSeconds(multiplicandInSeconds);
            var multiplicandTicks            = multiplicand.Ticks;
            var expectedProductTicks         = (long) (multiplicandTicks * multiplier);
            var expectedProductSpanFromTicks = TimeSpan.FromTicks(expectedProductTicks);

            Assert.That(multiplicand.Multiply(multiplier), Is.EqualTo(expectedProductSpanFromTicks));
        }

        [Test, Pairwise]
        public void TestSum(
            [ValueSource(nameof(valuesInSeconds))]
            double a_seconds,
            [ValueSource(nameof(valuesInSeconds))]
            double b_seconds,
            [ValueSource(nameof(valuesInSeconds))]
            double c_seconds
        ) {
            var a_span = TimeSpan.FromSeconds(a_seconds);
            var b_span = TimeSpan.FromSeconds(b_seconds);
            var c_span = TimeSpan.FromSeconds(c_seconds);

            var ls = new List<TimeSpan> {a_span, b_span, c_span};

            Assert.That(ls.Sum(), Is.EqualTo(a_span + b_span + c_span));
        }
    }
}