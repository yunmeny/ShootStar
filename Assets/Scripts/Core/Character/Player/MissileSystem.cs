using System.Collections;
using System.Collections.Generic;
using Core.UI;
using Core.SystemModules;
using UnityEngine;

public class MissileSystem : MonoBehaviour
{
    [SerializeField] private MissileView missileView;
    [SerializeField] private PlayerInput Input;
    [SerializeField] private GameObject missile;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private int defaultAmount = 5;
    [SerializeField] private AudioData lunchSFX;

    private bool m_isReady = true;
    private static int s_amount;
    private float remainingCooldown;
    public int Amount => s_amount;

    private void Awake()
    {
        s_amount = defaultAmount;
    }

    private void OnEnable()
    {
        Input.onMissile += OnMissile;
        missileView.UpdateAmountText(s_amount);
    }

    private void OnDisable()
    {
        Input.onMissile -= OnMissile;
    }

    private void OnMissile()
    {
        if (GameManager.GameState != GameState.Play || s_amount <= 0 || !m_isReady) return;

        m_isReady = false;
        AudioManager.Instance.PlaySfx(lunchSFX);
        PoolManger.Release(missile, muzzle.position, Quaternion.identity, _ => { });
        s_amount--;
        missileView.UpdateAmountText(s_amount);
        remainingCooldown = cooldown * (RunUpgradeController.Instance ? RunUpgradeController.Instance.Run.MissileCooldownMultiplier : 1f);
        StartCoroutine(nameof(WaitForCooldown));
    }

    private IEnumerator WaitForCooldown()
    {
        float duration = Mathf.Max(0.001f, remainingCooldown);

        while (remainingCooldown > 0f)
        {
            missileView.UpdateCooldown(remainingCooldown / duration);
            remainingCooldown = Mathf.Max(remainingCooldown - Time.deltaTime, 0f);
            yield return null;
        }

        m_isReady = true;
        missileView.UpdateCooldown(0f);
    }

    public void AddMissile(int amount)
    {
        MissileSystem.s_amount += amount;
        missileView.UpdateAmountText(MissileSystem.s_amount);

    }
}
