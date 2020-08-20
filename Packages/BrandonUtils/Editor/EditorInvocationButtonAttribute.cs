using System;
using System.Reflection;
using NUnit.Framework;

namespace Packages.BrandonUtils.Editor {
    /// <summary>
    /// Creates a button in the Unity editor that will execute the annotated <see cref="AttributeTargets.Method"/>.
    /// </summary>
    /// <remarks>
    /// <li>The actual code that creates the button and invokes the method is located in <see cref="MonoBehaviorEditor"/>.</li>
    /// <li>Works with both public and private methods.</li>
    /// <li>Works with both static and instance methods.</li>
    /// <li>Works with both declared and inherited methods.</li>
    /// <li>Works in both Editor and Play mode.</li>
    /// <li>While it is theoretically possible to pass values to the <see cref="MethodInfo"/> invocation, it is currently unsupported.</li>
    /// <li><see cref="ValidateMethodInfo"/> will throw an exception if the given <see cref="MethodInfo"/> is invalid - which in this case would be anything that contains parameters.</li>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class EditorInvocationButtonAttribute : Attribute {
        public static void ValidateMethodInfo(MethodInfo methodInfo) {
            Assert.That(methodInfo.GetParameters().Length, Is.EqualTo(0), $"Methods annotated with {nameof(EditorInvocationButtonAttribute)} must have exactly 0 parameters!");
        }

        public void ValidateMethod(MethodInfo methodInfo) {
            ValidateMethodInfo(methodInfo);
        }
    }
}