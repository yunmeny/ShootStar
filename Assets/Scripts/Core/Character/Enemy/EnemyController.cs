using System;
using System.Collections;
using System.Collections.Generic;
using Core.SystemModules;
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
    private EncounterSpawn encounterSpawn;
    private EenemyManager encounterDirector;
    protected virtual void Awake()
    {
        _waitForFixedUpdate = new WaitForFixedUpdate();
        paddingX = transform.GetChild(0).GetComponent<Renderer>().bounds.size.x / 2;
        paddingY = transform.GetChild(0).GetComponent<Renderer>().bounds.size.y / 2; 
    }
    
    protected virtual void OnEnable() 
    {
        if (encounterSpawn != null && encounterDirector)
        {
            StartCoroutine(EncounterCoroutine());
            return;
        }
        StartCoroutine(nameof(RandomMoveCoroutine));
        StartCoroutine(nameof(RandomFireCoroutine));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        encounterSpawn = null;
        encounterDirector = null;
        if (fireVfx) fireVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        transform.rotation = Quaternion.identity;
    }

    // Called while pooled objects are inactive, before OnEnable can start random behaviour.
    public void ConfigureEncounter(EncounterSpawn spawn, EenemyManager director)
    {
        encounterSpawn = spawn;
        encounterDirector = director;
    }

    private Vector3 EncounterPosition(EncounterMotionSample sample)
    {
        var view = Viewport.Instance;
        return new Vector3(
            Mathf.LerpUnclamped(view.MinX, view.MaxX, sample.X),
            Mathf.LerpUnclamped(view.MinY, view.MaxY, sample.Y));
    }

    private IEnumerator EncounterCoroutine()
    {
        var spawn = encounterSpawn;
        transform.SetPositionAndRotation(EncounterPosition(EncounterMotion.Sample(spawn, encounterDirector.EncounterElapsed)), Quaternion.identity);
        float nextFire = spawn.FireAt;
        while (gameObject.activeSelf && encounterDirector && GameManager.GameState != GameState.GameOver)
        {
            if (GameManager.GameState != GameState.Play) { yield return null; continue; }
            float now = encounterDirector.EncounterElapsed;
            var motion = EncounterMotion.Sample(spawn, now);
            transform.position = EncounterPosition(motion);
            if (motion.Stage == EncounterMotionStage.Complete)
            {
                // Escape completes the encounter slot without score, loot or kill energy.
                encounterDirector.RemoveEscapedEnemy(gameObject);
                gameObject.SetActive(false);
                yield break;
            }
            if (now >= nextFire && motion.CanFire)
            {
                int volley = Mathf.Min(projectiles.Length, EncounterChapter.MaxVolleyProjectiles);
                if (encounterDirector.TryEncounterVolley(volley))
                {
                    for (int i = 0; i < volley; i++)
                        encounterDirector.TrackEncounterProjectile(PoolManger.Release(projectiles[i], muzzle.position));
                    if (fireVfx) fireVfx.Play();
                    AudioManager.Instance.PlayRanSfx(projectileLunchSFX);
                }
                nextFire += spawn.FireEvery;
            }
            yield return null;
        }
    }

    // Test chapter presents aligned pairs so piercing and side shots have readable targets.
    public void SetBuildTestFormation(Vector3 slot)
    {
        StopCoroutine(nameof(RandomMoveCoroutine));
        transform.position = new Vector3(Viewport.Instance.MaxX + paddingX, slot.y);
        transform.rotation = Quaternion.identity;
        StartCoroutine(MoveToFormation(slot));
    }

    private IEnumerator MoveToFormation(Vector3 slot)
    {
        while (Vector3.Distance(transform.position, slot) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, slot, moveSpeed * Time.deltaTime);
            yield return null;
        }
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
            AudioManager.Instance.PlayRanSfx(projectileLunchSFX);
        }
    }
}
