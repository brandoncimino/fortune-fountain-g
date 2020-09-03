using System;
using NUnit.Framework.Constraints;

namespace Packages.BrandonUtils.Runtime.Testing {
    public class ApproximationConstraint : RangeConstraint {
        private readonly object ExpectedValue;
        private readonly object Threshold;
        private          object MinValue => GetMinValue(ExpectedValue, Threshold);
        private          object MaxValue => GetMaxValue(ExpectedValue, Threshold);

        public override string Description => $"approximately equal to {ExpectedValue}\n\t{nameof(Threshold)} = {Threshold}\n\t{nameof(MinValue)} = {MinValue}\n\t{nameof(MaxValue)} = {MaxValue}";

        public ApproximationConstraint(object expectedValue, object threshold) : base((IComparable) GetMinValue(expectedValue, threshold), (IComparable) GetMaxValue(expectedValue, threshold)) {
            ExpectedValue = expectedValue;
            Threshold     = threshold;
        }

        private static T GetMinValue<T>(T expectedValue, T threshold) {
            return (dynamic) expectedValue - (dynamic) threshold;
        }

        private static T GetMaxValue<T>(T expectedValue, T threshold) {
            return (dynamic) expectedValue + (dynamic) threshold;
        }
    }
}