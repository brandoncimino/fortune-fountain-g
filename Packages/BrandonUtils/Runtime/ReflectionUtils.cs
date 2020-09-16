using System;
using System.Reflection;

using JetBrains.Annotations;

using Packages.BrandonUtils.Runtime.Exceptions;

namespace Packages.BrandonUtils.Runtime {
    /// <summary>
    /// Contains utilities for <see cref="System.Reflection"/>.
    /// </summary>
    public static class ReflectionUtils {
        public const BindingFlags VariablesBindingFlags =
            BindingFlags.Default |
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.Static;

        /// <summary>
        /// Returns the <see cref="MethodInfo"/> for this method that <b><i>called <see cref="ThisMethod"/></i></b>.
        /// </summary>
        /// <returns></returns>
        public static MethodInfo ThisMethod() {
            throw new NotImplementedException("TBD - I need to stop getting distracted!");
        }

        [UsedImplicitly]
        [Pure]
        public static MemberInfo GetVariableInfo(this Type type, string variableName) {
            var prop = type.GetProperty(variableName, VariablesBindingFlags);
            if (!(prop is null)) {
                return prop;
            }

            var field = type.GetField(variableName, VariablesBindingFlags);
            if (!(field is null)) {
                return field;
            }

            throw new NotImplementedException($"The {nameof(type)} {type} did not have a field or property named {variableName}!");
        }

        [Pure]
        public static MemberInfo GetVariableInfo<T>(T obj, string variableName) {
            return typeof(T).GetVariableInfo(variableName);
        }

        [Pure]
        public static MemberInfo GetVariableInfo<T>(string variableName) {
            return typeof(T).GetVariableInfo(variableName);
        }

        [UsedImplicitly]
        [Pure]
        public static T GetVariable<T>(object obj, string variableName) {
            var v = obj.GetType().GetVariableInfo(variableName);
            switch (v) {
                case PropertyInfo prop:
                    try {
                        return (T) prop.GetValue(obj);
                    }
                    catch (InvalidCastException e) {
                        throw new InvalidCastException($"A property named {variableName} was found for the {obj.GetType().Name} {obj}, but it couldn't be cast to a {typeof(T).Name}!", e);
                    }
                case FieldInfo field:
                    try {
                        return (T) field.GetValue(obj);
                    }
                    catch (InvalidCastException e) {
                        throw new InvalidCastException($"A field named {variableName} was found for the {obj.GetType().Name} {obj}, but it couldn't be cast to a {typeof(T).Name}!", e);
                    }
            }

            throw new BrandonException($"Couldn't find a field or property named {variableName} for the type {obj.GetType().Name}!");
        }

        [Pure]
        public static object GetVariable(object obj, string variableName) {
            return GetVariable<object>(obj, variableName);
        }

        public static void SetVariable<T>(object obj, string variableName, T value) {
            var v = obj.GetType().GetVariableInfo(variableName);
            switch (v) {
                case PropertyInfo prop:
                    prop.SetValue(obj, value);
                    return;
                case FieldInfo field:
                    field.SetValue(obj, value);
                    return;
                default:
                    throw new BrandonException($"Couldn't find a field or property named {variableName} for the type {obj.GetType()}!");
            }
        }
    }
}