using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MissileDisply : MonoBehaviour
{
    static Text amountText;
    static Image cooldowm;

    private void Awake()
    {
        amountText = transform.Find("Amount Text").GetComponent<Text>();
        cooldowm = transform.Find("Cooldown Image").GetComponent<Image>();
    }
    
    public static void UpdateAmountText(int score) => amountText.text = score.ToString();
    
    public static void UpdateCooldown(float cooldown) => cooldowm.fillAmount = cooldown;
}
