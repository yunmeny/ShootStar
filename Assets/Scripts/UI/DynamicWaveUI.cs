using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DynamicWaveUI : MonoBehaviour
{
    [Header("--------Line--------")] 
    [SerializeField] private Vector2 lineTopStartPosition = new Vector2(-1250f, 140f);
    [SerializeField] private Vector2 lineTopEndPosition = new Vector2(0f, 140f);
    [SerializeField] private Vector2 lineBottomStartPosition = new Vector2(1250f, -10f);
    [SerializeField] private Vector2 lineBottomEndPosition = new Vector2(0f, -10f);

    [Header("--------Text--------")] 
    [SerializeField] private Vector2 waveTextStartScale = new Vector2(1f, 0f);
    [SerializeField] private Vector2 waveTextEndScale = Vector2.one;

    [SerializeField] private float showDuration = 0.5f;
    private RectTransform _lineTop;
    private RectTransform _lineBottom;
    private RectTransform _waveText;
    WaitForSeconds _waitForSeconds;

    private void Awake()
    {
        if (TryGetComponent<Animator>(out Animator animator))
        {
            if (animator.isActiveAndEnabled)
            {
                Destroy(this);
            }
        }

        if (showDuration * 2 >= EenemyManager.Instance.TimeBetweenWave)
        {
#if UNITY_EDITOR
            Debug.LogError("动画时间 showDuration*2 必须小于 EnemyManager 中的 TimeBetweenWave");
#endif
            showDuration = EenemyManager.Instance.TimeBetweenWave / 3;
        }

        _waitForSeconds = new WaitForSeconds(EenemyManager.Instance.TimeBetweenWave - showDuration);
        _lineTop = transform.Find("Line Top").GetComponent<RectTransform>();
        _lineBottom = transform.Find("Line Bottom").GetComponent<RectTransform>();
        _waveText = transform.Find("Wave Text").GetComponent<RectTransform>();
        
        _lineTop.anchoredPosition = lineTopStartPosition;
        _lineBottom.anchoredPosition = lineBottomStartPosition;
        _waveText.localScale = waveTextStartScale;
    }

    private void OnEnable()
    {
        // StartCoroutine(nameof(ShowWaveUICoroutine));
        
        StartCoroutine(ShowWaveCoroutine(_lineTop, lineTopStartPosition, lineTopEndPosition));
        StartCoroutine(ShowWaveCoroutine(_lineBottom, lineBottomStartPosition, lineBottomEndPosition));
        StartCoroutine(TextScaleCoroutine(_waveText, waveTextStartScale, waveTextEndScale));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator ShowWaveUICoroutine()
    {
        var timer = 0f;
        while (timer < showDuration)
        {
            timer += Time.deltaTime;
            _lineTop.localPosition =
                Vector2.MoveTowards(_lineTop.localPosition, lineTopEndPosition, 
                    (lineTopEndPosition-lineTopStartPosition).magnitude*Time.deltaTime/showDuration);
            _lineBottom.localPosition =
                Vector2.MoveTowards(_lineBottom.localPosition, lineBottomEndPosition, 
                    (lineBottomEndPosition-lineBottomStartPosition).magnitude*Time.deltaTime/showDuration);
            _waveText.localScale = Vector2.MoveTowards(_waveText.localScale, waveTextEndScale, 
                (waveTextEndScale-waveTextStartScale).magnitude*Time.deltaTime/showDuration);
            yield return null;
        }
        yield return new WaitForSeconds(EenemyManager.Instance.TimeBetweenWave-showDuration*2);
        timer = 0f;
        while (timer < showDuration)
        {
            timer += Time.deltaTime;
            _lineTop.localPosition =
                Vector2.MoveTowards(_lineTop.localPosition, lineTopStartPosition, 
                    (lineTopEndPosition-lineTopStartPosition).magnitude*Time.deltaTime/showDuration);
            _lineBottom.localPosition =
                Vector2.MoveTowards(_lineBottom.localPosition, lineBottomStartPosition, 
                    (lineBottomEndPosition-lineBottomStartPosition).magnitude*Time.deltaTime/showDuration);
            _waveText.localScale = Vector2.MoveTowards(_waveText.localScale, waveTextStartScale, 
                (waveTextEndScale-waveTextStartScale).magnitude*Time.deltaTime/showDuration);
            yield return null;
        }
        gameObject.SetActive(false);
    }

    IEnumerator ShowWaveCoroutine(RectTransform rect, Vector2 start, Vector2 end)
    {
        StartCoroutine(UIMoveCoroutine(rect, end, start));
        yield return _waitForSeconds; 
        StartCoroutine(UIMoveCoroutine(rect, start, end));

    }

    IEnumerator UIMoveCoroutine(RectTransform rect, Vector2 target ,Vector2 begin)
    {
        var timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / showDuration;
            rect.localPosition = Vector2.Lerp(begin, target, timer);
            yield return null;
        }

    }

    IEnumerator TextScaleCoroutine(RectTransform rect, Vector2 start , Vector2 end)
    {
        StartCoroutine(UIScaleCoroutine(rect, end, start));
        yield return _waitForSeconds;
        StartCoroutine(UIScaleCoroutine(rect, start, end));
    }

    IEnumerator UIScaleCoroutine(RectTransform rect, Vector2 target, Vector2 begin)
    {
        var timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / showDuration;
            rect.localScale = Vector2.Lerp(begin, target, timer);
            yield return null;
        }
    }
}