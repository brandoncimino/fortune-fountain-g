using System;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Logging;
using Runtime.Utils;

namespace Tests.Runtime {
    public class IncrementalUtilsTests {
        private static double[] seconds = {
            0,
            1,
            2,
            3,
            1.1231,
            523456234,
            12341.43123,
            0.001,
            97658765.0001,
            0.0001,
            21.123456
        };

        [TestCase(1, 1, 1, 1)]
        [TestCase(2, 1, 2, 2)]
        [TestCase(7.25, 0.25, 21, 7.25)]
        [TestCase(5.1324, 1, 0, 0)]
        [TestCase(2000, 1, 2000, 2000)]
        [TestCase(54, 99, 0, 0)]
        [TestCase(3, 2, 1, 2)]
        public void TestNumberOfTimesCompleted(
            double deltaTimeInSeconds,
            double timeToCompleteInSeconds,
            int expectedNumberOfCompletions,
            double expectedLastCompletionOffsetInSeconds
        ) {
            //"Normalizing" the minute values so that they match the correct TimeSpan precision
            deltaTimeInSeconds = Time.NormalizeSeconds(deltaTimeInSeconds);
            timeToCompleteInSeconds = Time.NormalizeSeconds(timeToCompleteInSeconds);

            TestNumberOfTimesCompleted(
                DateTime.Today,
                DateTime.Today.AddSeconds(deltaTimeInSeconds),
                TimeSpan.FromSeconds(timeToCompleteInSeconds),
                expectedNumberOfCompletions,
                DateTime.Today.AddSeconds(expectedLastCompletionOffsetInSeconds)
            );
        }

        private static void TestNumberOfTimesCompleted(
            DateTime startTime,
            DateTime endTime,
            TimeSpan duration,
            int expectedNumberOfCompletions,
            DateTime expectedLastCompletionTime
        ) {
            Assert.That(
                IncrementalUtils.NumberOfTimesCompleted(startTime, endTime, duration, out var lastCompletionTime),
                Is.EqualTo(expectedNumberOfCompletions)
            );

            Assert.That(lastCompletionTime, Is.EqualTo(expectedLastCompletionTime));
        }

        [Test]
        [Combinatorial]
        public void TestNumberOfTimesCompletedCombinatorial(
            [ValueSource(nameof(seconds))] double deltaTimeInSeconds,
            [ValueSource(nameof(seconds))] double timeToCompleteInSeconds
        ) {
            LogUtils.Log($"{nameof(deltaTimeInSeconds)} = {deltaTimeInSeconds} ~ {Time.NormalizeSeconds(deltaTimeInSeconds)}");

            deltaTimeInSeconds = Time.NormalizeSeconds(deltaTimeInSeconds);
            timeToCompleteInSeconds = Time.NormalizeSeconds(timeToCompleteInSeconds);

            if (timeToCompleteInSeconds == 0) Assert.Ignore($"Failure conditions with a {nameof(timeToCompleteInSeconds)} of <= 9 are a different test!");

            var deltaTime = TimeSpan.FromSeconds(deltaTimeInSeconds);
            var timeToComplete = TimeSpan.FromSeconds(timeToCompleteInSeconds);

            var startTime = DateTime.Today;
            var endTime = DateTime.Today.AddSeconds(deltaTimeInSeconds);

            LogUtils.Log($"test {nameof(startTime)} = {startTime} ({startTime.Ticks})");
            LogUtils.Log($"test {nameof(endTime)} = {endTime} ({endTime.Ticks})");

            var expectedTimesCompleted = Math.Floor(deltaTimeInSeconds / timeToCompleteInSeconds);
            var modSeconds = deltaTimeInSeconds % timeToCompleteInSeconds;
            LogUtils.Log($"{deltaTimeInSeconds} % {timeToCompleteInSeconds} = {modSeconds}");
            var expectedLastCompletionTime = startTime.AddSeconds(timeToCompleteInSeconds * expectedTimesCompleted);

            Assert.That(
                IncrementalUtils.NumberOfTimesCompleted(
                    startTime,
                    endTime,
                    timeToComplete,
                    out var lastCompletionTime
                ),
                Is.EqualTo(expectedTimesCompleted)
            );

            //to account for minor inaccuracies in floating point math, this checks for a tolerance range
            Assert.That(
                lastCompletionTime,
                Is.InRange(
                    expectedLastCompletionTime.AddMilliseconds(-1),
                    expectedLastCompletionTime.AddMilliseconds(1)
                )
            );
        }
    }
}