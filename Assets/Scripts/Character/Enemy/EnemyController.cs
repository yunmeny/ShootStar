using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("--------移动--------")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float moveRotationAngle = 25f;
    protected float paddingX;
    protected float paddingY;
    
    [Header("--------攻击--------")]
    [SerializeField] protected GameObject[] projectiles;
    [SerializeField] protected AudioData[] projectileLunchSFX;
    [SerializeField] protected Transform muzzle;
    [SerializeField] protected float minFireInterval = 0.2f;
    [SerializeField] protected float maxFireInterval = 2f;
    [SerializeField] protected ParticleSystem fireVfx;
    
    protected WaitForFixedUpdate _waitForFixedUpdate;
    private int count;
    protected virtual void Awake()
    {
        _waitForFixedUpdate = new WaitForFixedUpdate();
        paddingX = transform.GetChild(0).GetComponent<Renderer>().bounds.size.x / 2;
        paddingY = transform.GetChild(0).GetComponent<Renderer>().bounds.size.y / 2; 
    }
    
    protected virtual void OnEnable() 
    {
        StartCoroutine(nameof(RandomMoveCoroutine));
        StartCoroutine(nameof(RandomFireCoroutine));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    protected virtual IEnumerator RandomMoveCoroutine()
    {
        transform.position = Viewport.Instance.RandomEnemySpawnPosition(paddingX, paddingY);
        var targetPosition = Viewport.Instance.RandomRightHalfPosition(paddingX, paddingY);
        while (gameObject.activeSelf)
        {
            if (Vector3.Distance(transform.position, targetPosition) > moveSpeed * Time.fixedDeltaTime)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
                transform.rotation = Quaternion.AngleAxis((targetPosition-transform.position).normalized.y*moveRotationAngle,Vector3.right);    //将敌人移动的方向的y轴作为旋转角度
                
            }
            else
            {
                targetPosition = Viewport.Instance.RandomRightHalfPosition(paddingX, paddingY);
            }
            
            yield return _waitForFixedUpdate;
        }
    }

    protected virtual IEnumerator RandomFireCoroutine()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(minFireInterval, maxFireInterval));

            if (GameManager.GameState == GameState.GameOver)
            {
                yield break;
            }
            foreach (var projectile in projectiles)
            {
                PoolManger.Release(projectile, muzzle.position);
            }
            fireVfx.Play();
            AudioManager.Instance.PlayRandomSFX(projectileLunchSFX);
        }
    }
}