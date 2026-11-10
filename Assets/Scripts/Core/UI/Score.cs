using Core.SystemModules;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class Score : MonoBehaviour
    {
        static Text scoreText;

        private void Awake()
        {
            scoreText = GetComponent<Text>();
        }

        private void Start()
        {
            UpdateScoreText(ScoreManager.Instance.ScoreAmount);
        }
    
        public static void UpdateScoreText(int score)
        {
            if (scoreText) scoreText.text = score.ToString();
        }
    
    
        public static void ScaleText(Vector3 target)
        {
            if (scoreText) scoreText.transform.localScale = target;
        }
    }
}