using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private float damage = 1f;
    [SerializeField] protected float moveSpeed = 10f;
    [SerializeField] protected Vector2 moveDirection;
    [SerializeField] private AudioData[] hitSFX;
    protected GameObject Target;
    protected virtual float DamageMultiplier => 1f;
    protected virtual float HitDamage => damage * DamageMultiplier;

    protected virtual void OnEnable()
    {
        StartMovingDirectly();
    }

    protected void StartMovingDirectly() => StartCoroutine(nameof(MoveDirectly));

    IEnumerator MoveDirectly()
    {
        while (gameObject.activeSelf)
        {
            Move();
            yield return null;
        }
    }
    
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        // 如果碰撞的物体有Character组件，则调用TakeDamage方法。 TryGetComponent()方法会尝试获取指定组件，如果获取成功，则返回true，否则返回false。
        if (collision.gameObject.TryGetComponent<Character>(out Character character))
        {
            ApplyHit(character, collision);
            gameObject.SetActive(false);
        }
    }

    protected void ApplyHit(Character character, Collision2D collision)
    {
        character.TakeDamage(HitDamage);
        AudioManager.Instance.PlayRanSfx(hitSFX);
        PoolManger.Release(hitVFX, collision.GetContact(0).point,
            Quaternion.LookRotation(collision.GetContact(0).normal));
    }

    protected void SetTarget(GameObject target) => Target = target;
    
    public virtual void Move() => transform.Translate(Time.deltaTime * moveSpeed * moveDirection);
    
}
