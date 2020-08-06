using System;

namespace Packages.BrandonUtils.Runtime {
    public static class Time {
        /// <summary>
        /// Mimics .NET Core's <a href="https://docs.microsoft.com/en-us/dotnet/api/system.timespan.divide">TimeSpan.Divide</a>.
        ///
        /// Does this by converting the given <see cref="TimeSpan"/>s into ticks and performing the division on those.
        /// </summary>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend"/> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static double Divide(this TimeSpan dividend, TimeSpan divisor) {
            //cast of dividend.Ticks to (double) is to prevent loss of fractions
            return (double) dividend.Ticks / divisor.Ticks;
        }

        /// <summary>
        /// Divides <paramref name="dividend"/> by <paramref name="divisor"/> via <see cref="Divide"/>, returning the remainder.
        /// </summary>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend"/> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static double Modulus(this TimeSpan dividend, TimeSpan divisor) {
            return Divide(dividend, divisor) % 1;
        }

        /// <summary>
        /// Divides <paramref name="dividend"/> by <paramref name="divisor"/>, returning the integer quotient.
        /// </summary>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend"/> will be divided (i.e. the bottom of the fraction)</param>
        /// <returns></returns>
        public static double Quotient(this TimeSpan dividend, TimeSpan divisor) {
            return Math.Floor(Divide(dividend, divisor));
        }

        /// <summary>
        /// Performs a modulus operation similar to <see cref="Modulus"/>, but returns the result as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// Should be equivalent to multiplying <paramref name="divisor"/> by the result of <see cref="Modulus"/>, but is simpler in its actual execution.
        /// </remarks>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend"/> will be divided (i.e. the bottom of the fraction)</param>
        /// <seealso cref="QuotientSpan"/>
        /// <returns></returns>
        public static TimeSpan ModSpan(this TimeSpan dividend, TimeSpan divisor) {
            return TimeSpan.FromTicks(dividend.Ticks % divisor.Ticks);
        }

        /// <summary>
        /// Performs division similar to <see cref="Quotient"/>, but returns the result as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// Should be equivalent to multiplying <paramref name="dividend"/> by the result of <see cref="Quotient"/>, but is simpler in its actual execution.
        /// </remarks>
        /// <param name="dividend">The number to be divided (i.e. top of the fraction)</param>
        /// <param name="divisor">The number by which <paramref name="dividend"/> will be divided (i.e. the bottom of the fraction)</param>
        /// <seealso cref="ModSpan"/>
        /// <returns></returns>
        public static TimeSpan QuotientSpan(this TimeSpan dividend, TimeSpan divisor) {
            return TimeSpan.FromTicks(dividend.Ticks / divisor.Ticks);
        }
    }
}