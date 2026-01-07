using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
    [SerializeField] private LootSetting[] lootSetting;


    public void Spawn(Vector2 position)
    {
        foreach (var loot in lootSetting)
        {
            loot.SpawnLoot(position + Random.insideUnitCircle);
        }
    }
}
