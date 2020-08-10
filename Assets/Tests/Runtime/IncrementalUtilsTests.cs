using System;
using NUnit.Framework;
using Runtime.Utils;

namespace Tests.Runtime {
    public class IncrementalUtilsTests {
        [TestCase(0, 1, 1, 1, 1)]
        [TestCase(0, 2, 1, 2, 2)]
        [TestCase(2.5, 7.25, 0.25, 21, 7.25)]
        [TestCase(0, -5, 1, 0, 0)]
        [TestCase(0, 2000, 1, 2000, 0)]
        [TestCase(0, 54, 99, 0, 0)]
        public void TestNumberOfTimesCompleted(
            double startOffsetInMinutes,
            double endOffsetInMinutes,
            double durationInMinutes,
            int expectedNumberOfCompletions,
            double expectedLastCompletionOffsetInMinutes
        ) {
            TestNumberOfTimesCompleted(
                DateTime.Today.AddMinutes(startOffsetInMinutes),
                DateTime.Today.AddMinutes(endOffsetInMinutes),
                TimeSpan.FromMinutes(durationInMinutes),
                expectedNumberOfCompletions,
                DateTime.Today.AddMinutes(expectedLastCompletionOffsetInMinutes)
            );
        }

        private static void TestNumberOfTimesCompleted(
            DateTime startTime,
            DateTime endTime,
            TimeSpan duration,
            int expectedNumberOfCompletions,
            DateTime expectedLastCompletionOffsetInMinutes
        ) {
            Assert.That(
                IncrementalUtils.NumberOfTimesCompleted(startTime, endTime, duration, out var lastCompletionTime),
                Is.EqualTo(expectedNumberOfCompletions)
            );

            Assert.That(lastCompletionTime, Is.EqualTo(expectedLastCompletionOffsetInMinutes));
        }
    }
}