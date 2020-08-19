using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Packages.BrandonUtils.Runtime.Logging;
using Runtime.Valuables;
using UnityEngine;

namespace Runtime.Saving {
    [Serializable]
    public class Hand {
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

        //Generation-related stuff
        /// The default, immutable value for <see cref="GenerateTimeLimit"/>
        // ReSharper disable once InconsistentNaming
        private static readonly TimeSpan ImmutableGenerateTimeLimit = TimeSpan.FromSeconds(5);

        /// The maximum amount of time between <see cref="Saving.Hand.Throw"/>s that items can be <see cref="Saving.Hand.Grab"/>-ed during before another <see cref="Saving.Hand.Throw"/> is required. Defaults to <see cref="ImmutableGenerateTimeLimit"/>.
        [SerializeField]
        public TimeSpan GenerateTimeLimit => ImmutableGenerateTimeLimit;

        /// <summary>
        /// The <see cref="DateTime"/> when the <see cref="GenerateTimeLimit"/> will be reached.
        /// </summary>
        public DateTime GenerateEndLimit => LastThrowTime + GenerateTimeLimit;

        public double KarmaInHand => throwables.Select(it => it.ThrowValue).Sum();

        public delegate void ThrowHandDelegate(Hand hand);

        public static event ThrowHandDelegate ThrowHandEvent;

        public DateTime LastThrowTime {
            get => new DateTime(lastThrowTime);
            set => lastThrowTime = value.Ticks;
        }

        public DateTime LastGrabTime {
            get => new DateTime(lastGrabTime);
            set => lastGrabTime = value.Ticks;
        }

        public Hand() {
            //Subscribe to the ThrowSingleEvent
            Throwable.ThrowSingleEvent += HandleThrowSingleEvent;
        }

        /// <summary>
        /// Adds <paramref name="throwable"/> to <see cref="throwables"/>.
        /// </summary>
        /// <param name="throwable"></param>
        /// <param name="amount"></param>
        public void Grab(Throwable throwable, int amount = 1) {
            //TODO: Trigger a GRAB event!
            LastGrabTime = DateTime.Now;
            for (var i = 0; i < amount; i++) {
                throwables.Add(throwable);
            }
        }

        /// <summary>
        /// Triggers the <see cref="ThrowHandEvent"/>. which should in turn cause all <see cref="throwables"/> to trigger <see cref="Throwable.ThrowSingleEvent"/>s, removing all entries from <see cref="throwables"/>, adding their <see cref="Throwable.ThrowValue"/> via <see cref="FortuneFountainSaveData.AddKarma"/>
        /// </summary>
        /// <remarks>
        /// Intended design as of 6/25/2020:
        /// <list type="number">
        /// <item>Trigger the .</item>
        /// <item>Cause all <see cref="throwables"/> to trigger <see cref="Throwable.ThrowSingleEvent"/>s.</item>
        /// <item>Consume each <see cref="Throwable.ThrowSingleEvent"/>, removing each <see cref="Throwable"/> from <see cref="throwables"/>.</item>
        /// </list>
        /// </remarks>
        public void Throw() {
            LogUtils.Log($"Setting {nameof(LastThrowTime)} to {DateTime.Now:HH:mm:ss.fff}");
            LastThrowTime = DateTime.Now;
            LogUtils.Log($"{nameof(LastThrowTime)} set to {DateTime.Now:HH:mm:ss.fff}");

            ThrowHandEvent?.Invoke(this);

            LogUtils.Log($"{nameof(ThrowHandEvent)} invocation finished at {DateTime.Now:HH:mm:ss.fff}");
        }

        /// <summary>
        /// Handles the <see cref="Throwable.ThrowSingleEvent"/>.
        /// </summary>
        /// <param name="throwable"></param>
        private void HandleThrowSingleEvent(Throwable throwable) {
            RemoveFromHand(throwable);
        }

        /// <summary>
        /// Removes <paramref name="throwable"/> from <see cref="throwables"/>.
        /// </summary>
        /// <param name="throwable"></param>
        private void RemoveFromHand(Throwable throwable) {
            throwables.Remove(throwable);
        }
    }
}