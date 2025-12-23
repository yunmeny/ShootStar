using UnityEngine;

public class Enemy : Character
{
    [SerializeField] int deathEnergyBonus = 3;
    [SerializeField] int deathScore = 500;
    [SerializeField] int waveHealthBonus = 10;
    
    LootSpawner _lootSpawner;

    protected virtual void Awake()
    {
        _lootSpawner = GetComponent<LootSpawner>();
    }
    
    protected override void OnEnable()
    {
        SetHealthValue(EenemyManager.Instance.WaveNumber);

        base.OnEnable();
    }


    public override void Die()
    {
        PlayerEnergy.Instance.AccumulateEnergy(deathEnergyBonus);   // 死亡时增加玩家能量
        EenemyManager.Instance.RemoveEnemyFromList(gameObject);     // 从敌人列表中移除该敌人，以达到监控敌人数量的目的
        ScoreManager.Instance.AddScore(deathScore);
        _lootSpawner.Spawn(transform.position);
        base.Die();
    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.TryGetComponent(out Player player)) return;
        player.Die();
        Die();
    }

    private void SetHealthValue(int wave) => maxHp = (int)(maxHp * (1+wave*waveHealthBonus/100f));
    
}
