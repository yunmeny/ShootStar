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
        waveText.text = "- WAVE " + EenemyManager.Instance.WaveNumber + " -";
    }
}