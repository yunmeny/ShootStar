using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTraceProjectile : PlayerProjectile
{
    [SerializeField] private ProjectileGuidanceSystem guidanceSystem;
    
    protected override void OnEnable()
    {
        SetTarget(EenemyManager.Instance.GetRandomEnemy);
        transform.rotation = Quaternion.identity;
        if (Target == null)
        {
            base.OnEnable();
        }
        else
        {
            // base.OnEnable();
            StartCoroutine(guidanceSystem.HomingCoroutine(Target));
        }
    }
}
