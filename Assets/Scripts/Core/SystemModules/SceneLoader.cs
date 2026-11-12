using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Core.SystemModules
{
    public class SceneLoader : PersistentSingleton<SceneLoader>
    {
        private static readonly int Pos = Shader.PropertyToID("_Pos");
        [SerializeField] private GameObject TranslationObj;
        [SerializeField] private float fadeDuration = 1f;
        const string GAMEPLAY_SCENE = "GamePlay";
        const string MAIN_MENU_SCENE = "MainMenu_Improved";
        const string SCORE_SCENE = "Score";
        const string OPTIONS_SCENE = "Options";
        private Material _material;
        public bool IsLoading { get; private set; }
        private bool optionsFromGameplay;
        private UnityEngine.UI.Button optionsBackTemplate;
        public static event System.Action OptionsClosed;
        readonly List<Camera> suspendedCameras = new List<Camera>();
        readonly List<AudioListener> suspendedListeners = new List<AudioListener>();
        readonly List<Renderer> suspendedRenderers = new List<Renderer>();
        readonly List<Canvas> suspendedCanvases = new List<Canvas>();

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            if (TranslationObj && TranslationObj.TryGetComponent<MeshRenderer>(out var renderer))
            {
                _material = renderer.material;
                _material.SetVector(Pos, new Vector4(1f, 0f));
                renderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (_material) Destroy(_material);
        }

        public void LoadScene(string sceneName) => RequestScene(sceneName);
        public void LoadGamePlayScene() => RequestScene(GAMEPLAY_SCENE, GameMode.Story);
        public void LoadEndlessScene() => RequestScene(GAMEPLAY_SCENE, GameMode.Endless);
        public void LoadBuildTestScene(BuildFocus focus = BuildFocus.MainGun) => RequestScene(GAMEPLAY_SCENE,
            focus == BuildFocus.Missiles ? GameMode.MissileTest : focus == BuildFocus.Maneuver ? GameMode.ManeuverTest : GameMode.BuildTest);
        public void LoadEncounterTestScene(BuildFocus focus = BuildFocus.MainGun) => RequestScene(GAMEPLAY_SCENE,
            focus == BuildFocus.Missiles ? GameMode.EncounterMissiles : focus == BuildFocus.Maneuver ? GameMode.EncounterManeuver : GameMode.EncounterMainGun);
        public void RetryGamePlayScene() => RequestScene(GAMEPLAY_SCENE, GameManager.Instance.Mode);
        public void LoadMainMenuScene() => RequestScene(MAIN_MENU_SCENE);
        public void LoadScoreScene() => RequestScene(SCORE_SCENE);

        void RequestScene(string sceneName, GameMode mode = GameMode.Story)
        {
            if (IsLoading) return;
            IsLoading = true;
            if (TimeController.Instance) TimeController.Instance.ResetTime();
            Time.timeScale = 1f;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (sceneName == GAMEPLAY_SCENE) GameManager.Instance.BeginRun(mode);
            else GameManager.GameState = sceneName == SCORE_SCENE ? GameState.Scoring : GameState.Start;
            StartCoroutine(LoadingScene(sceneName));
        }

        public void LoadOptionsScene() => LoadOptionsScene(null);

        public void LoadOptionsScene(UnityEngine.UI.Button backTemplate)
        {
            if (IsLoading || SceneManager.GetSceneByName(OPTIONS_SCENE).isLoaded) return;
            IsLoading = true;
            optionsBackTemplate = backTemplate;
            optionsFromGameplay = SceneManager.GetActiveScene().name == GAMEPLAY_SCENE;
            GameManager.GameState = GameState.Options;
            StartCoroutine(OpenOptions());
        }

        IEnumerator OpenOptions()
        {
            // Keep the paused game loaded so Back resumes the same run.
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    if (renderer.enabled) { suspendedRenderers.Add(renderer); renderer.enabled = false; }
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                    if (canvas.enabled) { suspendedCanvases.Add(canvas); canvas.enabled = false; }
                foreach (var camera in root.GetComponentsInChildren<Camera>())
                    if (camera.enabled) { suspendedCameras.Add(camera); camera.enabled = false; }
                foreach (var listener in root.GetComponentsInChildren<AudioListener>())
                    if (listener.enabled) { suspendedListeners.Add(listener); listener.enabled = false; }
            }
            var inputEventSystem = Core.UI.UIInput.Instance.GetComponent<UnityEngine.EventSystems.EventSystem>();
            inputEventSystem.enabled = false;
            if (Core.UI.UIInput.Instance)
            {
                Core.UI.UIInput.Instance.DisablePlayerInput();
                Core.UI.UIInput.Instance.DisableUIInput();
            }
            yield return SceneManager.LoadSceneAsync(OPTIONS_SCENE, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByName(OPTIONS_SCENE);
            SceneManager.SetActiveScene(scene);
            // Disable the duplicate before reactivating our original EventSystem.
            foreach (var root in scene.GetRootGameObjects())
                foreach (var eventSystem in root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true))
                    eventSystem.gameObject.SetActive(false);
            inputEventSystem.enabled = true;
            CreateOptionsBackButton(scene);
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                {
                    var label = button.GetComponentInChildren<UnityEngine.UI.Text>();
                    if (label) label.text = "BACK";
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(CloseOptions);
                    Core.UI.ButtonPressedState.ButtonFunctionTable[button] = CloseOptions;
                    if (Core.UI.UIInput.Instance) Core.UI.UIInput.Instance.SelectUI(button);
                }
                foreach (var bar in root.GetComponentsInChildren<UnityEngine.UI.Scrollbar>(true))
                {
                    var label = bar.transform.parent.GetComponentInChildren<UnityEngine.UI.Text>();
                    bool music = label && label.text.Contains("音乐");
                    bar.SetValueWithoutNotify(music ? AudioManager.Instance.BGMVolume : AudioManager.Instance.SfxVolume);
                    if (music) bar.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
                    else bar.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
                }
            }
            IsLoading = false;
        }

        void CreateOptionsBackButton(Scene scene)
        {
            // Reuse the existing menu button styling in this placeholder settings scene.
            if (!optionsBackTemplate)
                optionsBackTemplate = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.GetComponent<UnityEngine.UI.Button>();
            if (!optionsBackTemplate) return;
            foreach (var root in scene.GetRootGameObjects())
            {
                var canvas = root.GetComponentInChildren<Canvas>();
                if (!canvas) continue;
                var button = Instantiate(optionsBackTemplate, canvas.transform);
                button.name = "OptionsBackButton";
                var rect = (RectTransform)button.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
                rect.sizeDelta = new Vector2(320f, 72f);
                rect.anchoredPosition = new Vector2(0f, 60f);
                var layout = button.GetComponent<UnityEngine.UI.LayoutElement>();
                if (!layout) layout = button.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
                layout.ignoreLayout = true;
                return;
            }
        }

        public void CloseOptions()
        {
            if (IsLoading) return;
            IsLoading = true;
            StartCoroutine(CloseOptionsCoroutine());
        }

        IEnumerator CloseOptionsCoroutine()
        {
            var scene = SceneManager.GetSceneByName(OPTIONS_SCENE);
            foreach (var root in scene.GetRootGameObjects())
                foreach (var button in root.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                    Core.UI.ButtonPressedState.ButtonFunctionTable.Remove(button);
            yield return SceneManager.UnloadSceneAsync(scene);
            var previous = SceneManager.GetSceneByName(optionsFromGameplay ? GAMEPLAY_SCENE : MAIN_MENU_SCENE);
            SceneManager.SetActiveScene(previous);
            foreach (var camera in suspendedCameras) if (camera) camera.enabled = true;
            foreach (var listener in suspendedListeners) if (listener) listener.enabled = true;
            foreach (var renderer in suspendedRenderers) if (renderer) renderer.enabled = true;
            foreach (var canvas in suspendedCanvases) if (canvas) canvas.enabled = true;
            suspendedRenderers.Clear();
            suspendedCanvases.Clear();
            suspendedCameras.Clear();
            suspendedListeners.Clear();
            foreach (var root in previous.GetRootGameObjects())
                foreach (var uiInput in root.GetComponentsInChildren<Core.UI.UIInput>(true))
                    uiInput.gameObject.SetActive(true);
            GameManager.GameState = optionsFromGameplay ? GameState.Pause : GameState.Start;
            IsLoading = false;
            OptionsClosed?.Invoke();
        }

        IEnumerator LoadingScene(string sceneName)
        {
            var transitionRenderer = TranslationObj ? TranslationObj.GetComponent<MeshRenderer>() : null;
            try
            {
                if (_material) _material.SetVector(Pos, new Vector4(1f, 0f));
                if (transitionRenderer) transitionRenderer.enabled = true;
                var operation = SceneManager.LoadSceneAsync(sceneName);
                operation.allowSceneActivation = false;
                yield return Fade(1f, -1f);
                yield return new WaitUntil(() => operation.progress >= 0.9f);
                operation.allowSceneActivation = true;
                yield return operation;
                // Render the new scene under the closed transition before revealing it.
                // Its activation frame may include a long synchronous Awake/OnEnable stall.
                yield return null;
                yield return Fade(-1f, 1f);
            }
            finally
            {
                if (_material) _material.SetVector(Pos, new Vector4(1f, 0f));
                if (transitionRenderer) transitionRenderer.enabled = false;
                IsLoading = false;
            }
        }

        IEnumerator Fade(float from, float to)
        {
            if (!_material) yield break;
            _material.SetVector(Pos, new Vector4(from, 0f));
            if (fadeDuration <= 0f)
            {
                _material.SetVector(Pos, new Vector4(to, 0f));
                yield break;
            }

            float elapsed = 0f;
            float maxFrameStep = Mathf.Min(fadeDuration / 10f, 1f / 30f);
            while (elapsed < fadeDuration)
            {
                yield return null;
                // Loading/shader compilation time is not visible animation time.
                // Keep at least several rendered steps even after a long frame.
                elapsed += Mathf.Min(Time.unscaledDeltaTime, maxFrameStep);
                _material.SetVector(Pos, new Vector4(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / fadeDuration)), 0f));
            }
        }
    }
}
