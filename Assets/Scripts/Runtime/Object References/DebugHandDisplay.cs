using System.Linq;
using Runtime.Valuables;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Object_References {
    public class DebugHandDisplay : MonoBehaviour {
        public Text DebugHandText;
        public Text DebugKarmaText;

        public void Update() {
            // DebugHandText.text = JsonUtility.ToJson(GameManager.SaveData.Hand, true);
            UpdateDebugHandText();
            UpdateDebugKarmaText();
        }

        private void UpdateDebugHandText() {
            var lines = GameManager.SaveData.Hand
                .Throwables
                .Select(it => it as ThrowableValuable)
                .Where(it => it != null)
                .GroupBy(it => it.ValuableType)
                .Select(FormatPlayerValuableGroup);
            DebugHandText.text = string.Join("\n", lines);
        }

        private static string FormatPlayerValuableGroup(IGrouping<ValuableType, ThrowableValuable> grouping) {
            return $"{grouping.Key}: {grouping.Count()} ({string.Join(", ", grouping.Select(it => it.PresentValue))})";
        }

        private void UpdateDebugKarmaText() {
            DebugKarmaText.text =
                $"Wallet: {GameManager.SaveData.Karma}\nHand: {GameManager.SaveData.Hand.KarmaInHand}";
        }
    }
}