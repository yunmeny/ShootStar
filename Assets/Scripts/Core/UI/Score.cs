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
            ScoreManager.Instance.ResetScore();
        }
    
        public static void UpdateScoreText(int score) => scoreText.text = score.ToString();
    
    
        public static void ScaleText(Vector3 target)=> scoreText.transform.localScale = target; 
    }
}