using System;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Packages.BrandonUtils.Tests {
    public class TimeTests {
        private static double[] valuesInSeconds = {
            5d,
            0d,
            0.53,
            2,
            10,
            264576.523,
            7801.623,
            15.623,
            0.123,
            234678.234,
            345.4 * 645.2
        };

        [Test]
        [Combinatorial]
        public void TestDivision(
            [ValueSource(nameof(valuesInSeconds))] double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))] double divisorSeconds
        ) {
            dividendSeconds = Time.NormalizeSeconds(dividendSeconds);
            divisorSeconds = Time.NormalizeSeconds(divisorSeconds);

            if (divisorSeconds == 0) Assert.Ignore("Checking for division by zero is a different test!");

            var dividend = TimeSpan.FromSeconds(dividendSeconds);
            var divisor = TimeSpan.FromSeconds(divisorSeconds);

            Assert.That(dividend.Divide(divisor), Is.EqualTo(dividendSeconds / divisorSeconds));
        }

        [Test]
        public void TestDivisionByZero([ValueSource(nameof(valuesInSeconds))] double dividendSeconds) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Divide(TimeSpan.Zero));
        }

        [Test]
        [Combinatorial]
        public void TestQuotient(
            [ValueSource(nameof(valuesInSeconds))] double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))] double divisorSeconds
        ) {
            dividendSeconds = Time.NormalizeSeconds(dividendSeconds);
            divisorSeconds = Time.NormalizeSeconds(divisorSeconds);

            if (divisorSeconds == 0) Assert.Ignore("Checking for division by zero is a different test!");

            var dividend = TimeSpan.FromSeconds(dividendSeconds);
            var divisor = TimeSpan.FromSeconds(divisorSeconds);

            Assert.That(dividend.Quotient(divisor), Is.EqualTo(Math.Floor(dividendSeconds / divisorSeconds)));
        }

        [Test]
        public void TestQuotientByZero([ValueSource(nameof(valuesInSeconds))] double dividendSeconds) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Quotient(TimeSpan.Zero));
        }

        [Test]
        [Combinatorial]
        public void TestModulusCombinatorial(
            [ValueSource(nameof(valuesInSeconds))] double dividendSeconds,
            [ValueSource(nameof(valuesInSeconds))] double divisorSeconds
        ) {
            dividendSeconds = Time.NormalizeSeconds(dividendSeconds);
            divisorSeconds = Time.NormalizeSeconds(divisorSeconds);

            if (divisorSeconds == 0) Assert.Ignore("The divisor must not be 0 - that's a different test.");

            var dividend = TimeSpan.FromSeconds(dividendSeconds);
            var divisor = TimeSpan.FromSeconds(divisorSeconds);
            var expectedModSeconds = dividend.TotalSeconds % divisor.TotalSeconds;
            var expectedModSpan = TimeSpan.FromSeconds(expectedModSeconds);

            Assert.That(
                dividend.Modulus(divisor),
                Is.EqualTo(expectedModSpan),
                $"The modulus of {dividend} % {divisor} = {expectedModSpan}"
            );
        }

        [Test]
        public void TestModulusByZero([ValueSource(nameof(valuesInSeconds))] double dividendSeconds) {
            Assert.Throws<DivideByZeroException>(() => TimeSpan.FromSeconds(dividendSeconds).Modulus(TimeSpan.Zero));
        }
    }
}