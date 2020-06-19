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
                    type: Coin,
                    immutableValue: 1,
                    magnitudes: new[] {
                        new Noun("Penny", "Pennies"),
                        new Noun("Dollar"),
                        new Noun("Piece of Eight", "Pieces of Eight"),
                        new Noun("Doubloon")
                    }
                ),

                [Metal] = new ValuableModel(
                    type: Metal,
                    immutableValue: 10,
                    magnitudes: new[] {
                        new Noun("Silver"),
                        new Noun("Gold"),
                        new Noun("Platinum"),
                        new Noun("Uranium")
                    },
                    displayName: new Noun("Precious Metal")
                ),

                [Gem] = new ValuableModel(
                    type: Gem,
                    immutableValue: 500,
                    magnitudes: new[] {
                        new Noun("Glass"),
                        new Noun("Garnet"),
                        new Noun("Salt"),
                        new Noun("Painite")
                    }
                ),

                [Fiduciary] = new ValuableModel(
                    type: Fiduciary,
                    immutableValue: 2500,
                    magnitudes: new[] {
                        new Noun("Gift Card"),
                        new Noun("Bond"),
                        new Noun("Debt"),
                        new Noun("Oath") //represented by a feather quill or fountain pen dripping blood
                    },
                    new Noun("Fiduciary", "Fiduciaries")
                ),

                [Scrip] = new ValuableModel(
                    type: Scrip,
                    immutableValue: 10000,
                    magnitudes: new[] {
                        new Noun("WAS IOU, BUT THAT SHOULD CHANGE"),
                        new Noun("Arcade Token"),
                        new Noun("Bottle Cap"),
                        new Noun("Gasoline")
                    }
                ),

                [Collectible] = new ValuableModel(
                    type: Collectible,
                    immutableValue: 360000,
                    magnitudes: new[] {
                        new Noun("Stuffed Animal"),
                        new Noun("Trading Card"),
                        new Noun("Postage Stamp"),
                        new Noun("Painting")
                    }
                ),

                [Livestock] = new ValuableModel(
                    type: Livestock,
                    immutableValue: 4000000,
                    magnitudes: new[] {
                        new Noun("Carp", "Carp"),
                        new Noun("Chicken"),
                        new Noun("Cow"),
                        new Noun("Kangaroo")
                    }
                ),

                [Speculative] = new ValuableModel(
                    type: Speculative,
                    immutableValue: long.MaxValue,
                    magnitudes: new[] {
                        new Noun("South Sea Lottery Ticket"),
                        new Noun("Stock Share"),
                        new Noun("Cryptocoin"),
                        new Noun("Indulgence")
                    }
                ),
            };
    }
}