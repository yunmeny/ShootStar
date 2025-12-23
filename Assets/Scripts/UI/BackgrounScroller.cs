using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgrounScroller : MonoBehaviour
{
    private Material _material;

    [SerializeField] Vector2 scrollSpeed;
    
    void Awake()
    {
        _material = GetComponent<Renderer>().material;
    }

    private IEnumerator Start()
    {
        while (GameManager.GameState != GameState.GameOver)
        {
            _material.mainTextureOffset += scrollSpeed * Time.deltaTime;
            yield return null;
        }
    }


}
