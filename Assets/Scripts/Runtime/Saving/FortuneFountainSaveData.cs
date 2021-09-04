using System;
using System.Runtime.Serialization;
using BrandonUtils.Saving;
using BrandonUtils.Standalone.Collections;
using BrandonUtils.Standalone.Exceptions;
using BrandonUtils.Timing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Runtime.Valuables;
using UnityEngine;

namespace Runtime.Saving {
    /// <inheritdoc />
    [JsonObject(IsReference = true)]
    public class FortuneFountainSaveData : SaveData<FortuneFountainSaveData> {
        /// A reference to the player's <see cref="Hand"/>.
        /// <br/>
        /// Must <b>not</b> be <c>readonly</c> for the <see cref="SerializeField"/> attribute to work properly.
        [JsonProperty] [NotNull] public Hand Hand;

        [JsonProperty] public double Karma;

        /// Holds information about the player's valuables <i>(<b>un-instantiated</b> types of <see cref="Throwable"/>s)</i>, such as upgrades.
        [JsonProperty]
        public KeyedList<ValuableType, PlayerValuable> PlayerValuables =
            new PrimaryKeyedList<ValuableType, PlayerValuable>();

        /// The total time spent "out-of-game" since the last <see cref="Saving.Hand.Throw"/>.
        [JsonProperty]
        public TimeSpan OutOfGameTimeSinceLastThrow { get; private set; }

        [JsonIgnore]
        public TimeSpan InGameTimeSinceLastThrow => FrameTime.Now - Hand.LastThrowTime - OutOfGameTimeSinceLastThrow;

        public FortuneFountainSaveData([NotNull] string nickname) : base(nickname) {
            Hand = new Hand(this);
            Hand.ThrowHandEvent += HandleThrowHandEvent;
            CurrentJsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        [OnDeserialized]
        protected void OnDeserialization(StreamingContext streamingContext) {
            FixHierarchy();
        }

        internal void FixHierarchy() {
            Hand.MySaveData = this;
        }

        public void AddKarma(double amount) {
            Karma += amount;
        }

        /// <summary>
        /// <inheritdoc cref="SaveData{T}.OnLoad"/>
        /// <p/>
        /// Adds the time "lost" between <see cref="SaveData{T}.LastLoadTime"/> and <see cref="SaveData{T}.LastSaveTime"/> to <see cref="OutOfGameTimeSinceLastThrow"/>.
        /// </summary>
        protected override void OnLoad() {
            base.OnLoad();
            OutOfGameTimeSinceLastThrow += GetOutOfGameTime();
        }

        private TimeSpan GetOutOfGameTime() {
            try {
                // TODO: Add an extension method BrandonUtils that throws nice exceptions if a nullable is empty
                if (!LastLoadTime.HasValue) {
                    throw new ArgumentNullException(nameof(LastLoadTime));
                }

                if (!LastSaveTime.HasValue) {
                    throw new ArgumentNullException(nameof(LastSaveTime));
                }

                return LastLoadTime.Value - LastSaveTime.Value;
            }
            catch (Exception e) {
                throw new TimeParadoxException($"Couldn't calculate the {nameof(GetOutOfGameTime)}!", e);
            }
        }

        protected void HandleThrowHandEvent(Hand handThatThrew) {
            ResetOutOfGameTimeSinceLastThrow();
        }

        protected void ResetOutOfGameTimeSinceLastThrow() {
            OutOfGameTimeSinceLastThrow = TimeSpan.Zero;
        }
    }
}