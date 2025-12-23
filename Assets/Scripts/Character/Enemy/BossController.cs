using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossController : EnemyController
{
    [SerializeField] float fireDuration = 1.5f;
    [Header("--------检测--------")]
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform playerDetectionTransform;
    [SerializeField] Vector3 playerDetectionSize;

    [Header("--------激光--------")]
    [SerializeField] private float beamCooldownTime = 12f;
    [SerializeField] private float beamDuration = 4f;
    [SerializeField] private float beamChargingDuration = 3f;
    [SerializeField] private AudioData beamLaunchSFX;
    [SerializeField] private AudioData beamChargingSFX;

    private Transform _playerTransform;
    private bool isBeamReady;
    private bool isBeamDuration;
    private List<GameObject> _magazine;
    private AudioData lunchSFX;
    private WaitForSeconds _waitForContinuouslyFire;
    private WaitForSeconds _waitForFireInterval;
    private WaitForSeconds _waitForBeamCooldown;
    private WaitForSeconds _waitForBeamDuration;
    private WaitForSeconds _waitForBeamChargingDuration;
    private Animator _animator;
    private int _parameterHash;
    protected override void Awake()
    {
        base.Awake();
        _playerTransform = FindObjectOfType<Player>().transform;
        _magazine = new List<GameObject>(projectiles.Length);
        _waitForContinuouslyFire = new WaitForSeconds(minFireInterval);
        _waitForFireInterval = new WaitForSeconds(maxFireInterval);
        _waitForBeamCooldown = new WaitForSeconds(beamCooldownTime);
        _waitForBeamDuration = new WaitForSeconds(beamDuration);
        _waitForBeamChargingDuration = new WaitForSeconds(beamChargingDuration);
        _animator = GetComponent<Animator>();
        _parameterHash = Animator.StringToHash("launchBeam");
    }

    protected override void OnEnable()
    {
        isBeamReady = false;
        isBeamDuration = false;
        StartCoroutine(BeamCooldownCoroutine()); // BOSS激活时大招就开始冷却
        base.OnEnable();
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(playerDetectionTransform.position, playerDetectionSize);
    }

    void LoadProjectiles()
    {
        _magazine.Clear();
        if (Physics2D.OverlapBox(playerDetectionTransform.position, playerDetectionSize, 0f, playerLayer))
        {
            _magazine.Add(projectiles[0]);
            lunchSFX = projectileLunchSFX[0];
        }
        else
        {
            if (UnityEngine.Random.value > 0.5f)
            {
                _magazine.Add(projectiles[1]);
                lunchSFX = projectileLunchSFX[1];
            }
            else
            {
                for (var i = 2; i < projectiles.Length; i++)
                {
                    _magazine.Add(projectiles[i]);
                }
                lunchSFX = projectileLunchSFX[2];

            }
        }
    }

    protected override IEnumerator RandomMoveCoroutine()
    {
        transform.position = Viewport.Instance.RandomEnemySpawnPosition(paddingX, paddingY);
        var targetPosition = Viewport.Instance.RandomRightHalfPosition(paddingX, paddingY);
        while (gameObject.activeSelf)
        {
            if (isBeamDuration)
            {
                var position = transform.position;
                var targetVer = new Vector2(Viewport.Instance.MaxX-paddingX, _playerTransform.position.y);
                position = Vector3.MoveTowards(position, targetVer, moveSpeed * Time.fixedDeltaTime);
                transform.position = position;
            }
            else
            {
                if (Vector3.Distance(transform.position, targetPosition) > moveSpeed * Time.fixedDeltaTime)
                {
                    Transform preTransform;
                    (preTransform = transform).position =
                        Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
                    transform.rotation =
                        Quaternion.AngleAxis((targetPosition - preTransform.position).normalized.y * moveRotationAngle,
                            Vector3.right);

                }
                else
                {
                    targetPosition = Viewport.Instance.RandomRightHalfPosition(paddingX, paddingY);
                }
            }
            yield return _waitForFixedUpdate;
        }
    }

    protected override IEnumerator RandomFireCoroutine()
    {
        while (isActiveAndEnabled)
        {
            if (GameManager.GameState == GameState.GameOver || !gameObject.activeSelf)
            {
                yield break;
            }
            yield return _waitForFireInterval;
            yield return ContinuouslyFireCoroutine();
            yield return BeamCoroutine();
        }
    }

    IEnumerator ContinuouslyFireCoroutine()
    {
        LoadProjectiles();
        var timer = 0f;
        while (timer < fireDuration)
        {
            if (GameManager.GameState == GameState.GameOver)
            {
                yield break;
            }
            foreach (var projectile in _magazine)
            {
                PoolManger.Release(projectile, muzzle.position);
            }
            AudioManager.Instance.PlayRandomSFX(lunchSFX);
            timer += minFireInterval;
            yield return _waitForContinuouslyFire;
            fireVfx.Play();
        }
    }

    
    // NOTE: 也可以使用动画事件来控制激光的发射
    IEnumerator BeamCoroutine()
    {
        if (!isBeamReady)
        {
            yield break;
        }
        isBeamDuration = true;
        isBeamReady = false;
        _animator.SetTrigger(_parameterHash);
        AudioManager.Instance.PlaySFX(beamChargingSFX);
        yield return _waitForBeamChargingDuration;
        AudioManager.Instance.PlaySFX(beamLaunchSFX);
        StartCoroutine(BeamCooldownCoroutine());
        yield return _waitForBeamDuration;
        isBeamDuration = false;
    }

    IEnumerator BeamCooldownCoroutine()
    {
        yield return _waitForBeamCooldown;
        isBeamReady = true;
    }
    
}
