using Core.SystemModules;
using TMPro;
using UnityEngine;

namespace Core.UI
{
    public class CombatBuildHUD : MonoBehaviour
    {
        private Canvas canvas;
        private TextMeshProUGUI status;
        private RunUpgradeController upgrades;
        private float nextRefresh;

        private void Awake()
        {
            upgrades = GetComponent<RunUpgradeController>();
            var root = new GameObject("CombatBuildHUD", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            status = new GameObject("BuildStatus", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            status.font = RunUpgradeScreen.ChineseFont;
            var rect = status.rectTransform;
            rect.SetParent(root.transform, false);
            rect.anchorMin = new Vector2(0.24f, 0.88f);
            rect.anchorMax = new Vector2(0.76f, 0.99f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            status.fontSize = 24;
            status.color = new Color(0.55f, 0.95f, 1f);
            status.alignment = TextAlignmentOptions.Top;
            status.raycastTarget = false;
        }

        private void Update()
        {
            canvas.enabled = GameManager.GameState == GameState.Play;
            if (!canvas.enabled) return;
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;
            var run = upgrades.Run;
            string text = "";
            if (run.HasSlipstream) text = "翻滚加速 " + upgrades.Combat.BoostRemaining.ToString("0.0") + " 秒";
            if (run.HasKineticBattery) text += "  |  充能 " + upgrades.Combat.Charge + "/100";
            if (run.HasReclaimer) text += "  |  导弹回收 " + upgrades.Combat.ReclaimerProgress + "/3";
            if (!string.IsNullOrEmpty(upgrades.CombatMessage)) text += "\n" + upgrades.CombatMessage;
            if (GameManager.Instance.IsEncounterTest)
            {
                var director = EenemyManager.Instance;
                text = "遭遇测试 " + director.WaveNumber + "/7  |  SEED " + director.EncounterSeed +
                    "  |  撤离 " + director.EncounterEscaped + "  |  " + director.EncounterStatus + "\n" + director.EncounterHint + "\n" + text;
            }
            status.text = text;
        }
    }
}
