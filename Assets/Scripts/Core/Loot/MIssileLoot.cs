using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIssileLoot : Loot
{
    [SerializeField] private int missileBonus = 1;
    private MissileSystem missileSystem;

    protected override void Awake()
    {
        base.Awake();
        missileSystem = FindObjectOfType<MissileSystem>();
    }

    protected override void PickUp()
    {
        base.PickUp();
        missileSystem.AddMissile(missileBonus);
    }
}