using System;
using Runtime.Saving;
using UnityEngine;

namespace Runtime.Valuables {
    public class PlayerValuable {
        /// Controls how often checks should be made to generate new items. An interval of 0 will check every <c>update</c>.
        private static readonly TimeSpan GenerateCheckInterval = new TimeSpan(0, 0, 0, 0, 100);

        /// The rate that a given Valuable is generated, measured in items per second.
        public int Rate;

        /// <summary>
        /// The <see cref="DateTime.Ticks"/> of the time the last time-dependent <see cref="Hand.Grab"/> was triggered.
        /// </summary>
        [SerializeField] private long lastGenerateTime;

        //TODO: This "serialize as ticks, but manipulate as a DateTime" concept is something I like and am using a bunch - maybe I can create a "TimeStamp" class for BrandonUtils that is an extension of DateTime, but serializes as Ticks?
        public DateTime LastGenerateTime {
            get => new DateTime(lastGenerateTime);
            set => lastGenerateTime = value.Ticks;
        }

        /// <summary>
        /// <c>true</c> if <see cref="GenerateCheckInterval"/> has passed since <see cref="LastGenerateTime"/>.
        /// </summary>
        private bool ShouldCheckGenerate => DateTime.Now - LastGenerateTime >= GenerateCheckInterval;

        public void CheckGenerate() {
            throw new NotImplementedException("will check if the given valuable needs to be generated, based on the rate");
        }

        public void Generate() {
            throw new NotImplementedException("tbd");
        }
    }
}