using System;

using NUnit.Framework.Constraints;

namespace Packages.BrandonUtils.Runtime.Testing {
    public class ApproximationConstraint : RangeConstraint {
        private readonly object ExpectedValue;
        private readonly object Threshold;
        private readonly object MinValue;
        private readonly object MaxValue;
        private const    string NUnitDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        public override string Description =>
            $"~ {FormatObject(ExpectedValue)}" +
            $"\n\t{nameof(Threshold)} = {FormatObject(Threshold)}" +
            $"\n\t{nameof(MinValue)}  = {FormatObject(MinValue)}" +
            $"\n\t{nameof(MaxValue)}  = {FormatObject(MaxValue)}";

        public ApproximationConstraint(
            object expectedValue,
            object threshold
        ) : base(
            (IComparable) CoercionUtils.CoerciveSubtraction(
                expectedValue,
                threshold
            ),
            (IComparable) CoercionUtils.CoerciveAddition(
                expectedValue,
                threshold
            )
        ) {
            ExpectedValue = expectedValue;
            Threshold     = threshold;
            MinValue      = CoercionUtils.CoerciveSubtraction(ExpectedValue, Threshold);
            MaxValue      = CoercionUtils.CoerciveAddition(ExpectedValue, Threshold);
        }

        /// <summary>
        /// Formats <paramref name="obj"/> in a similar style to NUnit's "MsgUtils" (which, unfortunately, is an <see langword="internal"/> class...)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string FormatObject(object obj) {
            switch (obj) {
                case DateTime date:
                    return date.ToString(NUnitDateTimeFormat);
                default:
                    return obj.ToString();
            }
        }
    }
}