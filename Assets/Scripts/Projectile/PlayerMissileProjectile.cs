using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissileProjectile : PlayerTraceProjectile
{
    [SerializeField] private float startSpeed = 8f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float delayTime = 0.5f;
    [SerializeField] private AudioData missileLunchSFX;
    [SerializeField] private LayerMask enemyLayer = default;
    [SerializeField] private AudioData explodeSFX;
    [SerializeField] private GameObject explodeVFX;
    [SerializeField] private float explodeRadius = 1f;
    [SerializeField] private float explodeDamage = 25f;
    
    WaitForSeconds _waitForSeconds;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(nameof(Accelerate));
        _waitForSeconds = new WaitForSeconds(delayTime);
        
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        PoolManger.Release(explodeVFX, transform.position);
        AudioManager.Instance.PlaySFX(explodeSFX);
        
        var collider2Ds = Physics2D.OverlapCircleAll(transform.position, explodeRadius, enemyLayer);
        foreach (var collider in collider2Ds)
        {
            if (collider.TryGetComponent<Character>(out Character character))
            {
                character.TakeDamage(explodeDamage);
            }
        }
    }

    IEnumerator Accelerate()
    {
        moveSpeed = startSpeed;
        if (Target != null)
        {
            AudioManager.Instance.PlaySFX(missileLunchSFX);
        }
        
        yield return _waitForSeconds;
        
        moveSpeed = maxSpeed;

    }
}
