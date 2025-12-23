using UnityEngine;
using UnityEngine.UI;
public class ScoreBouns : Loot
{
    [SerializeField] private int scoreBonus = 200;
    
    private Text _displayText;

    protected override void Awake()
    {
        base.Awake();
        _displayText = GetComponentInChildren<Text>(true);
    }
    
    protected override void PickUp()
    {
        base.PickUp();
        _displayText.text = $"score + {scoreBonus}";
        ScoreManager.Instance.AddScore(scoreBonus);
    }
}
