using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace Packages.BrandonUtils.Runtime {
    /// <summary>
    /// A convenient shortcut for a <see cref="PropertyConstraint"/> with the <see cref="PropertyConstraint.name"/> "Value".
    /// </summary>
    /// <example>
    /// Given the following <see cref="Dictionary{TKey,TValue}"/> (which is <see cref="Dictionary{TKey,TValue}.Enumerator">Enumerated</see> as <see cref="KeyValuePair{TKey,TValue}"/>s, which contain <see cref="KeyValuePair{TKey,TValue}.Value"/>s):
    /// <code><![CDATA[
    /// var dic = new Dictionary<string, int> {
    ///     {"one", 1},
    ///     {"two", 2},
    ///     {"three", 3}
    /// };
    /// ]]></code>
    ///
    /// The following will produce the same results:
    /// <code><![CDATA[
    /// Assert.That(dic, Has.All.Property("Value").GreaterThan(0));
    /// Assert.That(dic, Has.All.Values().GreaterThan(0));
    /// ]]></code>
    /// </example>
    /// <remarks>
    /// Ideally, we should also be able to use the following syntaxes interchangeably:
    /// <code><![CDATA[
    /// Assert.That(dic, Has.All.Values().GreaterThan(0)); //current syntax
    /// Assert.That(dic, Has.Values().GreaterThan(0));     //fancy syntax
    /// ]]></code>
    /// But, because we <a href="https://stackoverflow.com/questions/249222/can-i-add-extension-methods-to-an-existing-static-class">can't add extension methods to static classes</a> (in this case, <see cref="NUnit.Framework.Has"/>), that would require <a href="https://docs.nunit.org/articles/nunit/extending-nunit/Custom-Constraints.html#custom-constraint-usage-syntax">extending <see cref= "NUnit.Framework.Has"/></a>:
    /// <p><i>
    /// "Provide a static class patterned after NUnit's Is class, with properties or methods that construct your custom constructor. If you like, you can even call it Is and extend NUnit's Is, provided you place it in your own namespace and avoid any conflicts."
    /// </i></p>
    ///
    /// </remarks>
    public class AllValuesConstraint : PrefixConstraint {
        public AllValuesConstraint(IConstraint valueConstraint) : base(valueConstraint) => DescriptionPrefix = $"all dictionary key-value-pair values";

        public override ConstraintResult ApplyTo(object actual) {
            const string propName = "Value";
            if (actual.GetType().GetProperties().Any(it => it.Name == propName)) {
                var value = actual.GetType().GetProperties().First(it => it.Name == propName).GetValue(actual);
                return new ConstraintResult(this, actual, this.BaseConstraint.ApplyTo(value).IsSuccess);
            }
            else {
                throw new ArgumentException($"The actual value must contain a field named {propName}!", nameof(actual));
            }
        }
    }
}