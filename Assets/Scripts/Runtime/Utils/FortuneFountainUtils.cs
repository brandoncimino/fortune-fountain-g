using System;
using System.Collections.Generic;
using Runtime.Valuables;

namespace Runtime.Utils {
    /// <summary>
    /// Contains extension methods and things specific to FortuneFountain stuff like <see cref="PlayerValuable"/>.
    /// </summary>
    public static class FortuneFountainUtils {
        /// <summary>
        /// Calls <see cref="PlayerValuable.CheckGenerate()"/> against all <see cref="IDictionary{TKey,TValue}.Values"/> in <paramref name="playerValuables"/>.
        ///
        /// TODO: Can this be placed somewhere closer to <see cref="PlayerValuable"/>?
        /// </summary>
        /// <param name="playerValuables"></param>
        /// <param name="endTime"></param>
        public static void CheckGenerate<T>(this IDictionary<T, PlayerValuable> playerValuables, DateTime? endTime = null) {
            foreach (var pair in playerValuables) {
                pair.Value.CheckGenerate(endTime);
            }
        }

        /// <summary>
        /// Calls <see cref="PlayerValuable.CheckGenerate"/> against all items in <paramref name="playerValuables"/>.
        /// </summary>
        /// <seealso cref="CheckGenerate{T}"/>
        /// <param name="playerValuables"></param>
        /// <param name="endTime"></param>
        public static void CheckGenerate(this IEnumerable<PlayerValuable> playerValuables, DateTime? endTime = null) {
            foreach (var playerValuable in playerValuables) {
                playerValuable.CheckGenerate(endTime);
            }
        }
    }
}