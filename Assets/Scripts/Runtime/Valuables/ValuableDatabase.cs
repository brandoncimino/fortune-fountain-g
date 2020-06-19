// using System.Collections.Generic;

using System;
using System.Collections.Generic;
using Packages.BrandonUtils.Runtime;
using static Runtime.Valuables.ValuableType;

namespace Runtime.Valuables {
    /// <summary>
    /// Contains the instances of <see cref="ValuableModel"/> for each <see cref="ValuableType"/>.
    ///
    /// Essentially, using <see cref="Models"/> should correspond to using a Java enum.
    /// </summary>
    public static class ValuableDatabase {
        /// <summary>
        /// A convenient accessor for the enumerated <see cref="ValuableType"/> constants, similar to Java's <c>enum.values()</c>.
        /// </summary>
        public static readonly ValuableType[] ValuableTypes = Enum.GetValues(typeof(ValuableType)) as ValuableType[];

        public static readonly Dictionary<ValuableType, ValuableModel> Models =
            new Dictionary<ValuableType, ValuableModel>() {
                [Coin] = new ValuableModel(
                    Coin,
                    1
                ),

                [Ingot] = new ValuableModel(
                    Ingot,
                    10
                ),

                [Gem] = new ValuableModel(
                    Gem,
                    500
                ),

                [Fiduciary] = new ValuableModel(
                    Fiduciary,
                    2500,
                    new Noun("Fiduciary", "Fiduciaries")
                ),

                [Scrip] = new ValuableModel(
                    Scrip,
                    10000
                ),

                [Collectible] = new ValuableModel(
                    Collectible,
                    360000
                ),

                [Livestock] = new ValuableModel(
                    Livestock,
                    4000000
                ),

                [Speculative] = new ValuableModel(
                    Speculative,
                    long.MaxValue
                ),
            };
    }
}