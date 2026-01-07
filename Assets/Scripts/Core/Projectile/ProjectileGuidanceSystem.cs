using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileGuidanceSystem : MonoBehaviour
{
    [SerializeField] Projectile _projectile;
    [SerializeField] float minBallisticAngle = 50f;
    [SerializeField] float maxBallisticAngle = 80f;
    private Vector3 _targetDirection;
    private float _ballisticAngle;
    public IEnumerator HomingCoroutine(GameObject target)
    {
        _ballisticAngle = Random.Range(minBallisticAngle, maxBallisticAngle);
        while (gameObject.activeSelf)
        {
            if (target.activeSelf)
            {
                _targetDirection = (target.transform.position - transform.position).normalized;
                transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(_targetDirection.y,_targetDirection.x) * Mathf.Rad2Deg ,Vector3.forward);
                transform.rotation *= Quaternion.Euler(0, 0, _ballisticAngle);
                _projectile.Move();
            }
            else
            {
                _projectile.Move();
            }
            yield return null;
        }
    }
}
