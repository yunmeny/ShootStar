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
    private LineRenderer attackWarning;
    private Material warningMaterial;
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
        if (attackWarning) attackWarning.enabled = false;
        if (fireVfx) fireVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        transform.rotation = Quaternion.identity;
    }

    // Called while pooled objects are inactive, before OnEnable can start random behaviour.
    public void ConfigureEncounter(EncounterSpawn spawn, EenemyManager director)
    {
        encounterSpawn = spawn;
        encounterDirector = director;
    }

    private Vector3 EncounterPosition(float x, float y)
    {
        var view = Viewport.Instance;
        return new Vector3(
            Mathf.Clamp(Mathf.Lerp(view.MiddleX, view.MaxX, x), view.MiddleX + paddingX + 0.5f, view.MaxX - paddingX),
            Mathf.Clamp(Mathf.Lerp(view.MinY, view.MaxY, y), view.MinY + paddingY, view.MaxY - paddingY));
    }

    private IEnumerator EncounterCoroutine()
    {
        var spawn = encounterSpawn;
        var slot = EncounterPosition(spawn.X, spawn.Y);
        var spread = EncounterPosition(spawn.SpreadX, spawn.SpreadY);
        var entry = new Vector3(Viewport.Instance.MaxX + paddingX, slot.y);
        transform.SetPositionAndRotation(entry, Quaternion.identity);
        float nextFire = spawn.FireAt;
        EnsureAttackWarning();
        while (gameObject.activeSelf && encounterDirector && GameManager.GameState != GameState.GameOver)
        {
            if (GameManager.GameState != GameState.Play) { yield return null; continue; }
            float now = encounterDirector.EncounterElapsed;
            float entryProgress = Mathf.Clamp01((now - spawn.SpawnAt) / EncounterChapter.EntryDuration);
            transform.position = entryProgress < 1f ? Vector3.Lerp(entry, slot, Mathf.SmoothStep(0f, 1f, entryProgress)) :
                Vector3.Lerp(slot, spread, Mathf.Clamp01((now - EncounterChapter.ClusterSpreadAt) / EncounterChapter.ClusterSpreadDuration));
            bool warning = entryProgress >= 1f && now >= nextFire - EncounterChapter.WarningDuration;
            attackWarning.enabled = warning;
            if (warning)
            {
                // A growing line and arrow shape communicate wind-up without relying on colour.
                var origin = muzzle.position;
                float progress = Mathf.Clamp01(1f - (nextFire - now) / EncounterChapter.WarningDuration);
                attackWarning.SetPosition(0, origin + new Vector3(-0.5f, 0.25f));
                attackWarning.SetPosition(1, origin + new Vector3(-0.8f, 0f));
                attackWarning.SetPosition(2, origin + new Vector3(-0.5f, -0.25f));
                attackWarning.SetPosition(3, origin + new Vector3(-0.8f, 0f));
                attackWarning.SetPosition(4, origin + Vector3.left * (0.8f + progress * 1.2f));
            }
            if (now >= nextFire && entryProgress >= 1f)
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
                attackWarning.enabled = false;
            }
            yield return null;
        }
        if (attackWarning) attackWarning.enabled = false;
    }

    private void EnsureAttackWarning()
    {
        if (attackWarning) return;
        var root = new GameObject("AttackWarning");
        root.transform.SetParent(transform, false);
        attackWarning = root.AddComponent<LineRenderer>();
        warningMaterial = new Material(Shader.Find("Sprites/Default"));
        attackWarning.sharedMaterial = warningMaterial;
        attackWarning.useWorldSpace = true;
        attackWarning.positionCount = 5;
        attackWarning.startWidth = attackWarning.endWidth = 0.06f;
        attackWarning.startColor = attackWarning.endColor = new Color(1f, 0.85f, 0.45f);
        attackWarning.sortingOrder = 20;
        attackWarning.enabled = false;
    }

    private void OnDestroy()
    {
        if (warningMaterial) Destroy(warningMaterial);
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
