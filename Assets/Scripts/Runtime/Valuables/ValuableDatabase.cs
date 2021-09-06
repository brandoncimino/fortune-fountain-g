// using System.Collections.Generic;

using System;
using System.Collections.Generic;
using FowlFever.Conjugal;
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
            new Dictionary<ValuableType, ValuableModel> {
                [Coin] = new ValuableModel(
                    Coin,
                    ("coin", "coins"),
                    1,
                    ("Penny", "Pennies"),
                    ("Dollar", "Dollars"),
                    ("Piece of Eight", "Pieces of Eight"),
                    ("Doubloon", "Doubloons"),
                    ("Denarius", "Denarii")
                ),

                [Metal] = new ValuableModel(
                    Metal,
                    ("Precious Metal", "Precious Metals"),
                    10,
                    "Silver",
                    "Gold",
                    "Platinum",
                    "Uranium"
                ),

                [Gem] = new ValuableModel(
                    Gem,
                    "Gem".Plurablize(),
                    500,
                    "Glass".Plurablize(),
                    "Garnet".Plurablize(),
                    "Salt".Plurablize(),
                    "Painite".Plurablize()
                ),

                [Fiduciary] = new ValuableModel(
                    Fiduciary,
                    ("Fiduciary", "Fiduciaries"),
                    2500,
                    ("Gift Card", "Gift Cards"),
                    "Bond".Plurablize(),
                    ("Debt", "Debts"),
                    ("Oath", "Oaths") //represented by a feather quill or fountain pen dripping blood
                ),

                [Scrip] = new ValuableModel(
                    Scrip,
                    Plurable.Uncountable("Script"),
                    10000,
                    "WAS IOU, BUT THAT SHOULD CHANGE",
                    ("Prize Ticket", "Prize Tickets"),
                    ("Bottle Cap", "Bottle Caps"),
                    "Gasoline"
                ),

                [Collectible] = new ValuableModel(
                    Collectible,
                    "Collectible".Plurablize(),
                    360000,
                    ("Stuffed Animal", "Stuffed Animals"),
                    ("Trading Card", "Trading Cards"),
                    ("Postage Stamp", "Postage Stamps"),
                    ("Painting", "Paintings")
                ),

                [Livestock] = new ValuableModel(
                    Livestock,
                    "Livestock",
                    4000000,
                    "Carp",
                    "Chicken".Plurablize(),
                    "Cow".Plurablize(),
                    "Kangaroo".Plurablize()
                ),

                [Speculative] = new ValuableModel(
                    Speculative,
                    ("Speculative Asset", "Speculative Assets"),
                    long.MaxValue,
                    "South Sea Lottery Ticket",
                    ("Stock Share", "Stock Shares"),
                    ("Cryptocoin", "Cryptocoins"),
                    ("Indulgence", "Indulgences")
                ),
            };
    }
}