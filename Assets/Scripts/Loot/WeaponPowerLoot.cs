using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPowerLoot : Loot
{
    [SerializeField] private float duration = 5f;
    [SerializeField] WeaponPowerBonusType weaponPowerBonusType = WeaponPowerBonusType.WeaponTempAdd;
    [SerializeField] int scoreBonus = 200;
    private Text _displayText;
    
    private int _weaponPowerBonus;
    private WaitForSeconds _waitForWeaponPowerBonusDuration;

    protected override void Awake()
    {
        base.Awake();
        _displayText = GetComponentInChildren<Text>(true);
        _waitForWeaponPowerBonusDuration = new WaitForSeconds(duration);
    }

    protected override void PickUp()
    {
        switch (weaponPowerBonusType)
        {
            case WeaponPowerBonusType.WeaponTempAdd:
                Player.AddWeaponPower(_waitForWeaponPowerBonusDuration);
                break;
            case WeaponPowerBonusType.WeaponRandomAdd:
                Player.RandomWeaponPower();
                break;
            case WeaponPowerBonusType.WeaponPersistAdd:
                if (Player.weaponPower == Player.MaxWeaponPower)
                {
                    _displayText.text = $"Score + {scoreBonus}";
                    ScoreManager.Instance.AddScore(scoreBonus);
                }
                else
                {
                    _displayText.text = "Weapon Power UP!";
                }
                Player.PersistWeaponPower();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        base.PickUp();
    }


}

public enum WeaponPowerBonusType
{
    WeaponTempAdd,
    WeaponRandomAdd,
    WeaponPersistAdd,
}