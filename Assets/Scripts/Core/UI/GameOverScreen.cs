using System.Collections;
using Core.SystemModules;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] PlayerInput playerInput;
        [SerializeField] Canvas hudCanvas;
        [SerializeField] AudioData gameOverSFX;
        [SerializeField] float inputDelay = 1f;
        Canvas gameOverCanvas;
        Animator animator;
        bool confirmed;

        void Awake()
        {
            gameOverCanvas = GetComponent<Canvas>();
            animator = GetComponent<Animator>();
            gameOverCanvas.enabled = false;
            if (animator)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.enabled = false;
            }
        }

        void OnEnable()
        {
            GameManager.onGameOver += OnGameOver;
            playerInput.onConfirmGameOver += OnConfirmGameOver;
        }

        void OnDisable()
        {
            GameManager.onGameOver -= OnGameOver;
            playerInput.onConfirmGameOver -= OnConfirmGameOver;
        }

        void OnGameOver()
        {
            confirmed = false;
            hudCanvas.enabled = false;
            gameOverCanvas.enabled = true;
            var title = transform.Find("Texts/Game Over Text");
            if (title) title.GetComponent<Text>().text = GameManager.Instance.Victory ? "STAGE CLEAR!" : "GAME OVER!";
            if (animator) animator.enabled = true;
            playerInput.DisableAllInput();
            // Keep this fallback even if an animation clip loses its input event.
            StartCoroutine(EnableInputAfterDelay());
        }

        IEnumerator EnableInputAfterDelay()
        {
            yield return new WaitForSecondsRealtime(inputDelay);
            EnableGameOverScreenInput();
        }

        void EnableGameOverScreenInput()
        {
            if (!confirmed && GameManager.GameState == GameState.GameOver)
                playerInput.EnableGameOverInput();
        }

        void OnConfirmGameOver()
        {
            if (confirmed || GameManager.GameState != GameState.GameOver) return;
            confirmed = true;
            AudioManager.Instance.PlaySfx(gameOverSFX);
            playerInput.DisableAllInput();
            SceneLoader.Instance.LoadScoreScene();
        }
    }
}
