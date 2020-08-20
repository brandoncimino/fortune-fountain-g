using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Packages.BrandonUtils.Runtime.Logging {
    [RequireComponent(typeof(Text))]
    public class LogUtils : MonoBehaviour {
        [Flags]
        public enum Locations {
            None,
            Console,
            Unity,
            UI,
            All = Console | Unity | UI
        }

        public static List<string> lines     = new List<string>();
        public static Locations    locations = Locations.All;
        private       Text         _text;

        private void Awake() {
            _text = GetComponent<Text>();
            Log($"Started at: {DateTime.Now} with locations {locations}");
        }

        public static void Log(params object[] stuffToLog) {
            Log(null, stuffToLog);
        }

        public static void Log(Color? color, params object[] stuffToLog) {
            foreach (var thing in stuffToLog) {
                lines.Add(Colorize(thing, color));
            }

            var joinedLines = string.Join("\n", stuffToLog);

            if (locations.HasFlag(Locations.Console)) {
                Console.WriteLine(joinedLines);
            }

            if (locations.HasFlag(Locations.Unity)) {
                Debug.Log(joinedLines);
            }
        }

        public static string Colorize(object thing, Color? color) {
            return color == null ? $"{thing}" : $"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>{thing}</color>";
        }

        public void Update() {
            _text.enabled = locations.HasFlag(Locations.UI);

            _text.text = string.Join("\n", lines);
        }
    }
}