using System.Collections;
using System.Collections.Generic;
using Core.SystemModules;
using UnityEngine;

public class PlayerTraceProjectile : PlayerProjectile
{
    [SerializeField] private ProjectileGuidanceSystem guidanceSystem;
    
    protected override void OnEnable()
    {
        InitializePlayerShot();
        if (IsFragment)
        {
            StartMovingDirectly();
            return;
        }
        SetTarget(EenemyManager.Instance.GetRandomEnemy);
        transform.rotation = Quaternion.identity;
        if (Target == null)
        {
            StartMovingDirectly();
        }
        else
        {
            // base.OnEnable();
            StartCoroutine(guidanceSystem.HomingCoroutine(Target));
        }
    }

    protected override void OnPiercedTarget()
    {
        // Overdrive rounds continue along their impact heading instead of orbiting the ignored target.
        StopAllCoroutines();
        SetTarget(null);
        StartMovingDirectly();
    }
}
