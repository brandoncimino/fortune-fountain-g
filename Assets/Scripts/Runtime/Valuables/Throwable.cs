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
        /// A flag to prevent a single throwable from ever being thrown more than once.
        /// </summary>
        /// <remarks>
        /// <li>Just because the <see cref="Throwable"/> has been removed from <see cref="Hand.Throwables"/>, it <b>still exists</b></li>
        /// <li>This is due to the intricacies of garbage collection, probably...which I don't know much about</li>
        /// <li>We may run into limitations with this boolean flag</li>
        /// <li>One alternative is to have <see cref="Throwable"/> implement <see cref="IDisposable"/>, and dispose of the <see cref="Throwable"/> when we call <see cref="Throwable.Throw"/></li>
        /// </remarks>
        [JsonIgnore]
        public bool AlreadyThrown;

        public Throwable(ValuableType valuableType, double throwValue) {
            ValuableType = valuableType;
            ThrowValue   = throwValue;
        }

        public void Throw(Hand hand) {
            Throw(hand.SaveData);
        }

        public void Throw(FortuneFountainSaveData saveData) {
            if (AlreadyThrown) {
                throw new FortuneFountainException($"The {nameof(Throwable)} {this} has already been thrown, so it can't be thrown again!");
            }

            AlreadyThrown = true;
            saveData.ThrowSingle(this);
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}