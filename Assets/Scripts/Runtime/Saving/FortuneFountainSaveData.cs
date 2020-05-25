using System;
using Packages.BrandonUtils.Runtime.Saving;
using UnityEngine;

namespace Runtime.Saving
{
    /// <inheritdoc />
    [Serializable]
    public class FortuneFountainSaveData : SaveData<FortuneFountainSaveData>
    {
        /// <summary>
        /// A reference to the player's <see cref="Hand"/>.
        /// <br/>
        /// Must <b>not</b> be <c>readonly</c> for the <see cref="SerializeField"/> attribute to work properly.
        /// </summary>
        [SerializeField] public Hand Hand;

        public double Karma;

        public FortuneFountainSaveData()
        {
            Hand = new Hand(this);
        }

        public void AddKarma(double amount)
        {
            Karma += amount;
        }
    }
}