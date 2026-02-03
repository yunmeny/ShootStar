using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.SystemModules
{
    public class SceneLoader : PersistentSingleton<SceneLoader>
    {
        private static readonly int Pos = Shader.PropertyToID("_Pos");
        [SerializeField] private GameObject TranslationObj;
        [SerializeField] private float fadeDuration = 1f;
        private Color _loadingImageColor;
        const string GAMEPLAY_SCENE = "GamePlay";
        const string MAIN_MENU_SCENE = "MainMenu_Improved";
        const string SCROE_SCENE = "Score";
        const string OPTIONS_SCENE = "Options";

        private Material _material;

        private void Start()
        {
            if (TranslationObj)
            {
                _material = TranslationObj.GetComponent<MeshRenderer>().material;
            }
        }

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
            
            if (_material)
            {
                _material.SetVector(Pos, new Vector4(1,0));
                while (_material.GetVector(Pos).x > -1f)
                {
                    var vec = new Vector4(Mathf.Clamp(_material.GetVector(Pos).x - Time.unscaledDeltaTime / fadeDuration, -1, 1),0);
                    _material.SetVector(Pos, vec);
                    yield return null;
                }
            }
            
            // 等待加载完成， 0.9表示场景已经加载完成，但还没有开始激活
            yield return new WaitUntil(() => loadingOperation.progress >= 0.9f);
        
            loadingOperation.allowSceneActivation = true;

            if (_material)
            {
                while (_loadingImageColor.a < 1f)
                {
                    var vec = new Vector4(Mathf.Clamp(_material.GetVector(Pos).x + Time.unscaledDeltaTime / fadeDuration, -1, 1),0);
                    _material.SetVector(Pos, vec);
                    yield return null;
                }
            }
        }
    }
}