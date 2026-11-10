using Core.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SystemModules
{
    public class ScoreUIController : MonoBehaviour
    {
        [Header("====== 背景 ======")]
        [SerializeField] Image background;
        [SerializeField] Sprite[] backgrounds;
    
        [Header("====== 玩家分数 ======")]
        [SerializeField] Canvas scoreCanvas;
        [SerializeField] Text playerScoreText;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Transform highScoreLeaderboardContainer;
    
        [Header("====== 新的最高分 ======")]
        [SerializeField] Canvas newHighScoreCanvas;
        [SerializeField] Button cancelButton;
        [SerializeField] Button submitButton;
        [SerializeField] InputField inputField;
    
        private Button retryButton;

        void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 1f;
            scoreCanvas.enabled = false;
            newHighScoreCanvas.enabled = false;
            CreateRetryButton();
            ShowRandomBackground();
            if (ScoreManager.Instance.HasNewHighScore())
            {
                ShowNewHighScoreScreen();
            }
            else
            {
                ShowScoreScreen();
            }
            ButtonPressedState.ButtonFunctionTable.Add(mainMenuButton, OnButtonMainMenuClick);
            ButtonPressedState.ButtonFunctionTable.Add(cancelButton, OnButtonCancelClick);
            ButtonPressedState.ButtonFunctionTable.Add(submitButton, OnButtonSubmitClick);
            GameManager.GameState = GameState.Scoring;
        }


        private void OnDisable()
        {
            ButtonPressedState.ButtonFunctionTable.Remove(mainMenuButton);
            ButtonPressedState.ButtonFunctionTable.Remove(cancelButton);
            ButtonPressedState.ButtonFunctionTable.Remove(submitButton);
            if (retryButton) ButtonPressedState.ButtonFunctionTable.Remove(retryButton);
        }

        void CreateRetryButton()
        {
            retryButton = Instantiate(mainMenuButton, mainMenuButton.transform.parent);
            retryButton.name = "RetryButton";
            retryButton.GetComponentInChildren<Text>().text = "PLAY AGAIN";
            retryButton.onClick = new Button.ButtonClickedEvent();
            retryButton.onClick.AddListener(OnRetryButtonClick);
            var original = (RectTransform)mainMenuButton.transform;
            var rect = (RectTransform)retryButton.transform;
            if (!mainMenuButton.transform.parent.GetComponent<LayoutGroup>())
                rect.anchoredPosition = original.anchoredPosition + Vector2.up * (original.rect.height + 20f);
            retryButton.transform.SetSiblingIndex(mainMenuButton.transform.GetSiblingIndex());
            ButtonPressedState.ButtonFunctionTable[retryButton] = OnRetryButtonClick;
            var navigation = mainMenuButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = retryButton;
            mainMenuButton.navigation = navigation;
            navigation = retryButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnDown = mainMenuButton;
            retryButton.navigation = navigation;
        }

        void OnRetryButtonClick()
        {
            scoreCanvas.enabled = false;
            SceneLoader.Instance.RetryGamePlayScene();
        }

        void ShowRandomBackground()
        {
            background.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
        }

        private void ShowNewHighScoreScreen()
        {
            scoreCanvas.enabled = false;
            newHighScoreCanvas.enabled = true;
            UIInput.Instance.SelectUI(cancelButton);
        }

        private void HideNewHighScoreScreen()
        {
            newHighScoreCanvas.enabled = false;
            ShowRandomBackground();
            ShowScoreScreen();
        }
    
        void ShowScoreScreen()
        {
            scoreCanvas.enabled = true;
            playerScoreText.text = ScoreManager.Instance.ScoreAmount.ToString();
            UIInput.Instance.SelectUI(retryButton);
            UpdateHighScoreLeaderboard();
        }

        void UpdateHighScoreLeaderboard()
        {
            var playerScoreList =  ScoreManager.Instance.LoadPlayerScoreData();

            for (var i = 0; i < Mathf.Min(highScoreLeaderboardContainer.childCount, playerScoreList.list.Count); i++)
            {
                var child = highScoreLeaderboardContainer.GetChild(i);
            
                child.Find("Rank").GetComponent<Text>().text = (i + 1).ToString();
                child.Find("Name").GetComponent<Text>().text = playerScoreList.list[i].playerName;
                child.Find("Score").GetComponent<Text>().text = playerScoreList.list[i].score.ToString();
            }
        }
    
        void OnButtonMainMenuClick()
        {
            scoreCanvas.enabled = false;
            SceneLoader.Instance.LoadMainMenuScene();
        }

        void OnButtonCancelClick()
        {
            HideNewHighScoreScreen();
        }

        void OnButtonSubmitClick()
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                ScoreManager.Instance.SetPlayerName(inputField.text);
            }
            ScoreManager.Instance.SavePlayerScoreData();
            HideNewHighScoreScreen();
        }
    
    }
}
