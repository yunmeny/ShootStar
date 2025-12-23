using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : Singleton<PlayerEnergy> 
{
    [SerializeField] EnergyBar_HUD energyBar;
    [SerializeField] float overdriveInterval = 0.1f;
    public const int MAX_ENERGY = 100;
    public const int PERCENT = 1;
    int _energy;
    public int Energy => _energy;
    private bool _available = true;
    WaitForSeconds _waitForOverdriveInterval;

    protected override void Awake()
    {     
        base.Awake();
        _waitForOverdriveInterval = new WaitForSeconds(overdriveInterval);
    }

    private void Start()
    {
        _energy = MAX_ENERGY;
        energyBar.Initialize(_energy, MAX_ENERGY);
    }

    private void OnEnable()
    {
        PlayerOverdrive.on += PlayerOverdriveOn;
        PlayerOverdrive.off += PlayerOverdriveOff;
    }

    private void OnDisable()
    {
        PlayerOverdrive.on -= PlayerOverdriveOn;
        PlayerOverdrive.off -= PlayerOverdriveOff;
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    void PlayerOverdriveOn()
    {
        _available = false;
        StartCoroutine(nameof(KeepUsingEnergy));
    }

    void PlayerOverdriveOff()
    {
        _available = true;
        StopCoroutine(nameof(KeepUsingEnergy));
    }

    IEnumerator KeepUsingEnergy()
    {
        while (gameObject.activeSelf && _energy > 0)
        {
            yield return _waitForOverdriveInterval;

            UseEnergy(PERCENT);
        }
    }

    public void AccumulateEnergy(int value)
    {
        if (_energy == MAX_ENERGY || !_available || !gameObject.activeSelf) return;
        _energy = Mathf.Clamp(_energy + value, 0, MAX_ENERGY);
        energyBar.UpdateState(_energy, MAX_ENERGY);
    }

    public void UseEnergy(int value)
    {
        _energy -= value;
        energyBar.UpdateState(_energy, MAX_ENERGY);

        if (_energy == 0 && !_available)
        {
            PlayerOverdrive.off.Invoke();
        }
    }
    public bool IsEnough(int value) => _energy >= value;
}