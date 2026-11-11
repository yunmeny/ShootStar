using System.Collections.Generic;
using UnityEngine;
using Core.SystemModules;

public class PlayerProjectile : Projectile
{
    private TrailRenderer trail;
    private Gradient normalTrail;
    private Collider2D shotCollider;
    private Vector3 normalScale;
    private readonly List<Collider2D> ignoredColliders = new List<Collider2D>();
    private MainGunHitState hitState;
    private float shotDamageMultiplier = 1f;
    private float fragmentDamage;
    private Vector2 fragmentDirection;
    private GameObject fragmentPrefab;
    private RunUpgradeController fragmentOwner;
    protected bool IsFragment { get; private set; }
    protected override float DamageMultiplier => shotDamageMultiplier;
    protected override float HitDamage => IsFragment ? fragmentDamage : base.HitDamage;

    // Written while inactive by the pool, then consumed once by InitializePlayerShot.
    private bool pendingMainGun;
    private bool pendingFragment;
    private float pendingDamage;
    private Vector2 pendingDirection;
    private Enemy pendingIgnoredEnemy;
    private GameObject pendingFragmentPrefab;
    private RunUpgradeController pendingOwner;
    private float pendingVolleyMultiplier = 1f;

    public static void FireMainGun(GameObject prefab, GameObject sideShotPrefab, Vector3 position, float volleyMultiplier = 1f)
    {
        PoolManger.Release(prefab, position, Quaternion.identity, instance =>
        {
            var shot = instance.GetComponent<PlayerProjectile>();
            shot.pendingMainGun = true;
            shot.pendingFragmentPrefab = sideShotPrefab;
            shot.pendingVolleyMultiplier = volleyMultiplier;
        });
    }

    protected void InitializePlayerShot()
    {
        var controller = RunUpgradeController.Instance;
        var run = controller ? controller.Run : null;
        IsFragment = pendingFragment;
        shotDamageMultiplier = pendingMainGun ? run?.MainGunDamageMultiplier ?? 1f : run?.DamageMultiplier ?? 1f;
        if (pendingMainGun) shotDamageMultiplier *= pendingVolleyMultiplier;
        fragmentDamage = pendingDamage;
        fragmentDirection = pendingDirection;
        fragmentPrefab = pendingFragmentPrefab;
        fragmentOwner = pendingOwner;
        hitState.Reset(pendingMainGun && (run?.HasPierce ?? false), pendingMainGun && (run?.HasSplit ?? false),
            IsFragment, pendingIgnoredEnemy ? pendingIgnoredEnemy.GetInstanceID() : 0);
        transform.localScale = IsFragment ? normalScale * 0.65f : normalScale;
        if (trail)
        {
            trail.Clear();
            trail.colorGradient = normalTrail;
            if (IsFragment || (pendingMainGun && (run?.HasPierce ?? false)))
            {
                trail.startColor = IsFragment ? new Color(1f, 0.65f, 0.15f) : Color.cyan;
                trail.endColor = new Color(trail.startColor.r, trail.startColor.g, trail.startColor.b, 0f);
            }
        }
        if (pendingIgnoredEnemy) IgnoreTarget(pendingIgnoredEnemy);
        pendingMainGun = pendingFragment = false;
        pendingIgnoredEnemy = null;
        pendingFragmentPrefab = null;
        pendingOwner = null;
        pendingDamage = 0f;
        pendingDirection = Vector2.zero;
        pendingVolleyMultiplier = 1f;
    }

    protected override void OnEnable()
    {
        InitializePlayerShot();
        base.OnEnable();
    }
    private void Awake()
    {
        if (moveDirection != Vector2.right)
        {
            transform.GetChild(0).rotation = Quaternion.FromToRotation(Vector2.right, moveDirection);
        }
        trail = GetComponentInChildren<TrailRenderer>();
        if (trail) normalTrail = trail.colorGradient;
        shotCollider = GetComponent<Collider2D>();
        normalScale = transform.localScale;
    }

    private void OnDisable()
    {
        if (trail) trail.Clear();
        foreach (var other in ignoredColliders)
            if (shotCollider && other) Physics2D.IgnoreCollision(shotCollider, other, false);
        ignoredColliders.Clear();
        if (IsFragment && fragmentOwner) fragmentOwner.ReleaseFragment();
        IsFragment = false;
        fragmentOwner = null;
    }

    private void IgnoreTarget(Enemy enemy)
    {
        foreach (var other in enemy.GetComponentsInChildren<Collider2D>())
        {
            if (!shotCollider || ignoredColliders.Contains(other)) continue;
            Physics2D.IgnoreCollision(shotCollider, other, true);
            ignoredColliders.Add(other);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (!gameObject.activeSelf) return;
        var enemy = collision.gameObject.GetComponentInParent<Enemy>();
        if (!enemy) return;
        if (!hitState.TryHit(enemy.GetInstanceID(), out bool keepAlive, out bool fork))
        {
            IgnoreTarget(enemy);
            return;
        }
        Vector3 hitPosition = collision.GetContact(0).point;
        Vector2 direction = transform.TransformDirection(moveDirection).normalized;
        if (keepAlive) IgnoreTarget(enemy);
        ApplyHit(enemy, collision);
        PlayerEnergy.Instance.AccumulateEnergy(PlayerEnergy.PERCENT);
        if (fork) SpawnSideShots(hitPosition, direction, enemy);
        if (keepAlive) OnPiercedTarget();
        if (!keepAlive) gameObject.SetActive(false);
    }

    protected virtual void OnPiercedTarget() { }

    private void SpawnSideShots(Vector3 position, Vector2 direction, Enemy firstEnemy)
    {
        var owner = RunUpgradeController.Instance;
        if (!fragmentPrefab || !owner) return;
        float damage = HitDamage * 0.4f;
        for (int i = -1; i <= 1; i += 2)
        {
            if (!owner.TryReserveFragment()) break;
            Vector2 sideDirection = Quaternion.Euler(0f, 0f, 25f * i) * direction;
            PoolManger.Release(fragmentPrefab, position, Quaternion.FromToRotation(Vector3.right, sideDirection), instance =>
            {
                var shot = instance.GetComponent<PlayerProjectile>();
                shot.pendingFragment = true;
                shot.pendingDamage = damage;
                shot.pendingDirection = sideDirection;
                shot.pendingIgnoredEnemy = firstEnemy;
                shot.pendingOwner = owner;
            });
        }
    }

    public override void Move()
    {
        if (IsFragment) transform.position += (Vector3)(fragmentDirection * (Time.deltaTime * moveSpeed));
        else base.Move();
    }
}
