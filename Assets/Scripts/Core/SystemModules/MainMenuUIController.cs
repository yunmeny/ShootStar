using Core.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.SystemModules
{
    public class MainMenuUIController : MonoBehaviour
    {
        [SerializeField] private Button storyButton;
        [SerializeField] private Button endlessButton;
        [SerializeField] private Button optionButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Canvas mainMenuCanvas;
        private UnityEngine.UI.Button buildTestButton;
        private readonly System.Collections.Generic.Dictionary<UnityEngine.UI.Button, string> originalLabels =
            new System.Collections.Generic.Dictionary<UnityEngine.UI.Button, string>();
        private readonly System.Collections.Generic.Dictionary<UnityEngine.UI.Button, (float spacing, bool autoSize)> originalTextStyle =
            new System.Collections.Generic.Dictionary<UnityEngine.UI.Button, (float spacing, bool autoSize)>();

        private void Awake()
        {
            // Reuse the existing button prefab/animation and the container's layout.
            buildTestButton = Instantiate(storyButton, storyButton.transform.parent);
            buildTestButton.name = "BuildTestButton";
            buildTestButton.transform.SetSiblingIndex(endlessButton.transform.GetSiblingIndex() + 1);
            var legacyLabel = buildTestButton.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (legacyLabel) legacyLabel.text = "BUILD TEST";
            var label = buildTestButton.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (label) label.text = "BUILD TEST";
            SetTestLabel(buildTestButton, "BUILD TEST");
            foreach (var button in new[] { storyButton, endlessButton, buildTestButton, optionButton, exitButton })
            {
                var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
                var text = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
                originalLabels[button] = tmp ? tmp.text : text ? text.text : button.name;
                if (tmp) originalTextStyle[button] = (tmp.characterSpacing, tmp.enableAutoSizing);
            }
            // The source button's animation callback is mapped below, just like the other modes.
        }

        void Start()
        {
            Time.timeScale = 1;
            GameManager.GameState = GameState.Start;
            UIInput.Instance.SelectUI(storyButton);
        }
    
        private void OnEnable()
        {
            SceneLoader.OptionsClosed += OnOptionsClosed;
            ButtonPressedState.ButtonFunctionTable.Add(storyButton, OnStoryButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(endlessButton, OnEndlessButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(optionButton, OnOptionButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(exitButton, OnExitButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(buildTestButton, OnBuildTestButtonClick);
        }

        private void OnDisable()
        {
            SceneLoader.OptionsClosed -= OnOptionsClosed;
            ButtonPressedState.ButtonFunctionTable.Remove(storyButton);
            ButtonPressedState.ButtonFunctionTable.Remove(endlessButton);
            ButtonPressedState.ButtonFunctionTable.Remove(optionButton);
            ButtonPressedState.ButtonFunctionTable.Remove(exitButton);
            ButtonPressedState.ButtonFunctionTable.Remove(buildTestButton);
        }

        void OnOptionsClosed()
        {
            mainMenuCanvas.enabled = true;
            UIInput.Instance.SelectUI(optionButton);
        }

        void OnStoryButtonClick()
        {
            SceneLoader.Instance.LoadGamePlayScene();
        }

        void OnEndlessButtonClick()
        {
            SceneLoader.Instance.LoadEndlessScene();
        }

        void OnBuildTestButtonClick()
        {
            SetTestLabel(storyButton, "MAIN GUN");
            SetTestLabel(endlessButton, "MISSILES");
            SetTestLabel(buildTestButton, "MANEUVER");
            SetTestLabel(optionButton, "BACK");
            exitButton.gameObject.SetActive(false);
            ButtonPressedState.ButtonFunctionTable[storyButton] = () => SceneLoader.Instance.LoadBuildTestScene(BuildFocus.MainGun);
            ButtonPressedState.ButtonFunctionTable[endlessButton] = () => SceneLoader.Instance.LoadBuildTestScene(BuildFocus.Missiles);
            ButtonPressedState.ButtonFunctionTable[buildTestButton] = () => SceneLoader.Instance.LoadBuildTestScene(BuildFocus.Maneuver);
            ButtonPressedState.ButtonFunctionTable[optionButton] = CloseBuildChoices;
            UIInput.Instance.SelectUI(storyButton);
        }

        private void CloseBuildChoices()
        {
            foreach (var entry in originalLabels) SetLabel(entry.Key, entry.Value);
            foreach (var entry in originalTextStyle)
            {
                var label = entry.Key.GetComponentInChildren<TMPro.TMP_Text>(true);
                label.characterSpacing = entry.Value.spacing;
                label.enableAutoSizing = entry.Value.autoSize;
            }
            exitButton.gameObject.SetActive(true);
            ButtonPressedState.ButtonFunctionTable[storyButton] = OnStoryButtonClick;
            ButtonPressedState.ButtonFunctionTable[endlessButton] = OnEndlessButtonClick;
            ButtonPressedState.ButtonFunctionTable[buildTestButton] = OnBuildTestButtonClick;
            ButtonPressedState.ButtonFunctionTable[optionButton] = OnOptionButtonClick;
            UIInput.Instance.SelectUI(buildTestButton);
        }

        private static void SetLabel(UnityEngine.UI.Button button, string value)
        {
            var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (tmp) tmp.text = value;
            var text = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (text) text.text = value;
        }

        private static void SetTestLabel(UnityEngine.UI.Button button, string value)
        {
            SetLabel(button, value);
            var label = button.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (!label) return;
            label.characterSpacing = 0f;
            label.enableAutoSizing = true;
            label.fontSizeMin = 28;
            label.fontSizeMax = 70;
        }
    
        void OnOptionButtonClick()
        {
            mainMenuCanvas.enabled = false;
            SceneLoader.Instance.LoadOptionsScene(optionButton);
        }

        void OnExitButtonClick()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
