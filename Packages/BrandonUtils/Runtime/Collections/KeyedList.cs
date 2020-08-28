using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Packages.BrandonUtils.Runtime.Collections {
    /// <summary>
    /// A simple implementation of <see cref="KeyedCollection{TKey,TItem}"/>.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [Serializable]
    public class KeyedList<TKey, TValue> : KeyedCollection<TKey, TValue> where TValue : IPrimaryKeyed<TKey> {
        protected override TKey GetKeyForItem(TValue item) {
            return item.PrimaryKey;
        }

        public new TValue this[TKey key] {
            get => base[key];
            set {
                this.Remove(key);
                this.Add(value);
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
    }
}