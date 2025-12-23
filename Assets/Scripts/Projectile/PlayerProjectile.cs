using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    private TrailRenderer _trailRenderer;
    private Player _player;
    private void Awake()
    {
        if (moveDirection != Vector2.right)
        {
            transform.GetChild(0).rotation = Quaternion.FromToRotation(Vector2.right, moveDirection);
        }
        _player = FindObjectOfType<Player>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
    }

    private void OnDisable()
    {
        _trailRenderer.Clear();
    }

    
    // 碰撞一次就会调用一次
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        PlayerEnergy.Instance.AccumulateEnergy(PlayerEnergy.PERCENT);
        
    }
}