using System;
using Runtime.Saving;
using Runtime.Utils;
using UnityEngine;

namespace Runtime.Valuables {
    public class PlayerValuable {
        public readonly ValuableType ValuableType;

        /// <summary>
        ///
        /// Controls how often checks should be made to generate new items. An interval of 0 will check every <c>update</c>.
        /// </summary>
        /// <remarks>
        /// This value applies to <b>all valuables!</b>
        /// It is included in <see cref="PlayerValuable"/> for convenience and clarity: I generally like to keep constants close to the code that depends on them (particularly when they are very specific).
        /// However, I understand that many people prefer to group constants like that into a single location, so that might happen eventually.
        /// </remarks>
        private static readonly TimeSpan GenerateCheckInterval = TimeSpan.FromSeconds(0.1);

        /// The rate that a given Valuable is generated, measured in items per second.
        public double Rate;

        /// The fully calculated karma value of valuables of this type.
        /// TODO: Right now this just returns the <see cref="ValuableModel.ImmutableValue"/>, but this will eventually combine upgrades, etc.
        public double KarmaValue => ValuableDatabase.Models[ValuableType].ImmutableValue;

        /// <summary>
        /// The <see cref="DateTime.Ticks"/> of the time the last time-dependent <see cref="Hand.Grab"/> was triggered.
        /// </summary>
        [SerializeField] private long lastGenerateTime;

        public PlayerValuable(ValuableType valuableType) {
            this.ValuableType = valuableType;
        }

        //TODO: This "serialize as ticks, but manipulate as a DateTime" concept is something I like and am using a bunch - maybe I can create a "TimeStamp" class for BrandonUtils that is an extension of DateTime, but serializes as Ticks?
        public DateTime LastGenerateTime {
            get => new DateTime(lastGenerateTime);
            set => lastGenerateTime = value.Ticks;
        }

        /// <summary>
        /// <c>true</c> if <see cref="GenerateCheckInterval"/> has passed since <see cref="LastGenerateTime"/>.
        /// </summary>
        private bool ShouldCheckGenerate => DateTime.Now - LastGenerateTime >= GenerateCheckInterval;

        /// <see cref="Rate"/> converted to a <see cref="TimeSpan"/>.
        private TimeSpan GenerateInterval => TimeSpan.FromSeconds(1 / Rate);

        private Throwable Throwable => new Throwable(ValuableType, KarmaValue);

        /// <summary>
        /// Checks if this valuable should be generated, based on its <see cref="Rate"/>, and if so, <see cref="Hand.Grab"/>s the appropriate amount and updates <see cref="LastGenerateTime"/>.
        /// </summary>
        /// <remarks>
        /// Relies on <see cref="IncrementalUtils.NumberOfTimesCompleted"/>
        /// </remarks>
        private void CheckGenerate() {
            int numberToGenerate = (int) IncrementalUtils.NumberOfTimesCompleted(
                LastGenerateTime,
                DateTime.Now,
                GenerateInterval,
                out var timeOfGeneration
            );

            if (numberToGenerate > 0) {
                GameManager.SaveData.Hand.Grab(Throwable, numberToGenerate);
                LastGenerateTime = timeOfGeneration;
            }
        }
    }
}