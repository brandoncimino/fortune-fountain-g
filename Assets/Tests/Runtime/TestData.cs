using System.Collections.Generic;

using BrandonUtils.Standalone.Collections;

using Runtime.Saving;
using Runtime.Valuables;

namespace Tests.Runtime {
    /// <summary>
    /// Contains convenient things like "ugly" save files, etc.
    /// </summary>
    public static class TestData {
        /// <summary>
        /// Returns a <see cref="Dictionary{TKey,TValue}"/> for use as <see cref="FortuneFountainSaveData.PlayerValuables"/> that:
        /// <li>Contains every <see cref="ValuableType"/></li>
        /// <li>Has each <see cref="PlayerValuable.Rate"/> set to <paramref name="rate"/> (defaulting to 1)</li>
        /// </summary>
        /// <param name="saveData"></param>
        /// <param name="rate">The <see cref="PlayerValuable.Rate"/> that each <see cref="PlayerValuable"/> will have (defaults to 1)</param>
        /// <returns></returns>
        public static KeyedList<ValuableType, PlayerValuable> GetUniformPlayerValuables(FortuneFountainSaveData saveData, double rate = 1) {
            var keyedList = new KeyedList<ValuableType, PlayerValuable>();

            foreach (var valuableType in ValuableDatabase.ValuableTypes) {
                keyedList.Add(new PlayerValuable(saveData, valuableType));
                keyedList[valuableType].Rate = rate;
            }

            return keyedList;
        }
    }
}