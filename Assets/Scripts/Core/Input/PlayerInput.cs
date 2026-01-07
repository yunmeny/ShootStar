using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player Input")] //创建资源列表
public class
    PlayerInput : ScriptableObject, InputActions.IGamePlayActions,
        InputActions.IPauseMenuActions, InputActions.IGameOverActions //可重复使用数据容器 其不需要依附于游戏对象 
{
    private InputActions _inputActions; //实例化类

    // public event System.Action onMove_SysAct;    //与下述代码同理
    public event UnityAction<Vector2> onMove = delegate { }; //注册委托事件 以vector2为参数 以空委托作为初始值
    public event UnityAction onStopMove = delegate { };
    public event UnityAction onFire = delegate { };
    public event UnityAction onStopFire = delegate { };
    public event UnityAction onDodge = delegate { };
    public event UnityAction onOverdrive = delegate { };
    public event UnityAction onPause = delegate { };
    public event UnityAction onUnpause = delegate { };
    public event UnityAction onMissile = delegate { };
    public event UnityAction onConfirmGameover = delegate { };
    
    private void OnEnable()
    {
        _inputActions = new InputActions(); //初始化
        _inputActions.GamePlay.SetCallbacks(this); //设置回调
        _inputActions.PauseMenu.SetCallbacks(this); //设置回调
        _inputActions.GameOver.SetCallbacks(this); //设置回调
    }

    private void OnDisable()
    {
        DisableAllInput();
    }

    void SwitchInputActionMap(InputActionMap inputActionMap, bool isUIInput)
    {
        _inputActions.Disable();
        inputActionMap.Enable(); //启用动作表
        if (isUIInput)
        {
            Cursor.visible = true; //显示鼠标游标
            Cursor.lockState = CursorLockMode.None; //并将游标解锁
        }
        else
        {
            Cursor.visible = false; //隐藏鼠标游标
            Cursor.lockState = CursorLockMode.Locked; //并将游标锁定
        }
    }
    
    // 切换输入更新模式
    public void SwitchToDynamicUpdateMode()=>InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;

    public void SwitchToFixedUpdateMode()=>InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInFixedUpdate;
    
    
    //禁用GamePlay输入方法
    public void DisableAllInput() => _inputActions.Disable();
    
    //激活动作表方法
    public void EnableGamePlayInput() => SwitchInputActionMap(_inputActions.GamePlay, false);
    
    public void EnablePauseMenuInput() => SwitchInputActionMap(_inputActions.PauseMenu, true);
    
    public void EnableGameOverInput() => SwitchInputActionMap(_inputActions.GameOver, true);
    
    //实现接口里面的OnMove方法
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed) //输入阶段
        {
            onMove.Invoke(context.ReadValue<Vector2>()); //传入输入动作获取到的值
        }

        if (context.canceled)
        {
            onStopMove.Invoke();
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onFire.Invoke();
        }

        if (context.canceled)
        {
            onStopFire.Invoke();
        }
    }

    public void OnEnergy(InputAction.CallbackContext context)
    {
    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onDodge.Invoke();
        }
    }

    public void OnOverdrive(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onOverdrive.Invoke();
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onPause.Invoke();
        }
    }

    public void OnMissile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onMissile.Invoke();
        }
    }

    public void OnUnpause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onUnpause.Invoke();
        }
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onConfirmGameover.Invoke();
        }
    }
}