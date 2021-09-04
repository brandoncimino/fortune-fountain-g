using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Runtime.Saving;

namespace Runtime.Valuables {
    [Serializable]
    public abstract class Throwable {
        private readonly Guid ID = Guid.NewGuid();

        [JsonIgnore] [CanBeNull] internal Hand _hand;

        [JsonIgnore]
        [NotNull]
        public Hand MyHand {
            get => _hand ??
                   throw new NullReferenceException(
                       $"This {nameof(Throwable)} isn't currently assigned to a {nameof(Hand)}! {this}");
            internal set {
                if (_hand != null) {
                    throw new ArgumentException(
                        $"Cannot set {nameof(MyHand)} because it's already set to another {nameof(Hand)}!");
                }

                _hand = value;
            }
        }

        /// <summary>
        /// A flag to prevent a single throwable from ever being thrown more than once.
        /// </summary>
        /// <remarks>
        /// <li>Just because the <see cref="Throwable"/> has been removed from <see cref="Hand.Throwables"/>, it <b>still exists</b></li>
        /// <li>This is due to the intricacies of garbage collection, probably...which I don't know much about</li>
        /// <li>We may run into limitations with this boolean flag</li>
        /// <li>One alternative is to have <see cref="Throwable"/> implement <see cref="IDisposable"/>, and dispose of the <see cref="Throwable"/> when we call <see cref="Throwable.Throw"/></li>
        /// </remarks>
        [JsonIgnore]
        public bool HasBeenRedeemed { get; private set; }

        public void Flick() {
            MyHand.Flick(this);
        }

        public virtual void Redeem() {
            if (!CanBeRedeemed()) {
                throw new FortuneFountainException(
                    $"The {nameof(Throwable)} {this} has already been thrown ({nameof(HasBeenRedeemed)} = {HasBeenRedeemed}), so it can't be thrown again!");
            }

            HasBeenRedeemed = true;
        }

        public virtual bool CanBeRedeemed() {
            return HasBeenRedeemed == false;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        protected bool Equals([CanBeNull] Throwable other) {
            return ID.Equals(other?.ID);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj.GetType() == GetType() && Equals((Throwable)obj);
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}