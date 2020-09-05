using System;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using Packages.BrandonUtils.Runtime.Testing;
using Packages.BrandonUtils.Runtime.Timing;
using UnityEngine;
using UnityEngine.TestTools;

namespace Packages.BrandonUtils.Tests {
    public class RealTimeTests {
        private static float[] RealTimes = {
            0,
            1,
            2,
            0.5f,
        };

        [Test]
        public void RealTimeNowDoesntChangeWithinFrame(
            [ValueSource(nameof(RealTimes))]
            double secondsToSleep
        ) {
            var startNow = RealTime.Now;

            Thread.Sleep(TimeSpan.FromSeconds(secondsToSleep));

            Assert.That(RealTime.Now, Is.EqualTo(startNow));
            Assert.That(RealTime.Now, Is.Not.Approximately(DateTime.Now));
        }

        [UnityTest]
        public IEnumerator RealTimeNowChangesEveryFrame(
            [Values(5)]
            int framesToCheck
        ) {
            var oldRealTimeNow = RealTime.Now;
            var oldDateTimeNow = DateTime.Now;

            for (int i = 0; i < framesToCheck; i++) {
                //wait for the next frame
                yield return null;

                //Make sure that time has actually passed...
                Assert.That(DateTime.Now, Is.Not.EqualTo(oldDateTimeNow));
                Assert.That(RealTime.Now, Is.Not.EqualTo(oldRealTimeNow));

                oldRealTimeNow = RealTime.Now;
                oldDateTimeNow = DateTime.Now;
            }
        }

        [UnityTest]
        public IEnumerator RealTimeNowIsAccurate(
            [Values(0, 1, 2, Mathf.PI)]
            float secondsToWait
        ) {
            yield return new WaitForSecondsRealtime(secondsToWait);
            Assert.That(RealTime.Now, new ApproximationConstraint(DateTime.Now, TestUtils.ApproximationTimeThreshold));
        }
    }
}