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

        void Start()
        {
            Time.timeScale = 1;
            GameManager.GameState = GameState.Start;
            UIInput.Instance.SelectUI(storyButton);
        }
    
        private void OnEnable()
        {
            ButtonPressedState.ButtonFunctionTable.Add(storyButton, OnStoryButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(endlessButton, OnEndlessButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(optionButton, OnOptionButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(exitButton, OnExitButtonClick);
        }

        private void OnDisable()
        {
            ButtonPressedState.ButtonFunctionTable.Clear();
        }

        void OnStoryButtonClick()
        {
            mainMenuCanvas.enabled = false;
            SceneLoader.Instance.LoadGamePlayScene();
        }

        void OnEndlessButtonClick()
        {
            // TODO: 实现无尽模式
        }
    
        void OnOptionButtonClick()
        {
            mainMenuCanvas.enabled = false;
            SceneLoader.Instance.LoadOptionsScene();
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
