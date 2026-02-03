using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class StateBar_HUD : StateBar
    {
        [SerializeField] protected TextMeshProUGUI percentText;

        protected virtual void SetPercentText()
        {
            // percentText.text = Mathf.RoundToInt(targetFillAmount * 100f) + "%";
            percentText.text = TargetFillAmount.ToString("P0");
        }

        public override void Initialize(float value, float maxValue)
        {
            base.Initialize(value, maxValue);
            SetPercentText();
        }

        protected override IEnumerator BufferedFillingCoroutine(bool isFront)
        {
            SetPercentText();
            return base.BufferedFillingCoroutine(isFront);
        }
    }
}
