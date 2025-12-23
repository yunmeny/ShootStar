using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBeam : MonoBehaviour
{
    [SerializeField] private float damage = 50f ;
    [SerializeField] private GameObject hitVFX;
    
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        // 如果碰撞的物体有Character组件，则调用TakeDamage方法。 TryGetComponent()方法会尝试获取指定组件，如果获取成功，则返回true，否则返回false。
        if (!collision.gameObject.TryGetComponent<Player>(out var player)) return;
        player.TakeDamage(damage);
        PoolManger.Release(hitVFX, collision.GetContact(0).point,
            Quaternion.LookRotation(collision.GetContact(0).normal)); // 法线方向
    }
    
}
   