using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Collections;
using Runtime.Saving;
using Runtime.Utils;

namespace Runtime.Valuables {
    public class PlayerValuable : IPrimaryKeyed<ValuableType> {
        public delegate void GeneratePlayerValuableDelegate(PlayerValuable playerValuable, int count);

        public static event GeneratePlayerValuableDelegate GeneratePlayerValuableEvent;

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ValuableType ValuableType { get; private set; }

        /// The rate that a given Valuable is generated, measured in items per second.
        /// TODO: Currently defaults to 1, but will eventually combine upgrades, etc.
        [JsonProperty]
        public double Rate { get; set; } = 1;

        /// The fully calculated karma value of valuables of this type.
        /// TODO: Right now this just returns the <see cref="ValuableModel.ImmutableValue" />, but this will eventually combine upgrades, etc.
        [JsonProperty]
        public double KarmaValue => ValuableDatabase.Models[ValuableType].ImmutableValue;

        /// <summary>
        ///     The <see cref="DateTime" /> that the time the last time-dependent <see cref="Hand.Grab" /> was triggered.
        /// </summary>
        /// <remarks>
        /// Initialized as <see cref="DateTime.Now"/>.
        /// </remarks>
        [JsonProperty]
        public DateTime LastGenerateTime { get; set; } = DateTime.Now;

        /// <see cref="Rate" /> converted to a <see cref="TimeSpan" />.
        [JsonIgnore]
        public TimeSpan GenerateInterval {
            get {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Rate == 0) {
                    throw new ArgumentException("The rate for this item is 0!");
                }

                return TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / Rate));
            }
        }

        [JsonIgnore]
        private Throwable Throwable => new Throwable(ValuableType, KarmaValue);

        public PlayerValuable(ValuableType valuableType) {
            ValuableType = valuableType;
        }

        /// <summary>
        ///     Checks if this valuable should be generated, based on its <see cref="Rate" />, and if so, <see cref="Hand.Grab" />s the appropriate amount and updates <see cref="LastGenerateTime" />.
        /// </summary>
        /// <param name="generateLimitOverride">The maximum amount of time that items can be generated. Defaults to <see cref="Hand.GenerateTimeLimit"/> if omitted <b><i>or <c>null</c></i></b>.</param>
        /// <remarks>
        ///     Relies on <see cref="IncrementalUtils.NumberOfTimesCompleted" />
        /// </remarks>
        public int CheckGenerate(TimeSpan? generateLimitOverride = null) {
            //Check to see if we've completed a generation since loading the game. If not, then we have to do extra calculations using the time from the previous session.
            var partialGenerationDuringPreviousSession = GameManager.SaveData.LastLoadTime > LastGenerateTime;
            //Will hold the maximum amount of time that we _could_ have generated for.
            TimeSpan unlimitedDuration;

            if (partialGenerationDuringPreviousSession) {
                //Calculate the time we spent generating during the previous session (which will always have ENDED with a SAVE)
                var previousSessionDuration = GameManager.SaveData.LastSaveTime - LastGenerateTime;

                //Calculate the time spent generating during the current session (which will always have STARTED with a LOAD, and ENDED with NOW)
                var currentSessionDuration = DateTime.Now - GameManager.SaveData.LastLoadTime;

                unlimitedDuration = previousSessionDuration + currentSessionDuration;
            }
            else {
                //If our generation is taking place entirely during the session, then we can ignore SAVE and LOAD times.
                unlimitedDuration = DateTime.Now - LastGenerateTime;
            }

            //Resolve the actualGenerateLimit, based on whether "null" was passed or not
            var actualGenerateLimit = generateLimitOverride.GetValueOrDefault(GameManager.SaveData.Hand.GenerateTimeLimit);

            //limit the duration to actualGenerateLimit
            var limitedDuration = unlimitedDuration.Min(actualGenerateLimit);

            //record whether or not we reached the generate limit (because we need to do things differently if we did)
            var durationLimitReached = actualGenerateLimit < unlimitedDuration;

            //calculate the number of items to generate, finally!
            var numberToGenerate = (int) IncrementalUtils.NumberOfTimesCompleted(limitedDuration, this.GenerateInterval, out TimeSpan timeRefunded);

            //Check if we actually need to generate anything
            if (numberToGenerate > 0) {
                //Grab the items
                this.Grab(numberToGenerate);

                //Set the "LastGenerateTime" - aka the "time of last completion" - which is DateTime.Now IF we've surpassed the duration limit
                LastGenerateTime = durationLimitReached ? DateTime.Now : DateTime.Now - timeRefunded;

                //Invoke the GeneratePlayerValuableEvent
                GeneratePlayerValuableEvent?.Invoke(this, numberToGenerate);
            }

            //return the number of items we generated
            return numberToGenerate;
        }

        public void Grab(int amount = 1) {
            GameManager.SaveData.Hand.Grab(Throwable, amount);
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public ValuableType PrimaryKey => ValuableType;
    }
}