using System;
using System.Diagnostics.Contracts;
using Packages.BrandonUtils.Runtime;
using Packages.BrandonUtils.Runtime.Logging;

namespace Runtime.Utils {
    /// <summary>
    ///     Contains utility methods that manage general "Incremental Game" functionality,
    ///     such as calculating the number of "grabs" that have happened in a given time period.
    ///     Because this is a "Utility" class, methods should be <c>static</c> whenever possible.
    /// </summary>
    public static class IncrementalUtils {
        /// <summary>
        ///     Calculates the number of times that <paramref name="durationToComplete" /> has <b>completely</b> occurred within the span of <paramref name="startTime" /> to <paramref name="endTime" />.
        ///     <br />
        ///     Stores the time of the <b>last full completion</b> of <paramref name="durationToComplete" /> in <paramref name="lastCompletionTime" />.
        /// </summary>
        /// <remarks>
        ///     <li><paramref name="lastCompletionTime" /> can be used as the <paramref name="startTime" /> in future calls to <see cref="NumberOfTimesCompleted" />.</li>
        ///     <li>Returns a <c>double</c> (as opposed to a <c>long</c> or <c>int</c>) to mimic methods such as <see cref="Math.Floor(double)" />, which return <c>double</c> values despite the results expected to be whole numbers.</li>
        ///     <li>
        ///         As of 8/10/2020, this method's <see cref="Contract" /> requires that "good" data be passed to it:
        ///         <ul>
        ///             <li><paramref name="endTime" /> > <paramref name="startTime" /></li>
        ///             <li><paramref name="durationToComplete" /> > <see cref="TimeSpan.Zero" /></li>
        ///         </ul>
        ///         While there are valid ways to handle both of these situations, and it is clear what the expected <c>return</c> value would be for each, the expect <paramref name="lastCompletionTime" /> is more ambiguous, and depends on the specific scenario in which <see cref="NumberOfTimesCompleted" /> is being used.
        ///         As a result, I've decided to make this method "strict" for now, and will potentially revisit how these scenarios are handled when I encounter them.
        ///     </li>
        /// </remarks>
        /// <param name="startTime">The beginning of the time period. Must be less than <paramref name="endTime" />.</param>
        /// <param name="endTime">The end of the time period. Must be greater than <paramref name="startTime" /></param>
        /// <param name="durationToComplete">The duration of one "completion". Must be greater than <see cref="TimeSpan.Zero" />.</param>
        /// <param name="lastCompletionTime">The time that the previous "completion" finished.</param>
        /// <returns></returns>
        public static double NumberOfTimesCompleted(DateTime startTime, DateTime endTime, TimeSpan durationToComplete, out DateTime lastCompletionTime) {
            Contract.Assert(endTime > startTime, $"The given {nameof(endTime)} ({endTime}) was before the given {nameof(startTime)} ({startTime})!");
            Contract.Assert(durationToComplete > TimeSpan.Zero, $"The given {nameof(durationToComplete)} ({durationToComplete}) must be greater than 0!");

            var deltaTime = endTime - startTime;
            lastCompletionTime = endTime - deltaTime.QuotientSpan(durationToComplete);
            var numberOfTimesCompleted = deltaTime.Quotient(durationToComplete);

            LogUtils.Log($"{nameof(deltaTime)} = {deltaTime}");
            LogUtils.Log($"{nameof(durationToComplete)} = {durationToComplete}");
            LogUtils.Log($"{nameof(numberOfTimesCompleted)} = {numberOfTimesCompleted}");

            return numberOfTimesCompleted;
        }
    }
}