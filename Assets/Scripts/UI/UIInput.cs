using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIInput : Singleton<UIInput>
{
    [SerializeField] PlayerInput playerInput;
    
    InputSystemUIInputModule UIInputModule;
    
 
    protected override void Awake()
    {
        base.Awake();
        UIInputModule = GetComponent<InputSystemUIInputModule>();
        UIInputModule.enabled = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public static InputActionAsset DeepCopy(InputActionAsset original)
    {
        if (original == null)
        {
            return null;
        }

        // 序列化原始的 InputActionAsset 为 JSON 字符串
        string json = original.ToJson();

        // 反序列化 JSON 字符串为新的 InputActionAsset 对象
        InputActionAsset copy = ScriptableObject.CreateInstance<InputActionAsset>();
        copy.LoadFromJson(json);

        return copy;
    }

    public void SelectUI(Selectable uiObject)
    {
        uiObject.Select();
        uiObject.OnSelect(null);
        
        UIInputModule.enabled = true;
    }
    

    public void DisableAllUIInput()
    {
        playerInput.DisableAllInput();
        // UIInputModule.enabled = false;
        StopCoroutine(nameof(WaitForFrame));
        
    }

    IEnumerator WaitForFrame()
    {
        yield return null;
        UIInputModule.enabled = false;
    }
}
