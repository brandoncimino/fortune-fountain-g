using System;
using Packages.BrandonUtils.Runtime;

namespace Runtime.Utils {
    /// <summary>
    /// Contains utility methods that manage general "Incremental Game" functionality,
    /// such as calculating the number of "grabs" that have happened in a given time period.
    ///
    /// Because this is a "Utility" class, methods should be <c>static</c> whenever possible.
    /// </summary>
    public class IncrementalUtils {
        /// <summary>
        /// Calculates the number of times that <paramref name="duration"/> has <b>completely</b> occurred within the span of <paramref name="startTime"/> to <paramref name="endTime"/>.
        /// <br/>
        /// Stores the time of the <b>last full completion</b> of <paramref name="duration"/> in <paramref name="lastCompletionTime"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="lastCompletionTime"/> can be used as the <paramref name="startTime"/> in future calls to <see cref="NumberOfTimesCompleted"/>.
        /// <br/>
        /// Returns a <c>double</c> (as opposed to a <c>long</c> or <c>int</c>) to mimic methods such as <see cref="Math.Floor(double)"/>, which return <c>double</c> values despite the results expected to be whole numbers.
        /// </remarks>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="duration"></param>
        /// <param name="lastCompletionTime"></param>
        /// <returns></returns>
        public static double NumberOfTimesCompleted(DateTime startTime, DateTime endTime, TimeSpan duration, out DateTime lastCompletionTime) {
            var deltaTime = endTime - startTime;
            lastCompletionTime = endTime - deltaTime.QuotientSpan(duration);
            return deltaTime.Quotient(duration);
        }
    }
}