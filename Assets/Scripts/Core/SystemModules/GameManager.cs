using UnityEngine;

namespace Core.SystemModules
{
    public class GameManager : PersistentSingleton<GameManager>
    {
        public static System.Action onGameOver;
        public static GameState GameState { get => Instance._gameState; set => Instance._gameState = value; }
        private GameState _gameState = GameState.Start;
        public GameMode Mode { get; private set; } = GameMode.Story;
        public bool Victory { get; private set; }

        public void BeginRun(GameMode mode)
        {
            Mode = mode;
            Victory = false;
            _gameState = GameState.Play;
            ScoreManager.Instance.ResetScore();
        }

        public void EndRun(bool victory)
        {
            if (_gameState != GameState.Play) return;
            Victory = victory;
            _gameState = GameState.GameOver;
            ScoreManager.Instance.FinishScore();
            if (TimeController.Instance) TimeController.Instance.ResetTime();
            Time.timeScale = 0f;
            onGameOver?.Invoke();
        }
    }

    public enum GameMode { Story, Endless }
    public enum GameState { Start, Play, Pause, GameOver, Scoring, Options }
}
