﻿using System;
using System.Collections.Generic;
using System.Linq;

using BrandonUtils.Timing;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Runtime.Valuables;

namespace Runtime.Saving {
    public class Hand {
        [JsonProperty]
        [NotNull]
        public List<Throwable> throwables = new List<Throwable>();

        public IEnumerable<IGrouping<ValuableType, Throwable>> GroupedThrowables => throwables.GroupBy(throwable => throwable.ValuableType);

        //Generation-related stuff
        /// The default, immutable value for <see cref="GenerateTimeLimit"/>
        // ReSharper disable once InconsistentNaming
        [JsonIgnore]
        private static readonly TimeSpan ImmutableGenerateTimeLimit = TimeSpan.FromSeconds(5);

        /// The maximum amount of time between <see cref="Saving.Hand.Throw"/>s that items can be <see cref="Saving.Hand.Grab"/>-ed during before another <see cref="Saving.Hand.Throw"/> is required. Defaults to <see cref="ImmutableGenerateTimeLimit"/>.
        [JsonProperty]
        public TimeSpan GenerateTimeLimit = ImmutableGenerateTimeLimit;

        [JsonProperty]
        public double KarmaInHand => throwables.Sum(it => it.ThrowValue);

        public delegate void ThrowHandDelegate(Hand hand);

        public static event ThrowHandDelegate ThrowHandEvent;

        [JsonProperty]
        public DateTime LastThrowTime { get; set; } = FrameTime.Now;

        [JsonProperty]
        public DateTime LastGrabTime { get; set; } = FrameTime.Now;

        [JsonIgnore]
        public Dictionary<ValuableType, int> ValuableTypeCounts =>
            throwables.GroupBy(
                          throwable => throwable.ValuableType
                      )
                      .ToDictionary(
                          group => group.Key,
                          group => group.Count()
                      );

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
            LastGrabTime = FrameTime.Now;
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
            LastThrowTime = FrameTime.Now;
            ThrowHandEvent?.Invoke(this);
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