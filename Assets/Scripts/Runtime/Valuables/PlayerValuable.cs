using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Packages.BrandonUtils.Runtime.Collections;
using Packages.BrandonUtils.Runtime.Exceptions;
using Packages.BrandonUtils.Runtime.Logging;
using Packages.BrandonUtils.Runtime.Testing;
using Packages.BrandonUtils.Runtime.Timing;

using Runtime.Saving;
using Runtime.Utils;

namespace Runtime.Valuables {
    /// <summary>
    /// Holds information about <see cref="ValuableType"/>s that <b>constantly change</b> and are <b>specific to the current player</b>, such as:
    /// <li>What is my <see cref="Rate"/>?</li>
    /// <li>What is my <see cref="KarmaValue"/>?</li>
    /// <li>Can I be generated? (<see cref="CheckGeneration(System.DateTime)"/>)</li>
    /// <li>When was I last generated? (<see cref="LastGenerateCheckTime"/>)</li>
    /// </summary>
    /// <remarks>
    /// The largest role of <see cref="PlayerValuable"/>s is to generate items.
    /// This is composed of several steps:
    /// <li>Find out how long it's been since we last checked for generation (<see cref="InGameTimeSinceLastGenerationCheck"/>)</li>
    /// <li>Limit <see cref="InGameTimeSinceLastGenerationCheck"/>, both based on how much this <see cref="PlayerValuable"/> has been generated (<see cref="GenerateTimeUtilized"/>) and the main <see cref="Hand"/> itself (<see cref="FortuneFountainSaveData.InGameTimeSinceLastThrow"/>)</li>
    /// <li>Calculate the amount of items generated in that time</li>
    /// <li>Add that amount to <see cref="UnresolvedGeneratedItems"/></li>
    /// <li>Call <see cref="ResolveGeneration"/>, <see cref="Grab"/>-bing any completed items and leaving <see cref="UnresolvedGeneratedItems"/> as the remainder</li>
    /// </remarks>
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
        /// Initialized as <see cref="FrameTime.Now"/>.
        /// </remarks>
        [JsonProperty]
        public DateTime LastGenerateCheckTime { get; set; } = FrameTime.Now;

        /// <summary>
        /// The backing field for <see cref="UnresolvedGeneratedItems"/>.
        /// </summary>
        /// <remarks>
        /// I named this prefixed with an underscore because it's one of the _only_ times I'm using a traditional "backing" field, which is in order to do parameter validation against setting <see cref="UnresolvedGeneratedItems"/>.
        /// </remarks>
        [JsonIgnore]
        private double _unresolvedGeneratedItems;

        [JsonProperty]
        public double UnresolvedGeneratedItems {
            get => _unresolvedGeneratedItems;
            set {
                if (value < 0) {
                    throw new FortuneFountainException($"We can't generate negative items ({value})!");
                }

                _unresolvedGeneratedItems = value;
            }
        }

        // ReSharper disable once InconsistentNaming
        private TimeSpan _generateTimeUtilized;

        [JsonProperty]
        public TimeSpan GenerateTimeUtilized {
            get => _generateTimeUtilized;
            set {
                if (value < TimeSpan.Zero) {
                    throw new TimeParadoxException($"Cannot set {nameof(GenerateTimeUtilized)} to be a negative amount ({value})!");
                }

                _generateTimeUtilized = value;
            }
        }

        /// <see cref="Rate" /> converted to a <see cref="TimeSpan" />.
        [JsonIgnore]
        public TimeSpan GenerateInterval {
            get {
                if (Rate < 0) {
                    throw new FortuneFountainException($"The {nameof(Rate)} for {ValuableType} must be positive!");
                }

                return TimeSpan.FromTicks((long) (TimeSpan.TicksPerSecond / Rate));
            }
        }

        [JsonIgnore]
        private Throwable Throwable => new Throwable(ValuableType, KarmaValue);

        public PlayerValuable(ValuableType valuableType) {
            ValuableType = valuableType;

            //subscribing to the Throw event
            Hand.ThrowHandEvent += OnThrow;
        }

        /// <summary>
        ///     Checks how much <b>in-game</b> time has passed since we last called <see cref="CheckGeneration"/>, finds out how many items we could have generated during that time (including fractional items), and adds that amount to <see cref="UnresolvedGeneratedItems"/>.
        /// </summary>
        ///
        /// <param name="now">The time to consider "now" to be - i.e. a controllable replacement for <see cref="DateTime.Now"/> that can be the same across multiple <see cref="CheckGeneration(System.DateTime,System.Nullable{System.TimeSpan})"/> calculations.
        ///
        /// <p>This is <b>required</b> for the instance version of <see cref="CheckGeneration(System.DateTime,System.Nullable{System.TimeSpan})"/>, because it should <b>always</b> be set by the collection version of <see cref="FortuneFountainUtils.CheckGenerate(System.Collections.Generic.IEnumerable{Runtime.Valuables.PlayerValuable},System.Nullable{System.TimeSpan},System.Nullable{System.DateTime})"/>.</p></param>
        ///
        /// <param name="generateLimit">The maximum amount of time that items can be generated. Defaults to <see cref="Hand.GenerateTimeLimit"/> if omitted; ignored if <c>null</c>.
        /// TODO: Due to the implementation of <see cref="FrameTime.Now"/>, the <paramref name="now"/> parameter can be omitted.
        /// </param>
        internal int CheckGeneration(DateTime now, TimeSpan? generateLimit) {
            var unlimitedDuration = InGameTimeSinceLastGenerationCheck(now);
            LastGenerateCheckTime = now;

            var superLimitedDuration = LimitGenerationDuration(unlimitedDuration, generateLimit.GetValueOrDefault(GameManager.SaveData.Hand.GenerateTimeLimit));
            var limitedDuration      = unlimitedDuration;
            if (generateLimit != null) {
                //How much of the generate limit we can still utilize (which is based on the overall hand, not this valuable, since this valuable may have been enabled after the most recent throw)
                var generateLimitRemaining = generateLimit.GetValueOrDefault() - GenerateTimeUtilized;
                LogUtils.Log(
                    $"gen limit = {generateLimit}",
                    $"actual = {generateLimit.GetValueOrDefault()}",
                    $"genutil = {GenerateTimeUtilized}",
                    $"remain = {generateLimitRemaining}"
                );

                limitedDuration = unlimitedDuration.Min(generateLimitRemaining);
            }

            LogUtils.Log(
                $"limitedDuration = {limitedDuration}",
                $"GenerateTimeUtilize = {GenerateTimeUtilized}"
            );

            //Add the limitedDuration to the GenerateTimeUtilized
            GenerateTimeUtilized += limitedDuration;

            var exactAmountGenerated = limitedDuration.Divide(GenerateInterval);

            //Only add to UnresolvedGeneratedItems if we wound up with a positive amount (negative amounts can occur if  generate limit has been reached)
            if (exactAmountGenerated > 0) {
                UnresolvedGeneratedItems += exactAmountGenerated;
            }

            // LogUtils.Log(
            //     $"{nameof(unlimitedDuration)} = {unlimitedDuration}",
            //     $"{nameof(generateTimeUtilized)} = {generateTimeUtilized}",
            //     $"{nameof(GenerateInterval)} = {GenerateInterval}",
            //     $"{nameof(exactAmountGenerated)} = {exactAmountGenerated}",
            //     $"{nameof(UnresolvedGeneratedItems)} = {UnresolvedGeneratedItems}",
            //     "\n"
            //     );

            return ResolveGeneration();
        }

        /// <inheritdoc cref="CheckGeneration(System.Nullable{System.TimeSpan})"/>
        /// <returns></returns>
        internal int CheckGeneration(DateTime now) {
            return CheckGeneration(now, GameManager.SaveData.Hand.GenerateTimeLimit);
        }

        private TimeSpan LimitGenerationDuration(TimeSpan unlimitedDuration, TimeSpan generateLimit) {
            var personalGenerateLimitRemaining = generateLimit - GenerateTimeUtilized;
            var handGenerateLimitRemaining     = generateLimit - GameManager.SaveData.InGameTimeSinceLastThrow;
            return TimeSpan.Zero.Max(unlimitedDuration.Min(personalGenerateLimitRemaining, handGenerateLimitRemaining));
        }

        /// <summary>
        /// <see cref="Grab"/>s the integer component of <see cref="UnresolvedGeneratedItems"/>, setting <see cref="UnresolvedGeneratedItems"/> to the remainder.
        /// </summary>
        /// <example>
        /// if <see cref="UnresolvedGeneratedItems"/> = 1.4:
        /// <li><see cref="ResolveGeneration"/> returns 1</li>
        /// <li><see cref="Grab">Grab(1)</see> called</li>
        /// <li><see cref="UnresolvedGeneratedItems"/> set to 0.4</li>
        /// </example>
        /// <returns>The number of complete items generated.</returns>
        private int ResolveGeneration() {
            //Immediately return if we don't have anything to resolve
            if (UnresolvedGeneratedItems < 1) {
                return 0;
            }

            var integerAmountGenerated = (int) Math.Floor(UnresolvedGeneratedItems);
            var unresolvedRemainder    = UnresolvedGeneratedItems - integerAmountGenerated;

            //Grab the completed items
            this.Grab(integerAmountGenerated);

            //Invoke the GeneratePlayerValuableEvent
            GeneratePlayerValuableEvent?.Invoke(this, integerAmountGenerated);

            //Update UnresolvedGeneratedItems to only contain the remainder that we didn't grab
            UnresolvedGeneratedItems = unresolvedRemainder;

            //Post-condition - ensure that we did actually resolve everything
            //TODO: Is there a "correct" way to check post-conditions? Contract.Ensures is apparently dead: https://github.com/dotnet/docs/issues/6361
            if (UnresolvedGeneratedItems >= 1) {
                throw new FortuneFountainException($"We should have resolved any completed items, leaving us with less than 1 remaining, not {UnresolvedGeneratedItems}!");
            }

            //return the number of items we generated
            return integerAmountGenerated;
        }

        /// <summary>
        /// The amount of time since <see cref="LastGenerateCheckTime"/> that was spent <b>in-game</b>.
        /// </summary>
        /// <returns></returns>
        private TimeSpan InGameTimeSinceLastGenerationCheck(DateTime now) {
            //Check to see if we've completed a generation since loading the game. If not, then we have to do extra calculations using the time from the previous session.

            var isFirstGenerationCheckOfSession = GameManager.SaveData.LastLoadTime > LastGenerateCheckTime;

            //Will hold the final value we're calculating.
            TimeSpan elapsedGenerationTime_total;

            if (isFirstGenerationCheckOfSession) {
                //Calculate the time we spent generating during the previous session (which will always have ENDED with a SAVE)
                var elapsedGenerationTime_previousSession = GameManager.SaveData.LastSaveTime - LastGenerateCheckTime;

                //Calculate the time spent generating during the current session (which will always have STARTED with a LOAD, and ENDED with NOW)
                var elapsedGenerationTime_currentSession = now - GameManager.SaveData.LastLoadTime;

                elapsedGenerationTime_total = elapsedGenerationTime_previousSession + elapsedGenerationTime_currentSession;
            }
            else {
                //If our generation is taking place entirely during the session, then we can ignore SAVE and LOAD times.
                elapsedGenerationTime_total = now - LastGenerateCheckTime;
            }

            if (elapsedGenerationTime_total < TimeSpan.Zero) {
                throw new TimeParadoxException($"How have we spent negative time playing?! ({elapsedGenerationTime_total})");
            }

            if (elapsedGenerationTime_total > GameManager.SaveData.InGameTimeSinceLastThrow) {
                throw new TimeParadoxException(
                    $"We can't have spent more time generating items than we did actually playing the game!!" +
                    $"\n\t{nameof(elapsedGenerationTime_total)} ({ValuableType}) = {elapsedGenerationTime_total}" +
                    $"\n\t{nameof(FortuneFountainSaveData.InGameTimeSinceLastThrow)} = {GameManager.SaveData.InGameTimeSinceLastThrow}" +
                    $"\n\tDifference = {elapsedGenerationTime_total - GameManager.SaveData.InGameTimeSinceLastThrow}" +
                    $"\n\t{nameof(TestUtils.ApproximationTimeThreshold)} = {TestUtils.ApproximationTimeThreshold}"
                );
            }

            return elapsedGenerationTime_total;
        }

        public void Grab(int amount = 1) {
            GameManager.SaveData.Hand.Grab(Throwable, amount);
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public ValuableType PrimaryKey => ValuableType;

        private void OnThrow(Hand hand) {
            this.LastGenerateCheckTime = FrameTime.Now;
            this.GenerateTimeUtilized  = TimeSpan.Zero;
        }
    }
}