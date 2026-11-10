using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Core.UI
{
    public class MissileView : MonoBehaviour
    {
        [SerializeField] private Text amountText;
        [SerializeField] private Image coolDowm;
    
        public void UpdateAmountText(int score) => amountText.text = score.ToString();
    
        public void UpdateCooldown(float cooldown) => coolDowm.fillAmount = cooldown;
    }
}
