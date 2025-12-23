using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("--------死亡--------")]
    [SerializeField] GameObject deathVfx;
    [SerializeField] private AudioData[] deathSFX;
    
    [Header("--------HP--------")]
    [SerializeField] protected float maxHp = 100f;
    [SerializeField] StateBar onHeadStateBar;
    [SerializeField] bool showHeadStateBar = true;
    protected float hp = 100f;
    public float GetHp => hp;

    protected virtual void OnEnable()
    {
        hp = maxHp;
        if (showHeadStateBar)
        {
            ShowHeadStateBar();
        }
        else
        {
            HideHeadStateBar();
        }
    }
    
    private void ShowHeadStateBar()
    {
        onHeadStateBar.gameObject.SetActive(true);
        onHeadStateBar.Initialize(hp, maxHp);
    }

    private void HideHeadStateBar()
    {
        onHeadStateBar.gameObject.SetActive(false);
    }

    /// <summary>
    /// 受到伤害的方法。
    /// 当角色受到伤害时，会减少生命值并检查是否死亡。
    /// </summary>
    /// <param name="damage">
    /// 受到的伤害量。
    /// </param>
    public virtual void TakeDamage(float damage)
    {
        if (hp <= 0) return;
        hp -= damage;
        if (showHeadStateBar)
        {
            onHeadStateBar.UpdateState(hp, maxHp);
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 角色死亡的方法。
    /// 当角色死亡时，会播放死亡特效并隐藏角色。
    /// </summary>
    public virtual void Die()
    {
        hp = 0; //死亡时血量归零
        AudioManager.Instance.PlayRandomSFX(deathSFX); //播放死亡音效
        PoolManger.Release(deathVfx, transform.position); //播放死亡特效
        gameObject.SetActive(false); //隐藏角色
    }

    /// <summary>
    /// 恢复角色的生命值。
    /// 如果角色的生命值已经达到最大值，则不进行任何操作。
    /// </summary>
    /// <param name="health">要恢复的生命值数量。</param>
    public virtual void RestoreHealth(float health)
    {
        // 检查生命值是否已经达到最大值，如果是则直接返回
        if (Mathf.Abs(hp - maxHp) < Mathf.Epsilon) return;
        // 恢复生命值并确保生命值在0到最大生命值之间
        hp = Mathf.Clamp(hp + health, 0f, maxHp);
        if (showHeadStateBar)
        {
            onHeadStateBar.UpdateState(hp, maxHp);
        }
    }

    /// <summary>
    /// 恢复角色生命值的协程。
    /// 当角色需要恢复生命值时，会在指定的时间间隔内逐渐恢复生命值。
    /// </summary>
    /// <param name="waitForSeconds">
    /// 恢复生命值的时间间隔。
    /// </param>
    /// <param name="percent">
    /// <para>恢复生命值的百分比。</para>
    /// </param>
    /// <returns>
    /// </returns>
    protected IEnumerator HealthRegenerateCoroutine(WaitForSeconds waitForSeconds, float percent)
    {
        while (hp < maxHp)
        {
            yield return waitForSeconds;
            RestoreHealth(maxHp * percent);
        }
    }

    /// <summary>
    /// Dot伤害-持续伤害角色的协程。
    /// 当角色受到持续伤害时，会在指定的时间间隔内减少生命值。
    /// </summary>
    /// <param name="waitForSeconds">
    /// 持续伤害的时间间隔。
    /// </param>
    /// <param name="percent">
    /// <para>持续伤害的百分比。</para>
    /// </param>
    /// <returns></returns>
    protected IEnumerator DamageOverTimeCoroutine(WaitForSeconds waitForSeconds, float percent)
    {
        while (hp > 0f)
        {
            yield return waitForSeconds;
            TakeDamage(maxHp * percent);
        }
    }
}