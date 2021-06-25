using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Runtime.Saving;

namespace Runtime.Valuables {
    [Serializable]
    public class Throwable {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ValuableType ValuableType;

        [JsonProperty]
        public double ThrowValue;

        /// <summary>
        /// The event raised by <b>each <see cref="Throwable"/></b> when it is <see cref="Throw"/>n.
        /// </summary>
        public static event Action<Throwable> ThrowSingleEvent;

        /// <summary>
        /// A flag to prevent a single throwable from ever being thrown more than once.
        /// </summary>
        /// <remarks>
        /// <li><see cref="HandleThrowHandEvent"/> is a global event</li>
        /// <li>When <see cref="Hand.Throw"/> is triggered, <see cref="Throwable"/> instances are removed from <see cref="Hand.Throwables"/></li>
        /// <li>Just because the <see cref="Throwable"/> has been removed from <see cref="Hand.Throwables"/>, it <b>still exists</b></li>
        /// <li>This is due to the intricacies of garbage collection, probably...which I don't know much about</li>
        /// <li>We may run into limitations with this boolean flag</li>
        /// <li>One alternative is to have <see cref="Throwable"/> implement <see cref="IDisposable"/>, and dispose of the <see cref="Throwable"/> when we call <see cref="Throwable.Throw"/></li>
        /// </remarks>
        [JsonIgnore]
        private bool _alreadyThrown;

        public Throwable(ValuableType valuableType, double throwValue) {
            ValuableType = valuableType;
            ThrowValue   = throwValue;

            //subscribing to the ThrowHandEvent
            Hand.ThrowHandEvent += HandleThrowHandEvent;
        }

        private void HandleThrowHandEvent(Hand hand) {
            Throw();
        }

        public void Throw() {
            if (_alreadyThrown) {
                throw new FortuneFountainException($"The {nameof(Throwable)} {this} has already been thrown, so it can't be thrown again!");
            }

            _alreadyThrown = true;
            // LogUtils.Log($"{this} is being thrown at {DateTime.Now:HH:mm:ss.fff}");
            ThrowSingleEvent?.Invoke(this);
        }

        public override string ToString() {
            // return $"{{{ValuableType}, {ThrowValue}}}";
            return JsonConvert.SerializeObject(this);
        }
    }
}