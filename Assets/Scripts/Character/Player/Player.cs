using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))] //添加刚体组件
public class Player : Character
{
    [Header("--------状态--------")]
    [SerializeField] StateBar_HUD onHUDStateBar;
    [SerializeField] private EnergyBar_HUD energyBarHUD;

    [Header("--------角色--------")] 
    [SerializeField] bool isGenerateHealth = true;
    [SerializeField] float generateHealthInterval = 1f;
    [SerializeField, Range(0f, 1f)] float generateHealthPercent = 0.01f;

    [Header("--------移动--------")]
    [SerializeField] PlayerInput Input; //注册input类
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] private float accelerationTime = 2f;
    [SerializeField] private float decelerationTime = 2f;
    [SerializeField] private float moveQuaternionAngleTb = 50f;
    [SerializeField] private float moveQuaternionAngleLr = 50f;
    private Vector2 moveDirection;
    private float paddingX;
    private float paddingY;
    

    [Header("--------攻击--------")]
    [SerializeField] private GameObject projectile1;
    [SerializeField] private GameObject projectile2;
    [SerializeField] private GameObject projectile3;
    [SerializeField, Range(0, 2)] public int weaponPower = 0;
    [SerializeField] private Transform muzzleTop;
    [SerializeField] private Transform muzzleMiddle;
    [SerializeField] private Transform muzzleBottom;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private AudioData projectileLunchSFX;
    [SerializeField] private ParticleSystem fireVfx;
    private int _maxWeaponPower = 2;
    public int MaxWeaponPower => _maxWeaponPower ;
    private bool _isPersistWeaponPower = false;
    
    
    [Header("--------闪避--------")]
    [SerializeField,Range(0,100)] private int dodgeCost = 25;
    [SerializeField] private float maxRoll = 720f;
    [SerializeField] private float rollSpeed = 360f; //滚转速度 即每秒绕X轴滚转的角度
    [SerializeField] Vector3 dodgeScale = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private AudioData dodgeSFX;
    
    [Header("--------爆发--------")]
    [SerializeField] private GameObject overdriveProjectile;
    [SerializeField] private float overdriveSpeedFactor = 1.2f;
    [SerializeField] private float overdriveFireFactor = 1.2f;
    [SerializeField] private float overdriveDodgeFactor = 2f;
    
    private float _currentRoll;
    private bool _isDodging = false;
    private bool _isOverdriving = false;
    float dodgeDuration;
    readonly float InvincibleTime = 0.5f;
    private float dodgeBulletTime = 0.25f;
    private float overdriveBulletTime = 0.5f;
    private float takeDamageBulletTime = 0.5f;
    
    private Vector2 previousVelocity;
    private Quaternion previousRotation;
    private WaitForFixedUpdate _waitForMoveFixedUpdate; // 移动协程中
    private WaitForSeconds _waitForFireSeconds;
    private WaitForSeconds _waitForOverdriveFireSeconds;
    private WaitForSeconds _waitForSecondsGenerateHealth;
    private WaitForSeconds _waitForInvincible;
    private Coroutine _moveCoroutine;
    private Coroutine _healthRegenerateCoroutine;
    private Rigidbody2D _rigidbody;
    Collider2D _collider;
    
    public bool IsMaxHealth => Math.Abs(hp - maxHp) < Mathf.Epsilon;

    #region EVEVT

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        
        dodgeDuration = maxRoll / rollSpeed; //计算闪避持续时间
        _rigidbody.gravityScale = 0f; //设置重力为0 使其凭空悬浮 使玩家可以在场景中任意移动
        
        paddingX = transform.GetChild(0).GetComponent<Renderer>().bounds.size.x / 2;
        paddingY = transform.GetChild(0).GetComponent<Renderer>().bounds.size.y / 2;
        
        _waitForFireSeconds = new WaitForSeconds(fireInterval);
        _waitForOverdriveFireSeconds = new WaitForSeconds(fireInterval / overdriveFireFactor);
        _waitForSecondsGenerateHealth = new WaitForSeconds(generateHealthInterval);
        _waitForInvincible = new WaitForSeconds(InvincibleTime);
        _waitForMoveFixedUpdate = new WaitForFixedUpdate();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Input.onMove += Move; //订阅事件
        Input.onStopMove += StopMove;
        Input.onFire += Fire;
        Input.onStopFire += StopFire;
        Input.onDodge += Dodge;
        Input.onOverdrive += Overdrive;
        PlayerOverdrive.on += OverdriveOn;
        PlayerOverdrive.off += OverdriveOff;
        Input.EnableGamePlayInput();
        GameManager.GameState = GameState.Play;
    }

    void OnDisable()
    {
        Input.onMove -= Move;
        Input.onStopMove -= StopMove;
        Input.onFire -= Fire;
        Input.onStopFire -= StopFire;
        Input.onDodge -= Dodge; 
        Input.onOverdrive -= Overdrive;
        PlayerOverdrive.on -= OverdriveOn;
        PlayerOverdrive.off -= OverdriveOff;
    }

    void Start()
    {
        onHUDStateBar.Initialize(hp, maxHp);
    }

    #endregion

    #region Health

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if(!gameObject.activeSelf)  return;

        if (_isPersistWeaponPower)
        {
            weaponPower = Mathf.Clamp(weaponPower - 1, 0, _maxWeaponPower);
        }
        onHUDStateBar.UpdateState(hp, maxHp);
        TimeController.Instance.BulletTime(takeDamageBulletTime);
        StartCoroutine(InvincibleCoroutine());
        Move(moveDirection);
        
        if (!isGenerateHealth) return;
        
        if (_healthRegenerateCoroutine != null)
        {
            StopCoroutine(_healthRegenerateCoroutine);
        }

        _healthRegenerateCoroutine =
            StartCoroutine(HealthRegenerateCoroutine(_waitForSecondsGenerateHealth, generateHealthPercent));
    }

    IEnumerator InvincibleCoroutine()
    {
        _collider.isTrigger = true;
        yield return _waitForInvincible;
        _collider.isTrigger = false;
    }

    public override void RestoreHealth(float health)
    {
        base.RestoreHealth(health);
        onHUDStateBar.UpdateState(hp, maxHp);
    }

    public override void Die()
    {
        GameManager.onGameOver?.Invoke();
        GameManager.GameState = GameState.GameOver;
        onHUDStateBar.UpdateState(0, maxHp);
        base.Die();
    }

    #endregion
    
    #region Move

    private void Move(Vector2 input)
    {
        if (GameManager.GameState != GameState.Play) return;
    
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine); //StopCoroutine的第三个重载重载:一个协程变量 以解决变量不对称
        }
        moveDirection = input.normalized;
        _moveCoroutine = StartCoroutine(MoveCoroutine(accelerationTime, moveSpeed * moveDirection,
            Quaternion.AngleAxis(moveQuaternionAngleTb * input.y, Vector3.right),
            Quaternion.AngleAxis(moveQuaternionAngleLr * input.x, Vector3.forward))); 

        StartCoroutine(nameof(MovePositionLimitCoroutine)); //启动协程
    }

    private void StopMove()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        moveDirection = Vector2.zero;
        _moveCoroutine =
            StartCoroutine(MoveCoroutine(decelerationTime, Vector2.zero, Quaternion.identity, Quaternion.identity));
        StartCoroutine(nameof(WaitForVelocityZero)); //等待速度为0 再关闭协程 防止速度不为0时协程已经关闭
    }

    
    // 等待速度为0
    private IEnumerator WaitForVelocityZero()
    {
        yield return new WaitUntil(() => _rigidbody.velocity == Vector2.zero);
        StopCoroutine(nameof(MovePositionLimitCoroutine)); //关闭协程
    }

    //开始移动协程
    private IEnumerator MoveCoroutine(float time, Vector2 moveVelocity, Quaternion moveTopBottomQuaternion,
        Quaternion moveLeftRightQuaternion)
    {
        previousVelocity = _rigidbody.velocity; //获取当前速度的模长
        previousRotation = transform.rotation; //获取当前旋转的角度
        var t = 0f;
        while (t < time)
        {
            t += Time.fixedDeltaTime;
            var moveQuaternion = moveTopBottomQuaternion * moveLeftRightQuaternion; //将两个旋转组合
            _rigidbody.velocity = Vector2.Lerp(previousVelocity, moveVelocity, t / time);
            transform.rotation = Quaternion.Lerp(previousRotation, moveQuaternion, t / time); //Quaternion
            yield return _waitForMoveFixedUpdate;
        }
    }

    //通过协程更新  协程：Coroutine
    private IEnumerator MovePositionLimitCoroutine()
    {
        while (gameObject.activeSelf)
        {
            transform.position =
                Viewport.Instance.PlayerMoveablePosition(transform.position, paddingX, paddingY); //更新位置限定在范围内
            yield return null;
        }
    }

    #endregion

    #region Fire

    void Fire()
    {
        StartCoroutine(nameof(FireCoroutine));
        fireVfx.Play();
        // StartCoroutine(FireCoroutine());
    }

    void StopFire()
    {
        StopCoroutine(nameof(FireCoroutine));
        fireVfx.Stop();
        // StopCoroutine(FireCoroutine());
    }

    IEnumerator FireCoroutine()
    {
        while (gameObject.activeSelf)
        {
            switch (weaponPower)
            {
                case 0:
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile1, muzzleMiddle.position); //通过对象池生成子弹对象实例以发射子弹
                    break;
                case 1:
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile1, muzzleTop.position);
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile1, muzzleBottom.position);
                    break;
                case 2:
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile2, muzzleTop.position);
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile1, muzzleMiddle.position);
                    PoolManger.Release(_isOverdriving ? overdriveProjectile : projectile3, muzzleBottom.position);
                    break;
            }
            AudioManager.Instance.PlayRandomSFX(projectileLunchSFX);
            
            yield return _isOverdriving ? _waitForOverdriveFireSeconds : _waitForFireSeconds;
        }
    }

    
    // 随机获得武器威力 可能会出现武器威力为0的情况
    public void RandomWeaponPower()
    {
        _isPersistWeaponPower = true;
        var power = UnityEngine.Random.Range(0, 2);
        weaponPower = power;
    }

    public void AddWeaponPower(WaitForSeconds waitForSeconds = null)
    {
        _isPersistWeaponPower = false;
        if (weaponPower == 2)
        {
            return;
        }
        StartCoroutine(AddWeaponPowerCoroutine(waitForSeconds ?? new WaitForSeconds(3f)));
    }

    public void PersistWeaponPower()
    {
        _isPersistWeaponPower = true;
        weaponPower = Mathf.Clamp(weaponPower + 1, 0, MaxWeaponPower);
    }

    IEnumerator AddWeaponPowerCoroutine(WaitForSeconds waitForSeconds)
    {
        weaponPower++;
        yield return waitForSeconds;
        weaponPower--;
    }
    

    #endregion

    #region Dodge

    void Dodge()
    {
        if (_isDodging || !PlayerEnergy.Instance.IsEnough(dodgeCost)) return;
        TimeController.Instance.BulletTime(dodgeBulletTime,dodgeBulletTime);
        StartCoroutine(nameof(DodgeCoroutine));
    }

    IEnumerator DodgeCoroutine()
    {
        _isDodging = true;
        AudioManager.Instance.PlayRandomSFX(dodgeSFX);
        // 消耗能量
        PlayerEnergy.Instance.UseEnergy(dodgeCost);
        
        // 让玩家无敌
        _collider.isTrigger = true;
        // 让玩家围绕X轴旋转
        // 让玩家缩小
        var scale = transform.localScale;
        _currentRoll = 0f;
        while ( _currentRoll < maxRoll ) // 整个过程持续 maxRoll / rollSpeed 秒 ：2秒
        {
            _currentRoll+= rollSpeed * Time.deltaTime;
            transform.rotation = Quaternion.AngleAxis(_currentRoll, Vector3.right);
            
            // 线性缩放
            // if (_currentRoll < maxRoll / 2f)
            // {
            //     scale -= Time.deltaTime * dodgeScale;
            //     
            // }
            // else
            // {
            //     scale += Time.deltaTime * dodgeScale;
            // }
            // transform.localScale = scale;
            
            
            // 贝塞尔曲线缩放
            transform.localScale = BezierCurve.QuadraticBezier(Vector3.one, dodgeScale, Vector3.one,
                _currentRoll / maxRoll) ;
            
            yield return null;
        }
        transform.localScale = Vector3.one;
        _collider.isTrigger = false;
        _isDodging = false;
    }
    

    #endregion

    #region Overdrive

    void Overdrive()
    {
        if (!PlayerEnergy.Instance.IsEnough(PlayerEnergy.MAX_ENERGY)) return;
        TimeController.Instance.BulletTime(overdriveBulletTime,overdriveBulletTime);
        PlayerOverdrive.on.Invoke();
    }
    
    void OverdriveOn()
    {
        _isOverdriving = true;
        dodgeCost = (int) (dodgeCost * overdriveDodgeFactor);
        moveSpeed *= overdriveSpeedFactor;
    }

    void OverdriveOff()
    {
        _isOverdriving = false;
        dodgeCost = (int) (dodgeCost / overdriveDodgeFactor);
        moveSpeed /= overdriveSpeedFactor;
    }

    #endregion
    
}