using System.Reflection;
using Packages.BrandonUtils.Runtime.Logging;
using UnityEditor;
using UnityEngine;

namespace Packages.BrandonUtils.Editor {
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviorEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var type = target.GetType();

            if (GUILayout.Button($"Button for [{type.Name}]")) {
                LogUtils.Log($"My type is: {type}");
            }

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)) {
                var buttonAttribute = method.GetCustomAttribute<EditorButtonAttribute>();
                if (buttonAttribute != null) {
                    buttonAttribute.ValidateMethod(method);

                    if (ButtonForMethod(method)) {
                        LogUtils.Log($"<b>Invoke {method}</b>, declared by {method.DeclaringType}");
                        method.Invoke(target, new object[] { });
                    }
                }
            }
        }

        private bool ButtonForMethod(MethodInfo methodInfo) {
            return GUILayout.Button($"Invoke: <b>{methodInfo.Name}()</b>", new GUIStyle("Button") {richText = true, alignment = TextAnchor.MiddleLeft});
        }
    }
}