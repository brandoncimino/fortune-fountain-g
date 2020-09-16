using System;

using NUnit.Framework;

using Packages.BrandonUtils.Runtime;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Packages.BrandonUtils.Tests {
    public class ReflectionUtilsTests {
        private class Privacy<T> {
            public    T Field_Public;
            private   T Field_Private;
            protected T Field_Protected;

            public    T Prop_Public               { get;         set; }
            private   T Prop_Private              { get;         set; }
            protected T Prop_Protected            { get;         set; }
            public    T Prop_Mixed_Private_Getter { private get; set; }
            public    T Prop_Mixed_Private_Setter { get;         private set; }
            public    T Prop_Get_Only             => Prop_Public;

            public static    T Field_Static_Public;
            private static   T Field_Static_Private;
            protected static T Field_Static_Protected;

            public static    T Prop_Static_Public               { get;         set; }
            private static   T Prop_Static_Private              { get;         set; }
            protected static T Prop_Static_Protected            { get;         set; }
            public static    T Prop_Static_Mixed_Private_Getter { private get; set; }
            public static    T Prop_Static_Mixed_Private_Setter { get;         private set; }
            public static    T Prop_Static_Get_Only             => Prop_Static_Public;


            public Privacy(T value) {
                Field_Public    = value;
                Field_Private   = value;
                Field_Protected = value;

                Prop_Public               = value;
                Prop_Private              = value;
                Prop_Protected            = value;
                Prop_Mixed_Private_Getter = value;
                Prop_Mixed_Private_Setter = value;

                Field_Static_Public    = value;
                Field_Static_Private   = value;
                Field_Static_Protected = value;

                Prop_Static_Public               = value;
                Prop_Static_Private              = value;
                Prop_Static_Protected            = value;
                Prop_Static_Mixed_Private_Getter = value;
                Prop_Static_Mixed_Private_Setter = value;
            }

            public static string[] VariableNames = {
                nameof(Field_Public),
                nameof(Field_Private),
                nameof(Field_Protected),
                nameof(Prop_Public),
                nameof(Prop_Private),
                nameof(Prop_Protected),
                nameof(Prop_Mixed_Private_Getter),
                nameof(Prop_Mixed_Private_Setter),
                nameof(Field_Static_Public),
                nameof(Field_Static_Private),
                nameof(Field_Static_Protected),
                nameof(Prop_Static_Public),
                nameof(Prop_Static_Private),
                nameof(Prop_Static_Protected),
                nameof(Prop_Static_Mixed_Private_Getter),
                nameof(Prop_Static_Mixed_Private_Setter),
            };
        }

        public static string[] VariableNames = Privacy<int>.VariableNames;

        [Test]
        public void GetVariable(
            [ValueSource(
                nameof(VariableNames)
            )]
            [Values(
                nameof(Privacy<object>.Prop_Get_Only),
                nameof(Privacy<object>.Prop_Static_Get_Only)
            )]
            string variableName
        ) {
            var randomInt = new Random().Next();

            var privacy = new Privacy<int>(randomInt);

            var val = ReflectionUtils.GetVariable<int>(privacy, variableName);
            Assert.That(val, Is.EqualTo(randomInt));
        }

        [Test]
        public void SetVariable(
            [ValueSource(nameof(VariableNames))]
            string variableName
        ) {
            var randomInt = new Random().Next();
            var setInt    = randomInt + 1;

            var privacy = new Privacy<int>(randomInt);

            ReflectionUtils.SetVariable(privacy, variableName, setInt);
            Assert.That(ReflectionUtils.GetVariable(privacy, variableName), Is.EqualTo(setInt));
            Assert.That(ReflectionUtils.GetVariable(privacy, variableName), Is.Not.EqualTo(randomInt));
        }
    }
}