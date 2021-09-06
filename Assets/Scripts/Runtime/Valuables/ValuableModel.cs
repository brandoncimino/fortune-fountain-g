using System;
using FowlFever.Conjugal;
using JetBrains.Annotations;

namespace Runtime.Valuables {
    /// <summary>
    /// Contains information about each <b>type</b> of valuable (as opposed to an <b>instance</b> of a valuable),
    /// such as base price, textures, etc.
    ///
    /// Combined with <see cref="ValuableType"/> to produce "Java-style" enums accessed via <see cref="ValuableDatabase"/>.
    ///
    /// Should never be instantiated outside of <see cref="ValuableDatabase"/>.
    ///
    /// TODO: Introduce some mechanism that will prevent ValuableModel from being instantiated outside of <see cref="ValuableDatabase"/>.
    ///   There is probably some level of security (i.e. protected, internal, etc.) that will properly restrict this.
    /// </summary>
    public class ValuableModel {
        /// <summary>
        /// The <see cref="ValuableType"/> of this model.
        /// </summary>
        public ValuableType Type;

        /// <summary>
        /// The immutable, "raw" karma value of this valuable - i.e. the value that will <b>never change</b> during the course of play.
        /// </summary>
        public readonly long ImmutableValue;

        /// <summary>
        /// The name of this valuable <b>irrespective of magnitude</b>.
        /// </summary>
        public readonly Plurable DisplayName;

        /// <summary>
        /// The different real-world objects that this valuable can appear as, in order of magnitude.
        /// </summary>
        public readonly Plurable[] Denominations;

        /// <summary>
        /// Constructs a new <see cref="ValuableModel"/>.
        ///
        /// Should <b>only</b> be called when the <see cref="ValuableDatabase"/> is initialized.
        ///
        /// Should use explicit parameter names whenever possible.
        /// </summary>
        /// <param name="type"><see cref="Type"/></param>
        /// <param name="immutableValue"><see cref="ImmutableValue"/></param>
        /// <param name="denominations"><see cref="Denominations"/></param>
        /// <param name="displayName"><see cref="DisplayName"/>. Defaults to <paramref name="type"/>.</param>
        /// <exception cref="ArgumentException">if <paramref name="denominations"/> is empty.</exception>
        /// <exception cref="ArgumentNullException">if <paramref name="denominations"/> or any of its items are null.</exception>
        public ValuableModel(
            ValuableType type,
            Plurable displayName,
            long immutableValue,
            [NotNull] params Plurable[] denominations
        ) {
            if (denominations.Length == 0) {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(denominations));
            }

            Type = type;
            DisplayName = displayName;
            ImmutableValue = immutableValue;
            Denominations = denominations ?? throw new ArgumentNullException(nameof(denominations));
        }
    }
}