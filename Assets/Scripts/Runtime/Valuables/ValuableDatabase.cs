// using System.Collections.Generic;

using System;
using System.Collections.Generic;
using Packages.BrandonUtils.Runtime;
using static Runtime.Valuables.ValuableType;

namespace Runtime.Valuables
{
    /// <summary>
    /// Contains the instances of <see cref="ValuableModel"/> for each <see cref="ValuableType"/>.
    ///
    /// Essentially, using <see cref="Models"/> should correspond to using a Java enum.
    /// </summary>
    public static class ValuableDatabase
    {
        /// <summary>
        /// A convenient accessor for the enumerated <see cref="ValuableType"/> constants, similar to Java's <c>enum.values()</c>.
        /// </summary>
        public static readonly ValuableType[] ValuableTypes = Enum.GetValues(typeof(ValuableType)) as ValuableType[];

        public static readonly Dictionary<ValuableType, ValuableModel> Models =
            new Dictionary<ValuableType, ValuableModel>()
            {
                [Penny] = new ValuableModel(
                    Penny,
                    1,
                    new Noun("Penny", "Pennies")
                ),

                [Dollar] = new ValuableModel(
                    Dollar,
                    10,
                    new Noun("Dollar")
                ),

                [Silver] = new ValuableModel(
                    Silver,
                    500,
                    new Noun("Silver Bar")
                ),

                [Gold] = new ValuableModel(
                    Gold,
                    2500,
                    new Noun("Gold Bar")
                ),

                [Gem] = new ValuableModel(
                    Gem,
                    10000,
                    new Noun("Gem")
                ),
            };
    }
}