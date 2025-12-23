using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WaveUI : MonoBehaviour
{
    Text waveText;

    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
        waveText = GetComponentInChildren<Text>();
    }

    void OnEnable()
    {
        waveText.text = "- WAVE " + EenemyManager.Instance.WaveNumber + " -";
    }
}