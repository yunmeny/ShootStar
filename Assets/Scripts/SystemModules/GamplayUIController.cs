using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamplayUIController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] Canvas hudCanvas;
    [SerializeField] Canvas menuCanvas;
    
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;
    
    [SerializeField] private AudioData pauseSFX;
    [SerializeField] private AudioData unpauseSFX;
    
    
    public static Dictionary<string, Button> ButtonTable;
    
    int _buttonPressedParameterID = Animator.StringToHash("Pressed"); // 转换字符串为参数 ID

    private void Awake()
    {
        ButtonTable = new Dictionary<string, Button>();
    }

    void OnEnable()
    {
        playerInput.onPause += Pause;
        playerInput.onUnpause += Unpause;
        
        ButtonPressedState.ButtonFunctionTable.Add(resumeButton, OnResumeButtonClick);
        ButtonPressedState.ButtonFunctionTable.Add(mainMenuButton, OnMainMenuButtonClick);
        ButtonPressedState.ButtonFunctionTable.Add(optionsButton, OnOptionsButtonClick);
        
        ButtonTable.Add(resumeButton.gameObject.name, resumeButton);
        ButtonTable.Add(mainMenuButton.gameObject.name, mainMenuButton);
        ButtonTable.Add(optionsButton.gameObject.name, optionsButton);
    }
    
    private void OnDisable()
    {
        playerInput.onPause -= Pause;
        playerInput.onUnpause -= Unpause;
        
        ButtonPressedState.ButtonFunctionTable.Clear();
    }

    void Pause()
    {
        AudioManager.Instance.PlaySFX(pauseSFX);
        TimeController.Instance.Pause();
        GameManager.GameState = GameState.Pause;
        
        resumeButton.interactable = true;
        mainMenuButton.interactable = true;
        optionsButton.interactable = true;
        menuCanvas.enabled = true;
        hudCanvas.enabled = false;
        playerInput.EnablePauseMenuInput();
        playerInput.SwitchToDynamicUpdateMode();
        UIInput.Instance.SelectUI(resumeButton);
    }

    void Unpause()
    {
        GameManager.GameState = GameState.Play;
        AudioManager.Instance.PlaySFX(unpauseSFX);
        resumeButton.Select();
        resumeButton.animator.SetTrigger(_buttonPressedParameterID);
    }

    void OnResumeButtonClick()
    { 
        TimeController.Instance.UnPause();
        menuCanvas.enabled = false;
        hudCanvas.enabled = true;
        playerInput.EnableGamePlayInput();
        playerInput.SwitchToFixedUpdateMode();
    }

    void OnOptionsButtonClick()
    {
        menuCanvas.enabled = false;
        // 打开设置菜单
        SceneLoader.Instance.LoadOptionsScene();
    }

    void OnMainMenuButtonClick()
    {
        menuCanvas.enabled = false;
        // 回到主菜单
        SceneLoader.Instance.LoadMainMenuScene();
    }
}
