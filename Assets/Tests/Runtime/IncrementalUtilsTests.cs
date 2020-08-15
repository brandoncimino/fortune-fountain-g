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
            21.123456
        };

        [Test]
        [Combinatorial]
        public void TestNumberOfTimesCompletedCombinatorial([ValueSource(nameof(seconds))] double deltaTimeInSeconds, [ValueSource(nameof(seconds))] double timeToCompleteInSeconds) {
            LogUtils.Log($"{nameof(deltaTimeInSeconds)} = {deltaTimeInSeconds} ~ {TimeUtils.NormalizeSeconds(deltaTimeInSeconds)}");

            deltaTimeInSeconds      = TimeUtils.NormalizeSeconds(deltaTimeInSeconds);
            timeToCompleteInSeconds = TimeUtils.NormalizeSeconds(timeToCompleteInSeconds);

            if (timeToCompleteInSeconds <= 0) {
                Assert.Ignore($"Failure conditions with a {nameof(timeToCompleteInSeconds)} ({timeToCompleteInSeconds}) <= 0 are a different test!");
            }

            var deltaTime      = TimeSpan.FromSeconds(deltaTimeInSeconds);
            var timeToComplete = TimeSpan.FromSeconds(timeToCompleteInSeconds);

            var startTime = DateTime.Today;
            var endTime   = DateTime.Today.AddSeconds(deltaTimeInSeconds);

            LogUtils.Log($"test {nameof(startTime)} = {startTime} ({startTime.Ticks})");
            LogUtils.Log($"test {nameof(endTime)} = {endTime} ({endTime.Ticks})");

            var expectedTimesCompleted = Math.Floor(deltaTimeInSeconds / timeToCompleteInSeconds);
            var modSeconds             = deltaTimeInSeconds % timeToCompleteInSeconds;
            LogUtils.Log($"{deltaTimeInSeconds} % {timeToCompleteInSeconds} = {modSeconds}");
            var expectedLastCompletionTime = startTime.AddSeconds(timeToCompleteInSeconds * expectedTimesCompleted);

            Assert.That(IncrementalUtils.NumberOfTimesCompleted(startTime, endTime, timeToComplete, out var lastCompletionTime), Is.EqualTo(expectedTimesCompleted));

            //to account for minor inaccuracies in floating point math, this checks for a tolerance range
            Assert.That(lastCompletionTime, Is.InRange(expectedLastCompletionTime.AddMilliseconds(-1), expectedLastCompletionTime.AddMilliseconds(1)));
        }
    }
}