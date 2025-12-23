using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Loot : MonoBehaviour
{
    [SerializeField] protected float maxSpeed = 15f;
    [SerializeField] protected float minSpeed = 5f;
    [SerializeField] protected AudioData defaultPickUpSfx;
    protected Player Player;
    protected Animator PickUpAnimator;
    protected int PickUpParameterHash = Animator.StringToHash("PickUp");
    protected AudioData PickUpSfx;
    
    protected virtual void Awake()
    {
        Player = FindObjectOfType<Player>();
        PickUpAnimator = GetComponent<Animator>();
        PickUpSfx = defaultPickUpSfx;
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(nameof(MoveToPlayer));
    }
    

    void OnTriggerEnter2D(Collider2D other)
    {
        PickUp();
    }
    
    protected virtual void PickUp()
    {
        StopCoroutine(nameof(MoveToPlayer));
        PickUpAnimator.Play(PickUpParameterHash);
        AudioManager.Instance.PlayRandomSFX(PickUpSfx);
    }
    
    // 自动移动向玩家的方法
    protected virtual IEnumerator MoveToPlayer()
    {
        var speed = Random.Range(minSpeed, maxSpeed);
        while (gameObject.activeSelf && GameManager.GameState == GameState.Play)
        {
            transform.position = Vector3.MoveTowards(transform.position, Player.transform.position, speed * Time.deltaTime);
            yield return null;
        }   
    }
    
}
