using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Canvas hudCanvas;
    [SerializeField] AudioData gameOverSFX;
    
    int _exitStateParameterID = Animator.StringToHash("GameOverScreenExit"); // 转换字符串为参数 ID
    Canvas gameOverCanvas;
    Animator animator;

    void Awake()
    {
        gameOverCanvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();
        
        gameOverCanvas.enabled = false;
        animator.enabled = false;

    }

    void OnEnable()
    {
        GameManager.onGameOver += OnGameOver;
        playerInput.onConfirmGameover += OnConfirmGameOver;
    }

    void OnDisable()
    {
        GameManager.onGameOver -= OnGameOver;
        playerInput.onConfirmGameover -= OnConfirmGameOver;
    }

    void OnGameOver()
    {
        hudCanvas.enabled = false;
        gameOverCanvas.enabled = true;
        animator.enabled = true;
        playerInput.DisableAllInput();
    }

    
    // TODO 考虑启用输入的时机：可以在动画播放完成后插入事件帧调用此函数启用输入 || 也可以在动画行为脚本中的Exist事件中调用此函数 
    void EnableGameOverScreenInput()
    {
        playerInput.EnableGameOverInput();
    }

    void OnConfirmGameOver()
    {
        AudioManager.Instance.PlaySFX(gameOverSFX);
        playerInput.DisableAllInput();
        animator.Play(_exitStateParameterID);
        SceneLoader.Instance.LoadScoreScene();
    }
  
}
