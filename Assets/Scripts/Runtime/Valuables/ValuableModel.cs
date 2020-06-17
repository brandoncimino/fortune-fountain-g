using Packages.BrandonUtils.Runtime;

namespace Runtime.Valuables
{
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
    public class ValuableModel
    {
        /// <summary>
        /// The <see cref="ValuableType"/> of this model.
        /// </summary>
        public ValuableType Type;

        /// <summary>
        /// The base karma value of this valuable - i.e. the karma generated when this valuable is thrown.
        /// </summary>
        public long BaseValue;

        public Noun DisplayName;

        public ValuableModel(ValuableType type, long baseValue, Noun displayName)
        {
            Type = type;
            BaseValue = baseValue;
            DisplayName = displayName;
        }
    }
}
