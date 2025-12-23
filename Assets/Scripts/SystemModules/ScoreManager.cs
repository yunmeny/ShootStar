using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScoreManager : PersistentSingleton<ScoreManager>
{
    #region SCORE DISPLAY

    private int _score;
    private int currentScore;
    public int ScoreAmount => _score;
    Vector3  scoreTextScale = new Vector3(1.4f, 1.4f, 1.4f);
    
    public void ResetScore()
    {
        _score = 0;
        currentScore = 0;
        Score.UpdateScoreText(_score);
    }

    public void AddScore(int score)
    {
        currentScore += score;
        // Score.UpdateScoreText(_score);
        StartCoroutine(nameof(AddScoreCoroutine));
    }

    IEnumerator AddScoreCoroutine()
    {
        Score.ScaleText(scoreTextScale);
        while (currentScore > _score)
        {
            _score += 2;
            Score.UpdateScoreText(_score);
            yield return null;
        }
        Score.ScaleText(Vector3.one);
    }
    #endregion

    #region HIGH SCORE SYSTEM

    [System.Serializable] public class PlayerScore
    {
        public int score;
        public string playerName;

        public PlayerScore(int score, string playerName)
        {
            this.score = score;
            this.playerName = playerName;
        }
    }


    [System.Serializable] public class PlayerScoreDate
    {
        public List<PlayerScore> list = new List<PlayerScore>();
    }
    
    readonly string SaveFileName = "player_score.json";
    
    string playerName = "No Name";
    
    public bool HasNewHighScore() => _score > LoadPlayerScoreData().list[9].score;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    public void SavePlayerScoreData()
    {
        var playerScoreData = LoadPlayerScoreData();
        
        playerScoreData.list.Add(new PlayerScore(_score, playerName));
        playerScoreData.list.Sort((a, b) => b.score.CompareTo(a.score));
        
        SaveSystem.SaveByJson(SaveFileName, playerScoreData);
    }
    
    public PlayerScoreDate LoadPlayerScoreData()
    {
        var playerScoreData = new PlayerScoreDate();
        if (SaveSystem.SaveFileExists(SaveFileName))
        {
            playerScoreData = SaveSystem.LoadFromJson<PlayerScoreDate>(SaveFileName);
        }
        else
        {
            while (playerScoreData.list.Count < 10)
            {
                playerScoreData.list.Add(new PlayerScore(0, playerName));
            }
            SaveSystem.SaveByJson(SaveFileName, playerScoreData);
        }
        return playerScoreData;
    }
    #endregion
}