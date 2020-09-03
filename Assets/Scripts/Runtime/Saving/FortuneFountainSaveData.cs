using System;
using Newtonsoft.Json;
using Packages.BrandonUtils.Runtime.Collections;
using Packages.BrandonUtils.Runtime.Saving;
using Packages.BrandonUtils.Runtime.Time;
using Runtime.Valuables;
using UnityEngine;

namespace Runtime.Saving {
    /// <inheritdoc />
    public class FortuneFountainSaveData : SaveData<FortuneFountainSaveData> {
        /// A reference to the player's <see cref="Hand"/>.
        /// <br/>
        /// Must <b>not</b> be <c>readonly</c> for the <see cref="SerializeField"/> attribute to work properly.
        [JsonProperty]
        public Hand Hand;

        [JsonProperty]
        public double Karma;

        /// Holds information about the player's valuables <i>(<b>un-instantiated</b> types of <see cref="Throwable"/>s)</i>, such as upgrades.
        [JsonProperty]
        public KeyedList<ValuableType, PlayerValuable> PlayerValuables = new KeyedList<ValuableType, PlayerValuable>();

        /// The total time spent "out-of-game" since the last <see cref="Saving.Hand.Throw"/>.
        /// <br/>
        /// Equivalent to the <see cref="TimeUtils.Sum"/> of <see cref="OutOfGamePeriodsSinceLastThrow"/>.
        [JsonProperty]
        public TimeSpan OutOfGameTimeSinceLastThrow { get; private set; }

        [JsonIgnore]
        public TimeSpan InGameTimeSinceLastThrow => DateTime.Now - Hand.LastThrowTime - OutOfGameTimeSinceLastThrow;

        public FortuneFountainSaveData() {
            Hand = new Hand();

            //enabling the first PlayerValuable
            PlayerValuables.Add(new PlayerValuable(0));

            //subscribing to events
            Throwable.ThrowSingleEvent += HandleThrowSingleEvent;
            Hand.ThrowHandEvent        += HandleThrowHandEvent;
        }

        public void AddKarma(double amount) {
            Karma += amount;
        }

        /// <summary>
        /// Handles the <see cref="Throwable.ThrowSingleEvent"/>, primarily by calling <see cref="AddKarma"/>.
        /// </summary>
        /// <param name="throwable"></param>
        private void HandleThrowSingleEvent(Throwable throwable) {
            AddKarma(throwable.ThrowValue);
        }

        private void HandleThrowHandEvent(Hand hand) {
            OutOfGameTimeSinceLastThrow = TimeSpan.Zero;
        }

        /// <summary>
        /// <inheritdoc cref="SaveData{T}.OnLoad"/>
        /// <p/>
        /// Adds the time "lost" between <see cref="SaveData{T}.LastLoadTime"/> and <see cref="SaveData{T}.LastSaveTime"/> to <see cref="OutOfGameTimeSinceLastThrow"/>.
        /// </summary>
        protected override void OnLoad() {
            base.OnLoad();
            OutOfGameTimeSinceLastThrow += LastLoadTime - LastSaveTime;
        }
    }
}