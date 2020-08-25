using System.Collections.Generic;
using Newtonsoft.Json;
using Packages.BrandonUtils.Runtime.Saving;
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
        public Dictionary<ValuableType, PlayerValuable> PlayerValuables = new Dictionary<ValuableType, PlayerValuable>();

        public FortuneFountainSaveData() {
            Hand = new Hand();

            //enabling the first PlayerValuable
            const ValuableType firstValuableType = (ValuableType) 0;
            PlayerValuables.Add(firstValuableType, new PlayerValuable(firstValuableType));

            //subscribing to events
            Throwable.ThrowSingleEvent += HandleThrowSingleEvent;
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
    }
}