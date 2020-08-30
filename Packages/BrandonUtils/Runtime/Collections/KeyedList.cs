using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Packages.BrandonUtils.Runtime.Logging;

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
            set {
                LogUtils.Log($"PUTTING {key} to the keyed list {this} at {DateTime.Now}");
                Put(value);
            }
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
            LogUtils.Log($"ADDING {item} to {this}");
            base.Add(item);
        }

        // protected override void InsertItem(int index, TValue item) {
        //     LogUtils.Log($"INSERTING {item} at index {index}");
        //     var targetKey = GetKeyForItem(item);
        //     if (Contains(targetKey)) {
        //         LogUtils.Log($"ALREADY CONTAINS ITEM {item}");
        //         // var itemWithPrimaryKey = this.First(it => GetKeyForItem(it).Equals(GetKeyForItem(item)));
        //         // this.Remove(targetKey);
        //         var itemWithMatchingKey        = this[targetKey];
        //         var indexOfItemWithMatchingKey = IndexOf(itemWithMatchingKey);
        //         this.SetItem(indexOfItemWithMatchingKey, item);
        //     }
        //     else {
        //         LogUtils.Log($"DOES NOT CONTAIN ITEM {item}");
        //         base.InsertItem(index, item);
        //     }
        // }

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

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext streamingContext) {
            Clear();
        }
    }
}