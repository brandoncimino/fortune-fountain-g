using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Runtime.Valuables;
using UnityEngine;

namespace Runtime.Saving {
    [Serializable]
    public class Hand {
        /// <summary>
        /// A reference to the <see cref="FortuneFountainSaveData"/> that owns this <see cref="Hand"/>.
        /// </summary>
        private readonly FortuneFountainSaveData _saveData;

        /// <summary>
        /// The <see cref="DateTime.Ticks"/> of the time <see cref="Grab"/> was last called.
        /// <p/>
        /// A value of 0 means that <see cref="Grab"/> was <b>never</b> called.
        /// </summary>
        [SerializeField] private long lastGrabTime;

        /// <summary>
        /// The <see cref="DateTime.Ticks"/> of the time <see cref="Throw"/> was last called.
        /// <p/>
        /// A value of 0 means that <see cref="Throw"/> was <b>never</b> called.
        /// </summary>
        [SerializeField] private long lastThrowTime;

        [SerializeField] [NotNull] public List<Throwable> throwables = new List<Throwable>();

        public Hand(FortuneFountainSaveData saveData) {
            _saveData = saveData;
        }

        public DateTime LastThrowTime {
            get => new DateTime(lastThrowTime);
            set => lastThrowTime = value.Ticks;
        }

        public DateTime LastGrabTime {
            get => new DateTime(lastGrabTime);
            set => lastGrabTime = value.Ticks;
        }

        /// <summary>
        /// Adds an entry to <see cref="throwables"/> with the given <see cref="ValuableType"/> and <c>value</c>.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public void Grab(ValuableType type, double value) {
            LastGrabTime = DateTime.Now;
            throwables.Add(new Throwable(type, value));
        }

        /// <summary>
        /// Removes all entries from <see cref="throwables"/>, adding their <see cref="Throwable.ThrowValue"/> via <see cref="FortuneFountainSaveData.AddKarma"/>
        /// </summary>
        public void Throw() {
            LastThrowTime = DateTime.Now;

            while (throwables.Count > 0) {
                _saveData.AddKarma(throwables[0].ThrowValue);
                throwables.RemoveAt(0);
            }
        }
    }
}