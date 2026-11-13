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
        public bool IsBuildTest => Mode == GameMode.BuildTest || Mode == GameMode.MissileTest || Mode == GameMode.ManeuverTest;
        public bool IsEncounterTest => Mode == GameMode.EncounterMainGun || Mode == GameMode.EncounterMissiles || Mode == GameMode.EncounterManeuver;
        public bool IsTestChapter => IsBuildTest || IsEncounterTest;
        public BuildFocus TestFocus => Mode == GameMode.BuildTest || Mode == GameMode.EncounterMainGun ? BuildFocus.MainGun :
            Mode == GameMode.MissileTest || Mode == GameMode.EncounterMissiles ? BuildFocus.Missiles :
            Mode == GameMode.ManeuverTest || Mode == GameMode.EncounterManeuver ? BuildFocus.Maneuver : BuildFocus.None;

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

    public enum GameMode { Story, Endless, BuildTest, MissileTest, ManeuverTest, EncounterMainGun, EncounterMissiles, EncounterManeuver }
    public enum GameState { Start, Play, Pause, GameOver, Scoring, Options, UpgradeSelection }
}
