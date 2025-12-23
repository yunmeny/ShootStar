using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    private float _angleZ;

    void Awake()
    {
        _angleZ = transform.GetChild(0).rotation.eulerAngles.z * Mathf.Deg2Rad;
        var rotation = new Vector2(Mathf.Cos(_angleZ), Mathf.Sin(_angleZ));

        if (moveDirection != Vector2.left)
        {
            transform.GetChild(0).rotation = Quaternion.FromToRotation(rotation, moveDirection);
            // transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(moveDirection.y,moveDirection.x) * Mathf.Rad2Deg ,Vector3.forward);
        }
    }
}