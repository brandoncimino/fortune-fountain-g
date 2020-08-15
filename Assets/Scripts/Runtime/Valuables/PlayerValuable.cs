using System;
using Runtime.Saving;
using Runtime.Utils;
using UnityEngine;

namespace Runtime.Valuables {
    public class PlayerValuable {
        public delegate void GeneratePlayerValuableDelegate(PlayerValuable playerValuable);

        public readonly ValuableType ValuableType;

        /// <summary>
        ///     The <see cref="DateTime.Ticks" /> of the time the last time-dependent <see cref="Hand.Grab" /> was triggered.
        /// </summary>
        [SerializeField] private long lastGenerateTime;

        /// The rate that a given Valuable is generated, measured in items per second.
        /// TODO: Currently defaults to 1, but will eventually combine upgrades, etc.
        public double Rate { get; set; } = 1;

        public PlayerValuable(ValuableType valuableType) {
            ValuableType     = valuableType;
            LastGenerateTime = DateTime.Now;
        }

        /// The fully calculated karma value of valuables of this type.
        /// TODO: Right now this just returns the <see cref="ValuableModel.ImmutableValue" />, but this will eventually combine upgrades, etc.
        public double KarmaValue => ValuableDatabase.Models[ValuableType].ImmutableValue;

        //TODO: This "serialize as ticks, but manipulate as a DateTime" concept is something I like and am using a bunch - maybe I can create a "TimeStamp" class for BrandonUtils that is an extension of DateTime, but serializes as Ticks?
        public DateTime LastGenerateTime {
            get => new DateTime(lastGenerateTime);
            set => lastGenerateTime = value.Ticks;
        }

        /// <see cref="Rate" /> converted to a <see cref="TimeSpan" />.
        public TimeSpan GenerateInterval {
            get {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Rate == 0) throw new ArgumentException("The rate for this item is 0!");
                return TimeSpan.FromSeconds(1 / Rate);
            }
        }

        private Throwable Throwable => new Throwable(ValuableType, KarmaValue);

        public event GeneratePlayerValuableDelegate GeneratePlayerValuableEvent;

        /// <summary>
        ///     Checks if this valuable should be generated, based on its <see cref="Rate" />, and if so, <see cref="Hand.Grab" />s the appropriate amount and updates <see cref="LastGenerateTime" />.
        /// </summary>
        /// <remarks>
        ///     Relies on <see cref="IncrementalUtils.NumberOfTimesCompleted" />
        /// </remarks>
        public void CheckGenerate() {
            var numberToGenerate = (int) IncrementalUtils.NumberOfTimesCompleted(LastGenerateTime, DateTime.Now, GenerateInterval, out var timeOfGeneration);

            if (numberToGenerate > 0) {
                GameManager.SaveData.Hand.Grab(Throwable, numberToGenerate);
                LastGenerateTime = timeOfGeneration;
                GeneratePlayerValuableEvent?.Invoke(this);
            }
        }

        public override string ToString() {
            return JsonUtility.ToJson(this);
        }
    }
}