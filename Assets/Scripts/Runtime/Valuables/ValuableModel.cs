using Packages.BrandonUtils.Runtime;

namespace Runtime.Valuables
{
    /// <summary>
    /// Contains information about each <b>type</b> of valuable (as opposed to an <b>instance</b> of a valuable),
    /// such as base price, textures, etc.
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