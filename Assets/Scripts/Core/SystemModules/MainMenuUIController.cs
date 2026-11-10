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
            SceneLoader.OptionsClosed += OnOptionsClosed;
            ButtonPressedState.ButtonFunctionTable.Add(storyButton, OnStoryButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(endlessButton, OnEndlessButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(optionButton, OnOptionButtonClick);
            ButtonPressedState.ButtonFunctionTable.Add(exitButton, OnExitButtonClick);
        }

        private void OnDisable()
        {
            SceneLoader.OptionsClosed -= OnOptionsClosed;
            ButtonPressedState.ButtonFunctionTable.Remove(storyButton);
            ButtonPressedState.ButtonFunctionTable.Remove(endlessButton);
            ButtonPressedState.ButtonFunctionTable.Remove(optionButton);
            ButtonPressedState.ButtonFunctionTable.Remove(exitButton);
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
