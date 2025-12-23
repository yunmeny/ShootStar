using UnityEngine;
using UnityEngine.UI;
public class ShieldLoot : Loot
{
    [SerializeField] private int shieldBonus = 20;
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
        if (Player.IsMaxHealth)
        {
            _displayText.text = $"Score + {scoreBonus}";
            ScoreManager.Instance.AddScore(scoreBonus);
        }
        else
        {
            _displayText.text = $"Shield + {shieldBonus}";
            Player.RestoreHealth(shieldBonus);
        }
    }
}
