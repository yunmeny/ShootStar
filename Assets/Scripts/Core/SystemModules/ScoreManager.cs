using System.Collections;
using System.Collections.Generic;
using Core.UI;
using UnityEngine;

namespace Core.SystemModules
{
    public class ScoreManager : PersistentSingleton<ScoreManager>
    {
        #region SCORE DISPLAY

        private int _score;
        private int currentScore;
        public int ScoreAmount => currentScore;
        private Coroutine scoreAnimation;
        private bool scoreSaved;

        public void FinishScore()
        {
            StopAllCoroutines();
            scoreAnimation = null;
            _score = currentScore;
            Score.UpdateScoreText(_score);
            Score.ScaleText(Vector3.one);
        }
        Vector3  scoreTextScale = new Vector3(1.4f, 1.4f, 1.4f);
    
        public void ResetScore()
        {
            StopAllCoroutines();
            scoreAnimation = null;
            scoreSaved = false;
            playerName = "No Name";
            _score = 0;
            currentScore = 0;
            Score.UpdateScoreText(_score);
        }

        public void AddScore(int score)
        {
            if (GameManager.GameState != GameState.Play || score <= 0) return;
            currentScore += score;
            if (scoreAnimation == null) scoreAnimation = StartCoroutine(AddScoreCoroutine());
        }

        IEnumerator AddScoreCoroutine()
        {
            Score.ScaleText(scoreTextScale);
            while (currentScore > _score)
            {
                _score = Mathf.Min(currentScore, _score + Mathf.Max(2, (currentScore - _score) / 8));
                Score.UpdateScoreText(_score);
                yield return null;
            }
            Score.ScaleText(Vector3.one);
            scoreAnimation = null;
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
    
        public bool HasNewHighScore() => ScoreAmount > LoadPlayerScoreData().list[9].score;

        public void SetPlayerName(string name)
        {
            playerName = string.IsNullOrWhiteSpace(name) ? "No Name" : name.Trim();
        }

        public void SavePlayerScoreData()
        {
            if (scoreSaved) return;
            var playerScoreData = LoadPlayerScoreData();
        
            playerScoreData.list.Add(new PlayerScore(ScoreAmount, playerName));
            playerScoreData.list.Sort((a, b) => b.score.CompareTo(a.score));
        
            if (playerScoreData.list.Count > 10) playerScoreData.list.RemoveRange(10, playerScoreData.list.Count - 10);
            SaveSystem.SaveByJson(SaveFileName, playerScoreData);
            scoreSaved = true;
        }
    
        public PlayerScoreDate LoadPlayerScoreData()
        {
            var playerScoreData = new PlayerScoreDate();
            if (SaveSystem.SaveFileExists(SaveFileName))
            {
                playerScoreData = SaveSystem.LoadFromJson<PlayerScoreDate>(SaveFileName);
            }
            if (playerScoreData == null) playerScoreData = new PlayerScoreDate();
            if (playerScoreData.list == null) playerScoreData.list = new List<PlayerScore>();
            playerScoreData.list.RemoveAll(entry => entry == null);
            playerScoreData.list.Sort((a, b) => b.score.CompareTo(a.score));
            if (playerScoreData.list.Count > 10) playerScoreData.list.RemoveRange(10, playerScoreData.list.Count - 10);
            while (playerScoreData.list.Count < 10)
                playerScoreData.list.Add(new PlayerScore(0, "No Name"));
            return playerScoreData;
        }
        #endregion
    }
}