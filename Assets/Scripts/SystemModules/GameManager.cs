public class GameManager : PersistentSingleton<GameManager>
{
    
    public static System.Action onGameOver; // 没有使用event的原因是因为event只能在内部使用 
    
    public static GameState GameState { get => Instance._gameState; set => Instance._gameState = value; }
    private GameState _gameState = GameState.Start;

}


public enum GameState
{
    Start,
    Play,
    Pause,
    GameOver,
    Scoring,
    Options
}