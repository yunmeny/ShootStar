using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoader : PersistentSingleton<SceneLoader>
{
    [SerializeField] private Image loadingImage;
    [SerializeField] private float fadeDuration = 1f;
    private Color _loadingImageColor;
    const string GAMEPLAY_SCENE = "GamePlay";
    const string MAIN_MENU_SCENE = "MainMenu";
    const string SCROE_SCENE = "Score";
    const string OPTIONS_SCENE = "Options";
    
    public void LoadScene(string sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

    public void LoadGamePlayScene()
    {
        StopAllCoroutines();
        StartCoroutine(LoadingScene(GAMEPLAY_SCENE));
        GameManager.GameState = GameState.Play;
    }

    public void LoadMainMenuScene()
    {
        StopAllCoroutines();
        StartCoroutine(LoadingScene(MAIN_MENU_SCENE));
        GameManager.GameState = GameState.Start;
    }

    public void LoadScoreScene()
    {
        StopAllCoroutines();
        StartCoroutine(LoadingScene(SCROE_SCENE));
        GameManager.GameState = GameState.Scoring;
    }

    public void LoadOptionsScene()
    {
        StopAllCoroutines();
        StartCoroutine(LoadingScene(OPTIONS_SCENE));
        GameManager.GameState = GameState.Options;
    }
    
     
    IEnumerator LoadingScene(string sceneName)
    {
        
        // 异步加载场景，设置为不允许激活，防止场景提前加载完成
        var loadingOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        loadingOperation.allowSceneActivation = false;
        
        loadingImage.gameObject.SetActive(true);
        _loadingImageColor.a = 0f;
        while (_loadingImageColor.a < 1f)
        {
            _loadingImageColor.a = Mathf.Clamp01(_loadingImageColor.a + Time.unscaledDeltaTime / fadeDuration);
            loadingImage.color = _loadingImageColor;
            yield return null;
        }
        
        // 等待加载完成， 0.9表示场景已经加载完成，但还没有开始激活
        yield return new WaitUntil(() => loadingOperation.progress >= 0.9f);
        
        loadingOperation.allowSceneActivation = true;
        while (_loadingImageColor.a > 0f)
        {
            _loadingImageColor.a = Mathf.Clamp01(_loadingImageColor.a - Time.unscaledDeltaTime / fadeDuration);
            loadingImage.color = _loadingImageColor;
            yield return null;
        }
        
        loadingImage.gameObject.SetActive(false);
    }
}