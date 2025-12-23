using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StateBar_HUD : StateBar
{
    [SerializeField] protected Text percentText;

    protected virtual void SetPercentText()
    {
        // percentText.text = Mathf.RoundToInt(targetFillAmount * 100f) + "%";
        percentText.text = targetFillAmount.ToString("P0");
    }

    public override void Initialize(float value, float maxValue)
    {
        base.Initialize(value, maxValue);
        SetPercentText();
    }

    protected override IEnumerator BufferedFillingCoroutine(Image image)
    {
        SetPercentText();
        return base.BufferedFillingCoroutine(image);
    }
}
