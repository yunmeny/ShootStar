using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class StateBar : MonoBehaviour
    {
        private static readonly int FrontProgress = Shader.PropertyToID("_FrontProgress");
        private static readonly int BufferProgress = Shader.PropertyToID("_BufferProgress");

        [SerializeField] private float fillSpeed = 0.1f;
        [SerializeField] private float fillDelay = 0.5f;
        [SerializeField] bool delayFill = true;

        [SerializeField] protected new RawImage renderer;

        private Material MainMaterial;
        private Canvas _canvas;
        private Transform _followTarget;
        private Vector3 _worldOffset;
        private float _previousFillAmount;
        private float _currentFillAmount;
        protected float TargetFillAmount;
        private Coroutine _bufferFillCoroutine;
        WaitForSeconds _waitForDelayFill;
        private float _t;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas != null && _canvas.worldCamera == null)
            {
                _canvas.worldCamera = Camera.main;
            }

            if (_canvas != null && _canvas.renderMode == RenderMode.WorldSpace)
            {
                _followTarget = transform.parent;
                if (_followTarget != null)
                    _worldOffset = transform.position - _followTarget.position;
            }

            _waitForDelayFill = new WaitForSeconds(fillDelay);
        }

        private void LateUpdate()
        {
            if (_canvas == null || _canvas.renderMode != RenderMode.WorldSpace) return;

            // Follow movement without inheriting the aircraft's banking or dodge roll.
            if (_followTarget != null)
                transform.position = _followTarget.position + _worldOffset;

            var camera = _canvas.worldCamera;
            transform.rotation = camera != null ? camera.transform.rotation : Quaternion.identity;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public virtual void Initialize(float value, float maxValue)
        {
            MainMaterial = new(renderer.material);
            renderer.material = MainMaterial;
            _currentFillAmount = value / maxValue;
            TargetFillAmount = _currentFillAmount;
            MainMaterial.SetFloat(BufferProgress, _currentFillAmount);
            MainMaterial.SetFloat(FrontProgress, _currentFillAmount);

            UpdateState(value, maxValue);
        }

        /// <summary>
        /// 更新状态条的显示。
        /// </summary>
        /// <param name="currentValue">
        /// 当前值。
        /// </param>
        /// <param name="maxValue">
        /// 最大值。
        /// </param>
        public virtual void UpdateState(float currentValue, float maxValue)
        {
            if (_bufferFillCoroutine != null)
            {
                StopCoroutine(_bufferFillCoroutine);
            }

            TargetFillAmount = currentValue / maxValue; //计算目标填充量
            if (_currentFillAmount > TargetFillAmount)
            {
                MainMaterial.SetFloat(FrontProgress, TargetFillAmount);
                _bufferFillCoroutine = StartCoroutine(BufferedFillingCoroutine(false));
                return;
            }

            if (_currentFillAmount < TargetFillAmount)
            {
                MainMaterial.SetFloat(BufferProgress, TargetFillAmount);
                _bufferFillCoroutine = StartCoroutine(BufferedFillingCoroutine(true));
            }
        }

        protected virtual IEnumerator BufferedFillingCoroutine(bool isFront)
        {
            if (delayFill)
            {
                yield return _waitForDelayFill;
            }

            _t = 0;
            _previousFillAmount = _currentFillAmount;
            while (_t < 1)
            {
                _t += Time.deltaTime * fillSpeed;
                _currentFillAmount = Mathf.Lerp(_previousFillAmount, TargetFillAmount, _t);
                MainMaterial.SetFloat(isFront ? FrontProgress : BufferProgress, _currentFillAmount);
                yield return null;
            }
        }
    }
}