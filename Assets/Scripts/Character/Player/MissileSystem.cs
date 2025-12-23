using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileSystem : MonoBehaviour
{
    [SerializeField] private PlayerInput Input;
    [SerializeField] private GameObject missile;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private Transform muzzle;
    [SerializeField] private int defaultAmount = 5;
    [SerializeField] private AudioData lunchSFX;

    private WaitForSeconds waitforCooldown;
    private bool isReady = true;
    static int _amount;

    private void Awake()
    {
        _amount = defaultAmount;
        waitforCooldown = new WaitForSeconds(cooldown);
    }

    private void OnEnable()
    {
        Input.onMissile += OnMissile;
        MissileDisply.UpdateAmountText(_amount);
    }

    private void OnDisable()
    {
        Input.onMissile -= OnMissile;
    }

    private void OnMissile()
    {
        if (_amount <= 0 || !isReady) return;

        isReady = false;
        AudioManager.Instance.PlaySFX(lunchSFX);
        PoolManger.Release(missile, muzzle.position);
        _amount--;
        MissileDisply.UpdateAmountText(_amount);
        if (_amount == 0)
        {
            MissileDisply.UpdateCooldown(1);
        }
        else
        {
            StartCoroutine(nameof(WaitForCooldown));
        }
    }

    IEnumerator WaitForCooldown()
    {
        var cooldownValue = cooldown;

        while (cooldownValue > 0f)
        {
            MissileDisply.UpdateCooldown(cooldownValue / cooldown);
            cooldownValue = Mathf.Max(cooldownValue - Time.deltaTime, 0f);
            yield return null;
        }

        isReady = true;
    }

    public static void AddMissile(int amount)
    {
        _amount += amount;
        MissileDisply.UpdateAmountText(_amount);

    }
}