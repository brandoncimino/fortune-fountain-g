using System;
using System.Collections.Generic;
using Packages.BrandonUtils.Runtime.Saving;
using Runtime.Valuables;
using UnityEngine;

namespace Runtime.Saving {
    /// <inheritdoc />
    [Serializable]
    public class FortuneFountainSaveData : SaveData<FortuneFountainSaveData> {
        /// A reference to the player's <see cref="Hand"/>.
        /// <br/>
        /// Must <b>not</b> be <c>readonly</c> for the <see cref="SerializeField"/> attribute to work properly.
        [SerializeField] public Hand Hand;

        public double Karma;

        //Generation-related stuff

        /// The default, immutable value for <see cref="GrabTimeLimit"/>
        private const long GrabTimeLimit_Default = 5;

        /// The maximum amount of time between <see cref="Saving.Hand.Throw"/>s that items can be <see cref="Saving.Hand.Grab"/>-ed during before another <see cref="Saving.Hand.Throw"/> is required. Defaults to <see cref="GrabTimeLimit_Default"/>.
        public float GrabTimeLimit = GrabTimeLimit_Default;

        /// Holds information about the player's valuables <i>(<b>un-instantiated</b> types of <see cref="Throwable"/>s)</i>, such as upgrades.
        public Dictionary<ValuableType, PlayerValuable> Valuables;

        public FortuneFountainSaveData() {
            Hand = new Hand();

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