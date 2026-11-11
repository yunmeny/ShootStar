    using System;
using Core.SystemModules;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    Text waveText;

    private void Awake()
    {
        waveText = GetComponentInChildren<Text>();
    }

    void OnEnable()
    {
        int wave = EenemyManager.Instance.WaveNumber;
        waveText.text = GameManager.Instance.IsBuildTest ?
            (BuildTestChapter.IsBoss(wave) ? "- BUILD TEST / FINAL BOSS -" : "- BUILD TEST " + wave + " / 6 -") :
            "- WAVE " + wave + " -";
    }
}
