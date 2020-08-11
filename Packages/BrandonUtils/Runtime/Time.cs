using System;

namespace Packages.BrandonUtils.Runtime {
    public static class Time {
        /// <summary>
        ///     Mimics .NET Core's <a href="https://docs.microsoft.com/en-us/dotnet/api/system.timespan.divide">TimeSpan.Divide</a>.
        ///     Does this by converting the given <see cref="TimeSpan" />s into <see cref="TimeSpan.TotalSeconds" /> and performing the division on those.
        /// </summary>
        /// <remarks>
        ///     This originally performed the division on <see cref="TimeSpan.Ticks" /> rather than <see cref="TimeSpan.TotalSeconds" />, but this was actually slightly inaccurate due to the number of ticks being so large.
        /// </remarks>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend" /> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static double Divide(this TimeSpan dividend, TimeSpan divisor) {
            ValidateDivisor(divisor);
            return dividend.TotalSeconds / divisor.TotalSeconds;
        }

        /// <summary>
        ///     Divides <paramref name="dividend" /> by <paramref name="divisor" />, returning the integer quotient.
        /// </summary>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend" /> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static double Quotient(this TimeSpan dividend, TimeSpan divisor) {
            ValidateDivisor(divisor);
            return Math.Floor(Divide(dividend, divisor));
        }

        /// <summary>
        ///     Returns the <see cref="TimeSpan" /> remainder after <paramref name="dividend" /> is divided by <paramref name="divisor" />.
        /// </summary>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend" /> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static TimeSpan Modulus(this TimeSpan dividend, TimeSpan divisor) {
            ValidateDivisor(divisor);
            return TimeSpan.FromTicks(dividend.Ticks % divisor.Ticks);
        }

        private static void ValidateDivisor(TimeSpan divisor) {
            if (divisor == TimeSpan.Zero) throw new DivideByZeroException("Cannot divide by a zero TimeSpan!");
        }
    }
}