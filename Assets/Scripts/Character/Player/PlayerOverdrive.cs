using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PlayerOverdrive : MonoBehaviour
{
    [SerializeField] private GameObject triggerVFX;
    [SerializeField] private GameObject engineVFXNormal;
    [SerializeField] private GameObject engineVFXOverdrive;

    [SerializeField] private AudioData onSFX;
    [SerializeField] private AudioData offSFX;
    
    // 为了方便随时移除和添加该功能，以及方便代码复用，我们将其做成委托
    public static UnityAction on = delegate { };
    public static UnityAction off = delegate { };

    private void Awake()
    {
        on += On;
        off += Off;
    }

    void OnDestroy()
    {
        on -= On;
        off -= Off;
    }

    void On()
    {
        triggerVFX.SetActive(true);
        engineVFXNormal.SetActive(false);
        engineVFXOverdrive.SetActive(true);
        AudioManager.Instance.PlayRandomSFX(onSFX);
    }

    void Off()
    {
        engineVFXNormal.SetActive(true);
        engineVFXOverdrive.SetActive(false);
        AudioManager.Instance.PlayRandomSFX(offSFX);
    }
}
