using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    static Text scoreText;

    private void Awake()
    {
        scoreText = GetComponent<Text>();
    }

    private void Start()
    {
        ScoreManager.Instance.ResetScore();
    }
    
    public static void UpdateScoreText(int score) => scoreText.text = score.ToString();
    
    
    public static void ScaleText(Vector3 target)=> scoreText.transform.localScale = target; 
}