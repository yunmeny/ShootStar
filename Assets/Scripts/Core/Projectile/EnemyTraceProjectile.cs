using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyTraceProjectile : Projectile
{
    private void Awake()
    {
    }

    protected override void OnEnable()
    {
        
        SetTarget(GameObject.FindGameObjectWithTag("Player")); // 开销较大，可以考虑使用事件系统？？

        base.OnEnable();
        StartCoroutine(nameof(MoveDirectionCoroutine));
    }

    IEnumerator MoveDirectionCoroutine()
    {
        yield return null;
        if (Target.activeSelf)
        {
            moveDirection = (Target.transform.position - transform.position).normalized;
        }
    }
}