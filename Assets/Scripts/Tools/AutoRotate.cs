using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 360f; // 旋转速度
    [SerializeField] Vector3 rotationAxis = Vector3.up; // 旋转轴
    
    private void OnEnable()
    {
        StartCoroutine(nameof(AutoRotateCoroutine));
    }

    private void OnDisable()
    {
        StopCoroutine(nameof(AutoRotateCoroutine));
    }

    IEnumerator AutoRotateCoroutine()
    {
        while (gameObject.activeSelf)
        {
            gameObject.transform.Rotate(rotationAxis * rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
