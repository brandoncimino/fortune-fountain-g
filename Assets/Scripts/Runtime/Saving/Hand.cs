using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using BrandonUtils.Logging;
using BrandonUtils.Standalone.Collections;
using BrandonUtils.Timing;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Runtime.Valuables;

namespace Runtime.Saving {
    public class Hand {
        [JsonProperty] [NotNull] [ItemNotNull] internal List<Throwable> _throwables = new List<Throwable>();

        [JsonIgnore]
        [NotNull]
        [ItemNotNull]
        public ReadOnlyCollection<Throwable> Throwables => _throwables.AsReadOnly();

        [JsonIgnore]
        [NotNull]
        public List<ThrowableValuable> ThrowableValuables =>
            Throwables.Select(it => it as ThrowableValuable).Where(it => it != null).ToList();

        //Generation-related stuff
        /// The default, immutable value for <see cref="GenerateTimeLimit"/>
        [JsonIgnore] private static readonly TimeSpan ImmutableGenerateTimeLimit = TimeSpan.FromSeconds(5);

        /// The maximum amount of time between <see cref="Saving.Hand.Throw"/>s that items can be generated during before another <see cref="Saving.Hand.Throw"/> is required. Defaults to <see cref="ImmutableGenerateTimeLimit"/>.
        [JsonProperty] public TimeSpan GenerateTimeLimit = ImmutableGenerateTimeLimit;

        [JsonProperty]
        public double KarmaInHand => Throwables.Sum(
            it => {
                if (it is ThrowableValuable tv) {
                    return tv.PresentValue;
                }

                return 0;
            });

        [NotNull] public event Action<Hand> ThrowHandEvent = hand => { };

        [JsonIgnore]
        // [NotNull]
        public FortuneFountainSaveData MySaveData { get; internal set; }

        [OnDeserialized]
        protected void OnDeserialized(StreamingContext streamingContext) {
            FixHierarchy();
        }

        internal void FixHierarchy() {
            foreach (var t in Throwables) {
                t.MyHand = this;
            }
        }

        public Hand([NotNull] FortuneFountainSaveData mySaveData) {
            MySaveData = mySaveData;
        }

        [JsonProperty] public DateTime LastThrowTime { get; set; } = FrameTime.Now;

        /// <summary>
        /// Triggers the <see cref="ThrowHandEvent"/>. which should in turn cause all <see cref="Throwables"/> to trigger <see cref="Throwable.ThrowSingleEvent"/>s, removing all entries from <see cref="Throwables"/>, adding their <see cref="ThrowableValuable.PresentValue"/> via <see cref="FortuneFountainSaveData.AddKarma"/>
        /// </summary>
        /// <remarks>
        /// Intended design as of 6/25/2020:
        /// <list type="number">
        /// <item>Trigger the .</item>
        /// <item>Cause all <see cref="Throwables"/> to trigger <see cref="Throwable.ThrowSingleEvent"/>s.</item>
        /// <item>Consume each <see cref="Throwable.ThrowSingleEvent"/>, removing each <see cref="Throwable"/> from <see cref="Throwables"/>.</item>
        /// </list>
        /// Intended design as of 6/24/2021:
        /// <b>HOLY SHIT THAT'S SPOOKY</b>
        /// </remarks>
        public void Throw() {
            LastThrowTime = FrameTime.Now;
            ThrowHandEvent?.Invoke(this);
            ThrowAll();
        }

        private void RemoveFromHand([NotNull] Throwable throwable) {
            Validate_MustBeHeld(throwable);

            _throwables.Remove(throwable);
        }

        internal void AddToHand([NotNull] Throwable throwable) {
            if (throwable._hand != null && !ReferenceEquals(throwable._hand, this)) {
                throw new ArgumentException(
                    $"Cannot add that {nameof(Throwable)} to the {nameof(Hand)} because it's already in another {nameof(Hand)}!");
            }

            if (Throwables.Contains(throwable)) {
                throw new ArgumentException(
                    $"Cannot add that {nameof(Throwable)} to the {nameof(Hand)} because it's already IN the {nameof(Hand)}!");
            }

            throwable.MyHand = this;
            _throwables.Add(throwable);
        }

        /// <summary>
        /// Calls each of the <see cref="Throwables"/>' <see cref="Throwable.Throw"/> method, then clears the <see cref="Throwables"/> list of everything that is <see cref="Throwable.HasBeenRedeemed"/>
        /// </summary>
        private void ThrowAll() {
            LogUtils.Log($"Flicking {Throwables.Count} items:\n{Throwables.JoinLines()}");
            var throwablesCopy = Throwables.Copy().Where(it => it.CanBeRedeemed()).ToList();
            foreach (var throwable in throwablesCopy) {
                Flick(throwable);
            }

            LogUtils.Log("Done flicking");
        }

        public void Flick([NotNull] Throwable throwable) {
            LogUtils.Log($"flicking {throwable}");
            Validate_MustBeHeld(throwable);

            throwable.Redeem();
            RemoveFromHand(throwable);
        }


        private void Validate_MustBeHeld(Throwable throwable) {
            if (Throwables.Contains(throwable) == false) {
                throw new ArgumentOutOfRangeException($"That {nameof(Throwable)} isn't in this {nameof(Hand)}!");
            }
        }
    }
}