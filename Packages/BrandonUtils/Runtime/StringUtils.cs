using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Packages.BrandonUtils.Runtime {
    public static class StringUtils {
        /// <summary>
        ///     Prepends <paramref name="toIndent" />.
        /// </summary>
        /// <param name="toIndent">The <see cref="string" /> to be indented.</param>
        /// <param name="indentCount">The number of indentations (i.e. number of times hitting "tab").</param>
        /// <param name="indentSize">The size of each individual indentation.</param>
        /// <param name="indentChar">The character to use for indentation.</param>
        /// <returns></returns>
        public static string Indent(this string toIndent, int indentCount = 1, int indentSize = 2, char indentChar = ' ') {
            return indentChar.ToString().Repeat(indentSize).Repeat(indentCount) + toIndent;
        }

        /// <summary>
        ///     Joins <paramref name="toRepeat" /> with itself <paramref name="repetitions" /> times, using the optional <paramref name="separator" />
        /// </summary>
        /// <param name="toRepeat">The <see cref="string" /> to be joined with itself.</param>
        /// <param name="repetitions">The number of times <paramref name="toRepeat" /> should be repeated.</param>
        /// <param name="separator">An optional character, analogous to </param>
        /// <returns></returns>
        public static string Repeat(this string toRepeat, int repetitions, string separator = "") {
            var list = new List<string>();
            for (var i = 0; i < repetitions; i++) {
                list.Add(toRepeat);
            }

            return string.Join(separator, list);
        }

        /// <inheritdoc cref="Repeat(string,int,string)" />
        public static string Repeat(this char toRepeat, int repetitions, string separator = "") {
            return Repeat(toRepeat.ToString(), repetitions, separator);
        }

        /// <summary>
        ///     Joins together <paramref name="baseString" /> and <paramref name="stringToJoin" /> via <paramref name="separator" />,
        ///     <b>
        ///         <i>UNLESS</i>
        ///     </b>
        ///     <paramref name="baseString" /> is <c>null</c>, in which case <paramref name="stringToJoin" /> is returned.
        /// </summary>
        /// <remarks>
        ///     The idea of this is that it can be used to build out a single string and "list out" items, rather than building a <see cref="List{T}" /> and calling <see cref="string.Join(string,System.Collections.Generic.IEnumerable{string})" /> against it.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// "yolo".Join("swag")     -> yoloswag
        /// ]]></code>
        ///     <code><![CDATA[
        /// "yolo".Join("swag",":") -> yolo; swag
        /// ]]></code>
        ///     <code><![CDATA[
        /// "".Join("swag", ":")    -> swag
        /// ]]></code>
        ///     <code><![CDATA[
        /// null.Join(":")          -> swag
        /// ]]></code>
        /// </example>
        /// <param name="baseString"></param>
        /// <param name="stringToJoin"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string Join(this string baseString, string stringToJoin, string separator = "") {
            return string.IsNullOrEmpty(baseString) ? stringToJoin : string.Join(separator, baseString, stringToJoin);
        }

        public static string Prettify(object thing, bool recursive = true, int recursionCount = 0) {
            const int recursionMax = 10;
            var       type         = thing.GetType().ToString();
            string    method       = null;
            string    prettyString = null;

            switch (thing) {
                //don't do anything special with strings
                //check for value types (int, char, etc.), which we shouldn't do anything fancy with
                case string s:
                    method       = "string";
                    prettyString = s;
                    break;
                case ValueType _:
                    method       = nameof(ValueType);
                    prettyString = thing.ToString();
                    break;
                case IEnumerable enumerableThing:
                    method = $"recursion, {recursionCount}";

                    recursionCount++;

                    if (!recursive || recursionCount >= recursionMax) {
                        goto default;
                    }

                    foreach (var entry in enumerableThing) {
                        prettyString += "\n" + Prettify(entry, true, recursionCount);
                    }

                    break;
                default:
                    try {
                        method       = "JSON";
                        prettyString = JsonUtility.ToJson(thing, true);
                    }
                    catch (Exception) {
                        method = "JSON - FAILED!";
                    }

                    break;
            }

            // account for null prettyString and method
            // (we're doing this here, rather than initializing them to default values, so we can trigger things if there's a failure)
            prettyString = prettyString ?? thing.ToString();
            method       = method ?? "NO METHOD FOUND";


            return $"[{method}]{prettyString}".Indent(indentCount: recursionCount);
        }

        public static string ListVariables(object obj) {
            return ListMembers(obj, MemberTypes.Property | MemberTypes.Field);
        }

        public static string ListProperties(object obj) {
            return ListMembers(obj, MemberTypes.Property);
        }

        public static string ListFields(object obj) {
            return ListMembers(obj, MemberTypes.Field);
        }

        public static string ListMembers(object obj, MemberTypes memberTypes = MemberTypes.All) {
            //if obj is a already a type, cast it and use it; otherwise, grab its type
            Type objType = obj is Type type ? type : obj.GetType();
            return objType.GetMembers().Where(member => memberTypes.HasFlag(member.MemberType)).Aggregate($"[{objType}] {memberTypes}:", (current, member) => current + $"\n\t{FormatMember(member, obj)}");
        }

        public static string FormatMember(MemberInfo memberInfo, object obj = null) {
            var result = $"[{memberInfo.MemberType}] {memberInfo}";

            try {
                if (obj != null) {
                    switch (memberInfo) {
                        case PropertyInfo propertyInfo:
                            result += $": {propertyInfo.GetValue(obj)}";
                            break;
                        case FieldInfo fieldInfo:
                            result += $": {fieldInfo.GetValue(obj)}";
                            break;
                    }
                }
            }
            catch (Exception e) {
                result += $"ERROR: {e.Message}";
            }

            return result;
        }
    }
}