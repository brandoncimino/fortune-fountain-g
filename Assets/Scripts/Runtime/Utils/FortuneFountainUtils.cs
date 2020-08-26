using System.Collections.Generic;
using System.Linq;
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
        /// <remarks>
        /// For some reason, if we use LINQ for this (i.e. <see cref="Enumerable.Select{TSource,TResult}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,int,TResult})"/>, then the events don't get properly triggered. I have no idea why.
        ///
        /// EDIT: It looks like we have to "close" the LINQ expression using <see cref="Enumerable.ToList{TSource}"/>.
        /// </remarks>
        /// <param name="playerValuables"></param>
        public static List<int> CheckGenerate<T>(this IDictionary<T, PlayerValuable> playerValuables) {
            return playerValuables.Values.Select(it => it.CheckGenerate()).ToList();
        }

        /// <summary>
        /// Calls <see cref="PlayerValuable.CheckGenerate"/> against all items in <paramref name="playerValuables"/>.
        /// </summary>
        /// <seealso cref="CheckGenerate{T}"/>
        /// <param name="playerValuables"></param>
        public static List<int> CheckGenerate(this IEnumerable<PlayerValuable> playerValuables) {
            return playerValuables.Select(it => it.CheckGenerate()).ToList();
        }
    }
}