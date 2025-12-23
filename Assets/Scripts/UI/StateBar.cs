using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateBar : MonoBehaviour
{
    [SerializeField] protected Image fillImageBack;
    [SerializeField] protected Image fillImageFront;
    [SerializeField] private float fillSpeed = 0.1f;
    [SerializeField] private float fillDelay = 0.5f;
    [SerializeField] bool delayFill = true;
    
    private float _previousFillAmount;
    private Canvas _canvas;
    private float _currentFillAmount;
    protected float targetFillAmount;
    private Coroutine _bufferFillCoroutine;
    WaitForSeconds _waitForDelayFill;
    private float _t;

    private void Awake()
    {
        if (TryGetComponent<Canvas>(out Canvas canvas))
        {
            canvas.worldCamera = Camera.main;
        }
        
        _waitForDelayFill = new WaitForSeconds(fillDelay);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public virtual void Initialize(float value, float maxValue)
    {
        _currentFillAmount = value / maxValue;
        targetFillAmount = _currentFillAmount;
        fillImageBack.fillAmount = _currentFillAmount;
        fillImageFront.fillAmount = _currentFillAmount;
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
        targetFillAmount = currentValue / maxValue; //计算目标填充量
        if (_currentFillAmount > targetFillAmount)
        {
            fillImageFront.fillAmount = targetFillAmount;
            _bufferFillCoroutine = StartCoroutine(BufferedFillingCoroutine(fillImageBack));
            return;
        }

        if (_currentFillAmount < targetFillAmount)
        {
            fillImageBack.fillAmount = targetFillAmount;
            _bufferFillCoroutine = StartCoroutine(BufferedFillingCoroutine(fillImageFront));
        }
    }

    protected virtual IEnumerator BufferedFillingCoroutine(Image image)
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
            _currentFillAmount = Mathf.Lerp(_previousFillAmount, targetFillAmount, _t);
            image.fillAmount = _currentFillAmount;
            yield return null;
        }
    }
}