using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
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
        ButtonPressedState.ButtonFunctionTable.Clear();
    }

    void ShowRandomBackground()
    {
        background.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
    }

    private void ShowNewHighScoreScreen()
    {
        newHighScoreCanvas.enabled = true;
        UIInput.Instance.SelectUI(cancelButton);
    }

    private void HideNewHighScoreScreen()
    {
        newHighScoreCanvas.enabled = false;
        ScoreManager.Instance.SavePlayerScoreData();
        ShowRandomBackground();
        ShowScoreScreen();
    }
    
    void ShowScoreScreen()
    {
        scoreCanvas.enabled = true;
        playerScoreText.text = ScoreManager.Instance.ScoreAmount.ToString();
        UIInput.Instance.SelectUI(mainMenuButton);
        UpdateHighScoreLeaderboard();
    }

    void UpdateHighScoreLeaderboard()
    {
        var playerScoreList =  ScoreManager.Instance.LoadPlayerScoreData();

        for (var i = 0; i < highScoreLeaderboardContainer.childCount; i++)
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
        HideNewHighScoreScreen();
    }
    
}
