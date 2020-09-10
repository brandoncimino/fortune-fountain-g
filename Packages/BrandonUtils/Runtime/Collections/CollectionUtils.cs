﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Packages.BrandonUtils.Runtime.Collections {
    /// <summary>
    ///     Contains utility and extension methods for collections, such as <see cref="IList{T}" /> and <see cref="IDictionary{TKey,TValue}" />.
    /// </summary>
    public static class CollectionUtils {
        public static T Random<T>(this IEnumerable<T> enumerable) {
            var array = enumerable as T[] ?? enumerable.ToArray();
            return array.ElementAt(new System.Random().Next(0, array.Length));
        }

        public static T GrabRandom<T>(this ICollection<T> list) {
            var random = list.Random();
            list.Remove(random);
            return random;
        }

        public static ICollection<T> Randomize<T>(this ICollection<T> oldList) {
            var backupList = oldList.Copy();
            oldList.Clear();

            while (backupList.Any()) {
                oldList.Add(GrabRandom(backupList));
            }

            return oldList;
        }

        public static ICollection<T> RandomCopy<T>(this ICollection<T> oldList) {
            return oldList.Copy().Randomize();
        }

        public static IList<T> Copy<T>(this IList<T> oldCollection) {
            return oldCollection.Select(it => it).ToList();
        }

        public static ICollection<T> Copy<T>(this ICollection<T> oldCollection) {
            return (ICollection<T>) oldCollection.Select(it => it);
        }

        public static IEnumerable<T> Copy<T>(this IEnumerable<T> oldList) {
            return oldList.Select(it => it).ToList();
        }

        public static string Pretty<T>(this IEnumerable<T> toPrint, string separator = "\n") {
            return string.Join(separator, toPrint.Select(it => $"[{it}]").ToList());
        }

        /// <summary>
        ///     Similarly to <see cref="List{T}.ForEach" />, this performs <paramref name="action" /> against each
        ///     <b>
        ///         <see cref="KeyValuePair{TKey,TValue}" />
        ///     </b>
        ///     in <paramref name="dictionary" />.
        /// </summary>
        /// <example>
        ///     Which variation of <see cref="ForEach{T,T2}(System.Collections.Generic.Dictionary{T,T2},System.Action{System.Collections.Generic.KeyValuePair{T,T2}})">ForEach</see> is called depends on the first time the <paramref name="action" />'s <see cref="Delegate.Target" /> parameter is accessed.
        /// <p />For example, given:
        /// <code><![CDATA[
        /// Dictionary dictionary = new Dictionary<string, string>();
        /// ]]></code>
        /// Then the following would treat <b><i><c>it</c></i></b> as a <see cref="KeyValuePair{TKey,TValue}" />:
        /// <code><![CDATA[
        /// dictionary.ForEach(it => Console.WriteLine(it.Key);
        /// ]]></code>
        /// While the following would treat <b><i><c>it</c></i></b> as a <see cref="string" />:
        /// <code><![CDATA[
        /// dictionary.ForEach(it => Console.WriteLine(it.Length));
        /// ]]></code>
        /// While the following would <b>fail to compile</b>, due to attempting to treating <c>it</c> as both a <see cref="KeyValuePair{TKey,TValue}" /> and a <see cref="string" />:
        /// <code><![CDATA[
        /// dictionary.ForEach(it => Console.WriteLine($"{it.Key}, {it.Length}"));
        /// ]]></code>
        /// And the following would <b>fail to compile</b>, due to being ambiguous:
        /// <code><![CDATA[
        /// dictionary.ForEach(it => Console.WriteLine(it));
        /// ]]></code>
        /// </example>
        /// <param name="dictionary">The <see cref="Dictionary{TKey,TValue}" /> you would like to iterate over.</param>
        /// <param name="action">The <see cref="Action" /> that will be performed against each <see cref="KeyValuePair{TKey,TValue}" /> in <paramref name="dictionary" />.</param>
        /// <typeparam name="T">The type of <paramref name="dictionary" />'s <see cref="Dictionary{TKey,TValue}.Keys" /></typeparam>
        /// <typeparam name="T2">The type of <paramref name="dictionary" />'s <see cref="Dictionary{TKey,TValue}.Values" /></typeparam>
        /// <seealso cref="ForEach{T,T2}(System.Collections.Generic.IDictionary{T,T2},System.Action{T2})" />
        /// <seealso cref="List{T}.ForEach" />
        public static void ForEach<T, T2>(this IDictionary<T, T2> dictionary, Action<KeyValuePair<T, T2>> action) {
            foreach (var pair in dictionary) {
                action.Invoke(pair);
            }
        }

        /// <inheritdoc cref="ForEach{T,T2}(System.Collections.Generic.Dictionary{T,T2},System.Action{System.Collections.Generic.KeyValuePair{T,T2}})" />
        /// <summary>
        ///     Similarly to <see cref="List{T}.ForEach" />, this performs <paramref name="action" /> against each
        ///     <b>
        ///         <see cref="KeyValuePair{TKey,TValue}.Value" />
        ///     </b>
        ///     in <paramref name="dictionary" />.
        /// </summary>
        public static void ForEach<T, T2>(this IDictionary<T, T2> dictionary, Action<T2> action) {
            dictionary.Values.ToList().ForEach(action);
        }

        /// <summary>
        /// Calls <see cref="string.Join(string,System.Collections.Generic.IEnumerable{string})"/> against <paramref name="enumerable"/>.
        /// </summary>
        /// <remarks>Corresponds roughly to Java's <a href="https://docs.oracle.com/javase/8/docs/api/java/util/stream/Collectors.html#joining--">.joining()</a> collector.
        /// </remarks>
        /// <param name="enumerable"></param>
        /// <param name="separator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string JoinString<T>(this IEnumerable<T> enumerable, string separator = "") {
            return string.Join(separator, enumerable);
        }
    }
}