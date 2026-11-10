using System.Collections;
using System.Collections.Generic;
using Core.UI;
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
        if (s_amount <= 0 || !m_isReady) return;

        m_isReady = false;
        AudioManager.Instance.PlaySfx(lunchSFX);
        PoolManger.Release(missile, muzzle.position);
        s_amount--;
        missileView.UpdateAmountText(s_amount);
        if (s_amount == 0)
        {
            missileView.UpdateCooldown(1);
        }
        else
        {
            StartCoroutine(nameof(WaitForCooldown));
        }
    }

    private IEnumerator WaitForCooldown()
    {
        var cooldownValue = cooldown;

        while (cooldownValue > 0f)
        {
            missileView.UpdateCooldown(cooldownValue / cooldown);
            cooldownValue = Mathf.Max(cooldownValue - Time.deltaTime, 0f);
            yield return null;
        }

        m_isReady = true;
    }

    public void AddMissile(int amount)
    {
        MissileSystem.s_amount += amount;
        missileView.UpdateAmountText(MissileSystem.s_amount);

    }
}