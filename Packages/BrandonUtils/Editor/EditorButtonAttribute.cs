using System;
using System.Reflection;
using NUnit.Framework;

namespace Packages.BrandonUtils.Editor {
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorButtonAttribute : Attribute {
        public static void ValidateMethodInfo(MethodInfo methodInfo) {
            Assert.That(methodInfo.GetParameters().Length, Is.EqualTo(0), $"Methods annotated with {nameof(EditorButtonAttribute)} must have exactly 0 parameters!");
        }

        public void ValidateMethod(MethodInfo methodInfo) {
            ValidateMethodInfo(methodInfo);
        }
    }
}