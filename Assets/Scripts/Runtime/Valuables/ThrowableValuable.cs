using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Runtime.Valuables {
    public class ThrowableValuable : Throwable {
        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ValuableType ValuableType { get; }

        /// <summary>
        /// The amount of karma that this <see cref="ThrowableValuable"/> would currently fetch at market.
        /// </summary>
        /// <remarks>
        /// Also known as the "Grand Exchange" price.
        /// </remarks>
        [JsonProperty]
        public double PresentValue { get; }

        public ThrowableValuable(
            ValuableType valuableType,
            [JsonProperty(nameof(PresentValue))] double? karmaValue = default
        ) {
            ValuableType = valuableType;
            PresentValue = karmaValue.GetValueOrDefault(valuableType.FaceValue());
        }

        public override void Redeem() {
            base.Redeem();

            MyHand.MySaveData.AddKarma(PresentValue);
        }
    }
}