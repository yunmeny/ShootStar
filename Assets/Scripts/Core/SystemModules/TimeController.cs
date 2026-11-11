using System.Collections;
using UnityEngine;

namespace Core.SystemModules
{
    public class TimeController : Singleton<TimeController>
    {
        [SerializeField, Range(0, 2)] private float bulletTimeScale = 0.1f;

    
        private float _defaultDeltaFixedTime;
        private float _t;
    
        private float _timeScaleBeforePause;
        protected override void Awake()
        {
            base.Awake();
            _defaultDeltaFixedTime = Time.fixedDeltaTime;
        }
    
        public void ResetTime()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _defaultDeltaFixedTime;
            _timeScaleBeforePause = 1f;
        }

        private void OnDestroy() => ResetTime();

        public void BeginUpgradeSelection()
        {
            // Discard pending slow-motion routines so they cannot unfreeze the refit modal.
            ResetTime();
            GameManager.GameState = GameState.UpgradeSelection;
            Time.timeScale = 0f;
        }

        public void EndUpgradeSelection()
        {
            if (GameManager.GameState != GameState.UpgradeSelection) return;
            ResetTime();
            GameManager.GameState = GameState.Play;
        }

        public void Pause()
        {
            if (GameManager.GameState != GameState.Play) return;
            _timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0;
            GameManager.GameState = GameState.Pause;
        }

        public void UnPause()
        {
            if (GameManager.GameState != GameState.Pause) return;
            Time.timeScale = _timeScaleBeforePause;
            GameManager.GameState = GameState.Play;
        }
    
        public void BulletTime(float duration)
        {
            StartCoroutine(SlowOutCoroutine(duration));
        }
    
        public void BulletTime(float inDuration, float outDuration)
        {
            StartCoroutine(SlowInOutCoroutine(inDuration, outDuration));
        }
    
        public void BulletTime(float inDuration, float keepTime, float outDuration)
        {
            StartCoroutine(SlowInKeepOutCoroutine(inDuration, keepTime, outDuration));
        }

        public void SlowIn(float duration)
        {
            StartCoroutine(SlowInCoroutine(duration));
        }

        public void SlowOut(float duration)
        {
            StartCoroutine(SlowOutCoroutine(duration));
        }
    
        IEnumerator SlowInOutCoroutine(float inDuration, float outDuration)
        {
            yield return SlowInCoroutine(inDuration);
            StartCoroutine(SlowOutCoroutine(outDuration));
        }

        IEnumerator SlowInKeepOutCoroutine(float inDuration, float keepTime, float outDuration)
        {
            yield return SlowInCoroutine(inDuration);
            yield return new WaitForSecondsRealtime(keepTime);
            StartCoroutine(SlowOutCoroutine(outDuration));
        }
    
        IEnumerator SlowInCoroutine(float duration)
        {
            _t = 0;
            while (_t < 1)
            {
                if (GameManager.GameState == GameState.Play)
                {
                    // 使用帧间值确保不会受到TimeScale的影响
                    _t += Time.unscaledDeltaTime / duration;
                    Time.timeScale = Mathf.Lerp(1, bulletTimeScale, _t);
                    Time.fixedDeltaTime = _defaultDeltaFixedTime * Time.timeScale;
                }
                yield return null;
            }
        }
    
        IEnumerator SlowOutCoroutine(float duration)
        {
            _t = 0;
            while (_t < 1)
            {
                if (GameManager.GameState == GameState.Play && Time.timeScale != 0)
                {
                    _t += Time.unscaledDeltaTime / duration;
                    Time.timeScale = Mathf.Lerp(bulletTimeScale, 1, _t);
                    Time.fixedDeltaTime = _defaultDeltaFixedTime * Time.timeScale;
                }
                yield return null;
            }

        }
    }
}
