using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace Packages.BrandonUtils.Runtime {
    /// <summary>
    /// Contains utility methods that manipulate or extend <see cref="DateTime" />, <see cref="TimeSpan" />, etc.
    ///
    /// TODO: Allow for the syntax <c>TimeSpan * 2</c>. I think that would require a namespace-specific extension of <see cref="TimeSpan"/> that is also called <see cref="TimeSpan"/>...
    /// </summary>
    public static class TimeUtils {
        /// <summary>
        ///     Corresponds to <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.datavisualization.charting.datetimeintervaltype?view=netframework-4.8">DateTimeIntervalType</a> and <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.dateinterval?view=netcore-3.1">DateInterval</a>, but I couldn't find a corresponding enum that was available inside of Unity.
        /// </summary>
        /// <remarks>
        ///     <li>Specifically for use in <see cref="TimeUtils.NormalizePrecision" />.</li>
        ///     <li>These should have parity with the <see cref="TimeSpan" /> methods like <see cref="TimeSpan.FromDays" />.</li>
        /// </remarks>
        public enum IntervalType {
            Milliseconds,
            Seconds,
            Minutes,
            Hours,
            Days
        }

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
            return (double) dividend.Ticks / divisor.Ticks;
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
            if (divisor == TimeSpan.Zero) {
                throw new DivideByZeroException("Cannot divide by a zero TimeSpan!");
            }
        }

        /// <summary>
        ///     Multiplies <paramref name="timeSpan" /> by <paramref name="factor" />, returning a new <see cref="TimeSpan" />.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static TimeSpan Multiply(this TimeSpan timeSpan, double factor) {
            return TimeSpan.FromTicks((long) (timeSpan.Ticks * factor));
        }

        /// <inheritdoc cref="NormalizePrecision" />
        public static double NormalizeMinutes(double minutes) {
            return TimeSpan.FromMinutes(minutes).TotalMinutes;
        }

        /// <inheritdoc cref="NormalizePrecision" />
        public static double NormalizeSeconds(double seconds) {
            return TimeSpan.FromSeconds(seconds).TotalSeconds;
        }

        /// <inheritdoc cref="NormalizePrecision" />
        public static double NormalizeHours(double hours) {
            return TimeSpan.FromHours(hours).TotalHours;
        }

        /// <inheritdoc cref="NormalizePrecision" />
        public static double NormalizeDays(double days) {
            return TimeSpan.FromDays(days).TotalDays;
        }

        /// <inheritdoc cref="NormalizePrecision" />
        public static double NormalizeMilliseconds(double milliseconds) {
            return TimeSpan.FromMilliseconds(milliseconds).TotalMilliseconds;
        }

        /// <summary>
        ///     Reduces the given <paramref name="value" /> so that it matches the appropriate precision for the given <paramref name="unit" /> component of a <see cref="TimeSpan" />.
        /// </summary>
        /// <remarks>
        ///     <li>Converts <paramref name="value" /> into a <see cref="TimeSpan" /> via the given <paramref name="unit" />, then returns the total <paramref name="unit" />s of the new <see cref="TimeSpan" />.</li>
        ///     <li>
        ///         Joins together the multiple "Normalize" methods, e.g. <see cref="NormalizeMinutes" />, into one method, via <see cref="IntervalType" />.
        ///         <ul>
        ///             <li>
        ///                 The individual methods such as <see cref="NormalizeDays" /> are maintained for parity with <see cref="TimeSpan" /> methods such as <see cref="TimeSpan.FromDays" />.
        ///             </li>
        ///         </ul>
        ///     </li>
        /// </remarks>
        /// <example>
        ///     TODO: Add an example, because this is kinda hard to explain without one.
        /// </example>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static double NormalizePrecision(double value, IntervalType unit) {
            switch (unit) {
                case IntervalType.Milliseconds:
                    return NormalizeMilliseconds(value);
                case IntervalType.Seconds:
                    return NormalizeSeconds(value);
                case IntervalType.Minutes:
                    return NormalizeMinutes(value);
                case IntervalType.Hours:
                    return NormalizeHours(value);
                case IntervalType.Days:
                    return NormalizeDays(value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, $"I don't know how to make a {nameof(TimeSpan)} out of {unit}s!");
            }
        }

        /// <summary>
        /// Corresponds to <see cref="Math.Min(int, int)"/>, etc.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static DateTime Min(this DateTime a, DateTime b, params DateTime[] c) {
            return c.Append(a).Append(b).Min();
        }

        /// <summary>
        /// Corresponds to <see cref="Math.Max(byte,byte)"/>, etc.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static DateTime Max(this DateTime a, DateTime b, params DateTime[] c) {
            return c.Append(a).Append(b).Max();
        }
    }
}