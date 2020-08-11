using System.Collections.Generic;

namespace Runtime.Valuables {
    /// <summary>
    ///     Contains extension methods for <see cref="PlayerValuable" />, etc.
    /// </summary>
    public static class ValuableUtils {
        /// <summary>
        ///     A convenience method to call <see cref="PlayerValuable.CheckGenerate" /> against a collection of <see cref="PlayerValuable" />s.
        ///     TODO: Can this be included in the <see cref="PlayerValuable" /> class somehow?
        /// </summary>
        /// <param name="playerValuables">An <see cref="IEnumerable{T}" /> of <see cref="PlayerValuable" />s.</param>
        public static void CheckGenerate(this IEnumerable<PlayerValuable> playerValuables) {
            foreach (var playerValuable in playerValuables) playerValuable.CheckGenerate();
        }

        /// <summary>
        ///     <inheritdoc cref="CheckGenerate" />
        /// </summary>
        /// <param name="playerValuables">An <see cref="IDictionary{TKey,TValue}" /> where the <see cref="IDictionary{TKey,TValue}.Values" /> are <see cref="PlayerValuable" />s.</param>
        /// <typeparam name="T"></typeparam>
        public static void CheckGenerate<T>(this IDictionary<T, PlayerValuable> playerValuables) {
            playerValuables.Values.CheckGenerate();
        }
    }
}