using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Packages.BrandonUtils.Runtime.Collections {
    /// <summary>
    /// A simple implementation of <see cref="KeyedCollection{TKey,TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class KeyedList<TKey, TValue> : KeyedCollection<TKey, TValue> where TValue : class, IPrimaryKeyed<TKey> {
        protected override TKey GetKeyForItem(TValue item) {
            return item.PrimaryKey;
        }

        public new TValue this[TKey key] {
            get => base[key];
            set => Put(value);
        }

        public KeyedList(IEnumerable<TValue> collection) {
            foreach (TValue item in collection) {
                this.Add(item);
            }
        }

        public KeyedList() : base() { }

        public KeyedList<TKey, TValue> Copy() {
            return new KeyedList<TKey, TValue>(this);
        }

        public void ForEach(Action<TValue> action) {
            this.ToList().ForEach(action);
        }

        public new void Add(TValue item) {
            base.Add(item);
        }

        /// <summary>
        /// If the <see cref="GetKeyForItem">primary key</see> of <paramref name="item"/> already exists, update it.
        /// <br/>
        /// If not, <see cref="Add"/> it.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>The old item with the primary key matching <paramref name="item"/> if it existed; otherwise, <see langword="null"/>.</returns>
        public TValue Put(TValue item) {
            TValue oldItem = null;
            if (Contains(GetKeyForItem(item))) {
                oldItem = this[GetKeyForItem(item)];
            }

            Remove(GetKeyForItem(item));
            Add(item);

            return oldItem;
        }

        /// <summary>
        /// Method called with the object is de-serialized, e.g. via <see cref="JsonConvert.DeserializeObject{T}(string)"/>.
        /// </summary>
        /// <param name="streamingContext"></param>
        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext streamingContext) {
            Clear();
        }

        /// <summary>
        /// A parameterless override for <see cref="Enumerable.ToDictionary{TSource,TKey}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TKey})"/> that uses <see cref="GetKeyForItem"/> as the selector.
        /// </summary>
        /// <returns>A <i>new</i> <see cref="Dictionary{TKey,TValue}"/> where:
        /// <li>The <see cref="Dictionary{TKey,TValue}.Values"/> are the <see cref="KeyedList{TKey,TValue}"/>'s items</li>
        /// <li>The <see cref="Dictionary{TKey,TValue}.Keys"/> are the <see cref="KeyedList{TKey,TValue}"/>'s <see cref="GetKeyForItem"/>s.</li>
        /// </returns>
        /// <seealso cref="Enumerable.ToDictionary{TSource,TKey}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,TKey})"/>
        public Dictionary<TKey, TValue> ToDictionary() {
            return this.ToDictionary(GetKeyForItem);
        }
    }
}