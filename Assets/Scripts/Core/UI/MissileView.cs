using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.UI
{
    public class MissileView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image coolDown;
    
        public void UpdateAmountText(int score) => amountText.text = score.ToString();
    
        public void UpdateCooldown(float cooldown) => coolDown.fillAmount = cooldown;
    }
}
