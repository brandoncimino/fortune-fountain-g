using System;
using System.Collections.Generic;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;

namespace Packages.BrandonUtils.Tests {
    public class TimeTests {
        private class DivTest {
            public double dividendSeconds;
            public double divisorSeconds;

            public TimeSpan dividend => TimeSpan.FromSeconds(dividendSeconds);
            public TimeSpan divisor => TimeSpan.FromSeconds(divisorSeconds);

            public DivTest(double dividendSeconds, double divisorSeconds) {
                this.dividendSeconds = dividendSeconds;
                this.divisorSeconds = divisorSeconds;
            }
        }

        /// <summary>
        /// NOTE: There are limits to these values based on the precision of TimeSpan.Seconds
        /// </summary>
        private List<DivTest> DivTestData = new List<DivTest> {
            new DivTest(5d, 2),
            new DivTest(0d, 10),
            new DivTest(0.53, 2),
            new DivTest(264576.523, 7801.623)
        };

        [Test]
        public void TestDivision() {
            foreach (var test in DivTestData) {
                Assert.That(test.dividend.Divide(test.divisor), Is.EqualTo(test.dividendSeconds / test.divisorSeconds));
            }
        }

        [Test]
        public void TestQuotient() {
            foreach (var test in DivTestData) {
                Assert.That(test.dividend.Quotient(test.divisor), Is.EqualTo(Math.Floor(test.dividendSeconds / test.divisorSeconds)));
            }
        }

        [Test]
        public void TestModulus() {
            foreach (var test in DivTestData) {
                var expectedModulus = TimeSpan.FromSeconds(test.dividendSeconds % test.divisorSeconds);
                Assert.That(
                    test.dividend.Modulus(test.divisor),
                    Is.EqualTo(expectedModulus),
                    $"The modulus of {test.dividendSeconds} % {test.divisorSeconds} = {expectedModulus}"
                );
            }
        }
    }
}