using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.SystemModules;

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
    private readonly MissileBlastState blast = new MissileBlastState();
    private bool chainEnabled;
    private bool reclaimerEnabled;
    private float shotRadius;
    private float shotBlastDamage;
    private readonly Collider2D[] overlapResults = new Collider2D[64];
    private readonly List<Vector3> secondaryCenters = new List<Vector3>(CombatBuildState.MaxSecondaryBlasts);
    
    protected override void OnEnable()
    {
        blast.Reset();
        secondaryCenters.Clear();
        moveSpeed = startSpeed;
        _waitForSeconds = new WaitForSeconds(delayTime);
        base.OnEnable();
        var run = RunUpgradeController.Instance ? RunUpgradeController.Instance.Run : null;
        shotRadius = explodeRadius * (run?.MissileRadiusMultiplier ?? 1f);
        shotBlastDamage = explodeDamage * DamageMultiplier;
        chainEnabled = run?.HasChainDetonation ?? false;
        reclaimerEnabled = run?.HasReclaimer ?? false;
        StartCoroutine(nameof(Accelerate));
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameObject.activeSelf) return;
        var firstEnemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (!firstEnemy || firstEnemy.GetHp <= 0f) return;
        if (!blast.TryDetonate()) return;
        Vector3 position = transform.position;
        Vector3 enemyPosition = firstEnemy.transform.position;
        base.OnCollisionEnter2D(collision);
        if (firstEnemy.GetHp <= 0f || !firstEnemy.gameObject.activeSelf)
        {
            if (blast.RecordKill(chainEnabled)) secondaryCenters.Add(enemyPosition);
        }
        PoolManger.Release(explodeVFX, position);
        AudioManager.Instance.PlaySfx(explodeSFX);
        DamageArea(position, shotRadius, shotBlastDamage, chainEnabled);
        // Only primary/direct kills seed secondary blasts. Their kills cannot recurse.
        int secondaryCount = secondaryCenters.Count;
        for (int i = 0; i < secondaryCount; i++)
        {
            PoolManger.Release(explodeVFX, secondaryCenters[i]);
            DamageArea(secondaryCenters[i], shotRadius * 0.75f, shotBlastDamage * 0.6f, false);
        }
        var owner = RunUpgradeController.Instance;
        if (owner)
        {
            int refund = reclaimerEnabled ? owner.Combat.RecordMissileKills(blast.Kills) : 0;
            if (refund > 0) FindFirstObjectByType<MissileSystem>()?.AddMissile(refund);
            owner.ReportCombatEvent("MISSILE / CHAIN " + secondaryCount + " / KILLS " + blast.Kills + (refund > 0 ? " / +1 AMMO" : ""));
        }
    }

    private void DamageArea(Vector3 position, float radius, float damage, bool seedSecondary)
    {
        blast.BeginArea();
        int count = Physics2D.OverlapCircleNonAlloc(position, radius, overlapResults, enemyLayer);
        for (int i = 0; i < count; i++)
        {
            var enemy = overlapResults[i].GetComponentInParent<Enemy>();
            if (!enemy || !enemy.gameObject.activeSelf || enemy.GetHp <= 0f || !blast.TryDamage(enemy.GetInstanceID())) continue;
            Vector3 center = enemy.transform.position;
            enemy.TakeDamage(damage);
            if (enemy.GetHp > 0f && enemy.gameObject.activeSelf) continue;
            if (blast.RecordKill(seedSecondary))
                secondaryCenters.Add(center);
        }
        System.Array.Clear(overlapResults, 0, count);
        blast.BeginArea();
    }

    IEnumerator Accelerate()
    {
        moveSpeed = startSpeed;
        if (Target != null)
        {
            AudioManager.Instance.PlaySfx(missileLunchSFX);
        }
        
        yield return _waitForSeconds;
        
        moveSpeed = maxSpeed;

    }
}
