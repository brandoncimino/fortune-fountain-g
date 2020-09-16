using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

// ReSharper disable MemberCanBePrivate.Global

namespace Packages.BrandonUtils.Runtime.Timing {
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
        /// Divides <paramref name="dividend"/> by <paramref name="divisor"/>, returning a new <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="dividend"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static TimeSpan Divide(this TimeSpan dividend, double divisor) {
            return TimeSpan.FromTicks((long) (dividend.Ticks / divisor));
        }


        /// <inheritdoc cref="Divide(System.TimeSpan,double)"/>
        public static TimeSpan Divide(this DateTime dividend, double divisor) {
            return dividend.AsTimeSpan().Divide(divisor);
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

        public static TimeSpan Min(this TimeSpan a, TimeSpan b, params TimeSpan[] c) {
            return c.Append(a).Append(b).Min();
        }

        public static TimeSpan Max(this TimeSpan a, TimeSpan b, params TimeSpan[] c) {
            return c.Append(a).Append(b).Max();
        }

        /// <summary>
        /// Converts <see cref="DateTime"/> <paramref name="dateTime"/> into a <see cref="TimeSpan"/> representing the elapsed time since <see cref="DateTime.MinValue"/>.
        /// </summary>
        /// <remarks>
        /// A bunch of people on the stackoverflow that shows up as the first search result, <a href="https://stackoverflow.com/questions/17959440/convert-datetime-to-timespan">Convert DateTime to TimeSpan</a>, suggest using <see cref="DateTime.TimeOfDay"/> - which is an absolutely bafflingly incorrect answer because <see cref="DateTime.TimeOfDay"/> gives you the time elapsed <b><i>today</i></b>, discarding almost all of the information in the <see cref="DateTime"/>...
        /// <p/>Sidenote - "stackoverflow" and "stackexchange" might be different websites...?
        /// </remarks>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static TimeSpan AsTimeSpan(this DateTime dateTime) {
            return TimeSpan.FromTicks(dateTime.Ticks);
        }

        /// <summary>
        /// Converts <see cref="TimeSpan"/> <paramref name="timeSpan"/> into a <see cref="DateTime"/> representing the date if <paramref name="timeSpan"/> had elapsed since <see cref="DateTime.MinValue"/>.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static DateTime AsDateTime(this TimeSpan timeSpan) {
            return new DateTime(timeSpan.Ticks);
        }

        /// <summary>
        /// Equivalent to calling <see cref="Enumerable.Sum(System.Collections.Generic.IEnumerable{decimal})"/> against a <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// As of 8/26/2020, despite methods like <see cref="Enumerable.Min(System.Collections.Generic.IEnumerable{decimal})"/> having genericized versions (that I can't seem to create a direct link doc comment link to), <a href="https://stackoverflow.com/questions/4703046/sum-of-timespans-in-c-sharp">.Sum() does not</a>.
        /// </remarks>
        /// <param name="timeSpans"></param>
        /// <returns></returns>
        public static TimeSpan Sum(this IEnumerable<TimeSpan> timeSpans) {
            return new TimeSpan(timeSpans.Sum(it => it.Ticks));
        }

        /// <summary>
        /// Attempts to convert <paramref name="value"/> to a <see cref="TimeSpan"/>, either by:
        /// <li>Directly casting <paramref name="value"/>, i.e. <c>(TimeSpan)value</c></li>
        /// <li>Casting <paramref name="value"/> to a number type (int, long, etc.; casting that to a <c>long</c> if necessary) and passing it to <see cref="TimeSpan.FromTicks"/></li>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TimeSpan? TimeSpanFromObject(object value) {
            switch (value) {
                case TimeSpan timeSpan:
                    return timeSpan;
                case int i:
                    return TimeSpan.FromTicks(i);
                case long l:
                    return TimeSpan.FromTicks(l);
                case float f:
                    return TimeSpan.FromTicks((long) f);
                case double d:
                    return TimeSpan.FromTicks((long) d);
                case decimal d:
                    return TimeSpan.FromTicks((long) d);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Wraps a <see cref="List{T}"/> of <see cref="TimeSpan.Ticks"/> (as <see cref="Times"/>) and provides some convenient Linq methods and a flexible <see cref="CompareTo"/>.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        public class ExecutionTime : IComparable<ExecutionTime> {
            public readonly List<long> Times = new List<long>();

            public long MinTicks => Times.Min();

            [JsonProperty]
            public TimeSpan Min => TimeSpan.FromTicks(MinTicks);

            public long MaxTicks => Times.Max();

            [JsonProperty]
            public TimeSpan Max => TimeSpan.FromTicks(MaxTicks);

            public double AverageTicks => Times.Average();

            [JsonProperty]
            public TimeSpan Average => TimeSpan.FromTicks((long) AverageTicks);

            public long TotalTicks => Times.Sum();

            [JsonProperty]
            public TimeSpan Total => TimeSpan.FromTicks(TotalTicks);

            /// <summary>
            /// Compares <b><i>this</i></b> to <paramref name="other"/> using <see cref="CompareTo"/> calls against multiple properties:
            /// <li><see cref="Min"/></li>
            /// <li><see cref="Max"/></li>
            /// <li><see cref="Average"/></li>
            /// Returns -1 or 1 if <b>any</b> properties return that value and <b>none</b> return the other;
            /// otherwise, returns 0.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(ExecutionTime other) {
                var minCompare = Min.CompareTo(other.Min);
                var maxCompare = Max.CompareTo(other.Max);
                var avgCompare = Average.CompareTo(other.Average);

                var compares = new int[] {minCompare, maxCompare, avgCompare};
                if (compares.Any(it => it > 0) && compares.All(it => it >= 0)) {
                    return 1;
                }
                else if (compares.Any(it => it < 0) && compares.All(it => it <= 0)) {
                    return -1;
                }
                else {
                    return 0;
                }
            }

            public override string ToString() {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
        }

        public static ExecutionTime AverageExecutionTime(Action action, int iterations = 1) {
            var lapTimes = new ExecutionTime();

            for (int i = 0; i < iterations; i++) {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                lapTimes.Times.Add(stopwatch.ElapsedTicks);
            }

            return lapTimes;
        }
    }
}