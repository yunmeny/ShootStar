using UnityEngine;

public class Boss : Enemy
{
    BossHealthBar healthBar;
    Canvas healthBarCanvas;

    protected override void Awake()
    {
        base.Awake();
        healthBar = FindObjectOfType<BossHealthBar>();
        healthBarCanvas = healthBar.GetComponentInChildren<Canvas>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        healthBar.Initialize(hp, maxHp);
        healthBarCanvas.enabled = true;
    }

    protected override void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.TryGetComponent<Player>(out Player player)) return;
        player.Die();
    }

    public override void Die()
    {
        base.Die();
        healthBarCanvas.enabled = false;
        
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        healthBar.UpdateState(hp, maxHp);
    }
}
