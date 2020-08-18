using NUnit.Framework.Constraints;

namespace Packages.BrandonUtils.Runtime.Testing {
    /// <summary>
    /// <see cref="CollectionOperator"/> that applies the <see cref="AllValuesConstraint"/>.
    /// Analogous to <see cref="PropOperator"/>.
    /// </summary>
    public class ValuesOperator : CollectionOperator {
        public override IConstraint ApplyPrefix(IConstraint constraint) => (IConstraint) new AllValuesConstraint(constraint);
    }
}