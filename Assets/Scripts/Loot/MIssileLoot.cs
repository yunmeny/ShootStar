using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIssileLoot : Loot
{
    [SerializeField] private int missileBonus = 1;
    
    protected override void PickUp()
    {
        base.PickUp();
        MissileSystem.AddMissile(missileBonus);
    }
}