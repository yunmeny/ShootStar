using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MainMenuUIController : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Canvas mainMenuCanvas;

    void Start()
    {
        Time.timeScale = 1;
        GameManager.GameState = GameState.Start;
        UIInput.Instance.SelectUI(startButton);
    }
    
    private void OnEnable()
    {
        ButtonPressedState.ButtonFunctionTable.Add(startButton, OnStartButtonClick);
        ButtonPressedState.ButtonFunctionTable.Add(optionButton, OptionButtonClick);
        ButtonPressedState.ButtonFunctionTable.Add(exitButton, ExitButtonClick);
    }

    private void OnDisable()
    {
        ButtonPressedState.ButtonFunctionTable.Clear();
    }

    void OnStartButtonClick()
    {
        mainMenuCanvas.enabled = false;
        SceneLoader.Instance.LoadGamePlayScene();
    }
    
    void OptionButtonClick()
    {
        mainMenuCanvas.enabled = false;
        SceneLoader.Instance.LoadOptionsScene();
    }

    void ExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
