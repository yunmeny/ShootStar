using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Viewport : Singleton<Viewport>
{
    private float minX, minY, maxX, maxY, middleX;
    
    public float MinX => minX;
    public float MinY => minY;
    public float MaxX => maxX;
    public float MaxY => maxY;
    public float MiddleX => middleX;
    
    private void Start()
    {
        var mainCamera = Camera.main;
        Vector2 bottomLeft = mainCamera.ViewportToWorldPoint(Vector3.zero); //视口坐标转换为世界坐标
        Vector2 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1));
        Vector2 middle = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f));
        minX = bottomLeft.x;
        minY = bottomLeft.y;
        maxX = topRight.x;
        maxY = topRight.y;
        middleX = middle.x;
        // Debug.Log(maxX);
    }

    public Vector3 PlayerMoveablePosition(Vector3 playerPosition, float paddingX, float paddingY)
    {
        var position = Vector3.zero;
        position.x = Mathf.Clamp(playerPosition.x, minX + paddingX, maxX - paddingX); //限定移动范围
        position.y = Mathf.Clamp(playerPosition.y, minY + paddingY, maxY - paddingY);
        return position;
    }

    public Vector3 RandomEnemySpawnPosition(float paddingX, float paddingY)
    {
        Vector3 position = Vector3.zero;
        position.x = maxX + paddingX;
        position.y = UnityEngine.Random.Range(minY + paddingY, maxY - paddingY);
        return position;
    }

    public Vector3 RandomRightHalfPosition(float paddingX, float paddingY)
    {
        Vector3 position = Vector3.zero;
        position.x = UnityEngine.Random.Range(middleX, maxX - paddingX);
        position.y = UnityEngine.Random.Range(minY + paddingY, maxY - paddingY);
        return position;
    }

    public Vector3 RandomEnemyPosition(float paddingX, float paddingY)
    {
        Vector3 position = Vector3.zero;
        position.x = UnityEngine.Random.Range(minX + paddingX, maxX - paddingX);
        position.y = UnityEngine.Random.Range(minY + paddingY, maxY - paddingY);
        return position;
    }
}