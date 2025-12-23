using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootSetting
{
    public GameObject lootPrefab;
    [Range(0, 100)] public float lootDropChance;

    public void SpawnLoot(Vector3 position)
    {
        if (Random.Range(0f, 100f) <= lootDropChance)
        {
            PoolManger.Release(lootPrefab, position);
        }
    }
}
