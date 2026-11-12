using Core.SystemModules;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI
{
    // Runtime-built uGUI, like the existing settings Back button. No scene wiring required.
    public class RunUpgradeScreen : MonoBehaviour
    {
        public bool CanSelect { get; private set; }
        private GameObject root;
        private TextMeshProUGUI heading;
        private TextMeshProUGUI summary;
        private TextMeshProUGUI controls;
        internal static TMP_FontAsset ChineseFont => Resources.Load<TMP_FontAsset>("Fonts/Text");
        private readonly UnityEngine.UI.Button[] buttons = new UnityEngine.UI.Button[3];
        private readonly TextMeshProUGUI[] labels = new TextMeshProUGUI[3];

        public void Initialize(RunUpgradeController controller)
        {
            root = new GameObject("RunUpgradeCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var backdrop = Rect("Backdrop", root.transform, Vector2.zero, Vector2.one);
            backdrop.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(0.015f, 0.025f, 0.055f, 0.96f);
            heading = Text("Heading", backdrop, new Vector2(0.08f, 0.77f), new Vector2(0.92f, 0.93f), 46);
            Text("Instruction", backdrop, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.77f), 25).text =
                "回收舰船组件 / 选择一项改装后继续";

            var cards = Rect("Choices", backdrop, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.65f));
            var layout = cards.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.spacing = 26;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = layout.childForceExpandHeight = true;
            for (int i = 0; i < 3; i++)
            {
                int choiceIndex = i;
                var card = Rect("Upgrade" + (i + 1), cards, Vector2.zero, Vector2.one);
                var image = card.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.color = Color.white;
                var button = card.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = image;
                var colors = button.colors;
                colors.normalColor = new Color(0.07f, 0.13f, 0.2f);
                colors.highlightedColor = new Color(0.12f, 0.3f, 0.4f);
                colors.selectedColor = new Color(0.12f, 0.3f, 0.4f);
                colors.pressedColor = new Color(0.16f, 0.43f, 0.5f);
                button.colors = colors;
                button.onClick.AddListener(() => controller.Select(choiceIndex));
                buttons[i] = button;
                labels[i] = Text("Description", card, new Vector2(0.07f, 0.08f), new Vector2(0.93f, 0.92f), 28);
                labels[i].alignment = TextAlignmentOptions.TopLeft;
                labels[i].enableAutoSizing = true;
                labels[i].fontSizeMin = 20;
                labels[i].fontSizeMax = 28;
            }
            // Explicit navigation keeps left/right and controller selection inside this modal.
            for (int i = 0; i < 3; i++)
                buttons[i].navigation = new UnityEngine.UI.Navigation
                {
                    mode = UnityEngine.UI.Navigation.Mode.Explicit,
                    selectOnLeft = buttons[(i + 2) % 3], selectOnRight = buttons[(i + 1) % 3]
                };
            summary = Text("RunSummary", backdrop, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.23f), 23);
            controls = Text("Controls", backdrop, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.11f), 22);
            root.SetActive(false);
        }

        public void Show(int wave, RunUpgradeDefinition[] choices, RunUpgradeState run)
        {
            StopAllCoroutines();
            CanSelect = false;
            controls.text = "请松开射击键和确认键，再选择改装";
            foreach (var button in buttons) button.interactable = false;
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            heading.text = "第 " + wave + " 波已清除 / 战地改装";
            for (int i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                string rank = choice.IsService ? "即时补给" : "等级 " + (run.Rank(choice.Id) + 1) + " / " + choice.MaxRank;
                labels[i].text = "<color=#67E4ED>" + rank + "</color>\n\n<b>" + choice.Title + "</b>\n\n" + choice.Description;
                if (choice.Prerequisite.HasValue)
                    labels[i].text += "\n\n<color=#67E4ED>前置：" + RunUpgradeState.Definition(choice.Prerequisite.Value).Title + "（已安装）</color>";
            }
            summary.text = "已改装 " + run.Selections + " 次  |  主炮伤害 " + run.MainGunDamageMultiplier.ToString("0.00") +
                " 倍  |  射速 " + run.FireRateMultiplier.ToString("0.00") + " 倍  |  机体 " + run.HullMultiplier.ToString("0.00") + " 倍\n" +
                BuildSummary(run);
            if (GameManager.Instance.IsTestChapter)
                heading.text = FocusName(GameManager.Instance.TestFocus) +
                    (GameManager.Instance.IsEncounterTest ? "遭遇试炼 " : "改装试炼 ") + wave + " / 6 已完成";
            root.SetActive(true);
            if (UIInput.Instance) UIInput.Instance.EnableUIInput();
            StartCoroutine(ArmAfterRelease());
        }

        private static string BuildSummary(RunUpgradeState run)
        {
            string main = run.HasSplit ? "穿透 + 分裂" : run.HasPierce ? "穿透" : "标准";
            string missile = run.HasReclaimer ? "弹头回收" : run.HasChainDetonation ? "连锁引爆" : run.HasWideWarhead ? "扩域爆炸" : "标准";
            string maneuver = run.HasOverdriveRelay ? "极速继电" : run.HasKineticBattery ? "动能电池" : run.HasSlipstream ? "翻滚加速" : "标准";
            return "主炮：" + main + "  |  导弹：" + missile + "  |  机动：" + maneuver;
        }

        private static string FocusName(BuildFocus focus)
        {
            switch (focus)
            {
                case BuildFocus.MainGun: return "主炮";
                case BuildFocus.Missiles: return "导弹";
                case BuildFocus.Maneuver: return "机动";
                default: return "";
            }
        }

        private IEnumerator ArmAfterRelease()
        {
            // Let the newly enabled UI actions process their initial state while cards are locked.
            yield return null;
            do
            {
                while (UIInput.Instance && UIInput.Instance.IsConfirmationHeld) yield return null;
                // Drain pointer-up/click and submit events before making the cards interactive.
                // Use frames rather than scaled time: the game is frozen during refit.
                yield return null;
            } while (UIInput.Instance && UIInput.Instance.IsConfirmationHeld);

            foreach (var button in buttons) button.interactable = true;
            CanSelect = true;
            controls.text = "点击安装 / 方向键或手柄十字键选择，确认键安装\n改装仅在本局生效。完成后需重新按下射击键。";
            if (UIInput.Instance) UIInput.Instance.SelectUI(buttons[0]);
        }

        public void Hide()
        {
            StopAllCoroutines();
            CanSelect = false;
            if (EventSystem.current) EventSystem.current.SetSelectedGameObject(null);
            root.SetActive(false);
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static TextMeshProUGUI Text(string name, Transform parent, Vector2 min, Vector2 max, float size)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = ChineseFont;
            text.fontSize = size;
            text.color = new Color(0.92f, 0.96f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }
    }
}
