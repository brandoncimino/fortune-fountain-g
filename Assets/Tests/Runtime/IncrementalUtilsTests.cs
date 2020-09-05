using System;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Logging;
using Packages.BrandonUtils.Runtime.Timing;
using Runtime.Utils;

namespace Tests.Runtime {
    public class IncrementalUtilsTests {
        private static double[] seconds = {
            1,
            2,
            3,
            1.1231,
            523456234,
            12341.43123,
            0.001,
            97658765.0001,
            21.123456,
            19,
            Math.PI,
            Math.E,
            TimeSpan.MaxValue.TotalSeconds / 3,
        };

        [Test]
        [Combinatorial]
        public void TestNumberOfTimesCompletedCombinatorial(
            [ValueSource(nameof(seconds)), Values(0)]
            double deltaTimeInSeconds,
            [ValueSource(nameof(seconds))]
            double timeToCompleteInSeconds
        ) {
            Assume.That(timeToCompleteInSeconds, Is.GreaterThan(0), $"Failure conditions with a {nameof(timeToCompleteInSeconds)} ({timeToCompleteInSeconds}) <= 0 are a different test!");

            var deltaTime      = TimeSpan.FromSeconds(deltaTimeInSeconds);
            var timeToComplete = TimeSpan.FromSeconds(timeToCompleteInSeconds);

            var      startTime = DateTime.Today;
            DateTime endTime;

            try {
                endTime = DateTime.Today.AddSeconds(deltaTimeInSeconds);
            }
            catch (ArgumentOutOfRangeException e) {
                throw new IgnoreException($"The given {nameof(deltaTime)} ({deltaTime}), when added to DateTime.Now, would exceed DateTime.MaxValue!", e);
            }

            LogUtils.Log($"test {nameof(startTime)} = {startTime} ({startTime.Ticks})");
            LogUtils.Log($"test {nameof(endTime)} = {endTime} ({endTime.Ticks})");

            var expectedTimesCompleted = deltaTime.Ticks / timeToComplete.Ticks;

            var expectedLastCompletionTime = startTime.Add(timeToComplete.Multiply(expectedTimesCompleted));

            Assert.That(IncrementalUtils.NumberOfTimesCompleted(startTime, endTime, timeToComplete, out var lastCompletionTime), Is.EqualTo(expectedTimesCompleted), $"Incorrect {nameof(IncrementalUtils.NumberOfTimesCompleted)}!");

            Assert.That(lastCompletionTime, Is.EqualTo(expectedLastCompletionTime), $"Incorrect {nameof(lastCompletionTime)} ({lastCompletionTime})!");
        }
    }
}