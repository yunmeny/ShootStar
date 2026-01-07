using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PoolManger : MonoBehaviour
{
    [SerializeField] private Pool[] vfxPools;
    [SerializeField] private Pool[] playerProjectilePools; //  Pool类数组 代表一系列对象池
    [SerializeField] private Pool[] enemyProjectilePools;
    [SerializeField] private Pool[] enemyPools;
    [SerializeField] private Pool[] lootPools;
    static Dictionary<GameObject, Pool> _dictionary;

    private void Awake()
    {
        _dictionary = new Dictionary<GameObject, Pool>();
        Initialize(playerProjectilePools); // 初始化玩家子弹对象池
        Initialize(enemyProjectilePools); // 初始化敌人子弹对象池
        Initialize(vfxPools); // 初始化特效对象池
        Initialize(enemyPools); // 初始化敌人对象池
        Initialize(lootPools); // 初始化拾取物对象池
    }

#if UNITY_EDITOR
    private void OnDestroy()
    {
        CheckPoolSize(playerProjectilePools);
        CheckPoolSize(enemyProjectilePools);
        CheckPoolSize(vfxPools);
        CheckPoolSize(enemyPools);
        CheckPoolSize(lootPools);
    }
#endif


    /// <summary>
    /// 检查对象池的大小是否符合预期, 如果不符合预期则发出警告
    /// </summary>
    /// <param name="pools"></param>
    void CheckPoolSize(Pool[] pools)
    {
        foreach (var pool in pools)
        {
            if (pool.Size < pool.RuntimeSize)
            {
                Debug.LogWarning(string.Format(
                    "The runtime size {0} of pool: {1} are bigger than its initial size {2}!",
                    pool.RuntimeSize,
                    pool.Prefab.name,
                    pool.Size));
            }
        }
    }

    /// <summary>
    /// 初始化对象池 内部包含了父子级对象处理 
    /// </summary>
    /// <param name="pools">
    /// <para>Pool类型的数组</para>
    /// </param>
    void Initialize(IEnumerable<Pool> pools)
    {
        foreach (var pool in pools)
        {
#if UNITY_EDITOR
            if (_dictionary.ContainsKey(pool.Prefab))
            {
                Debug.LogError("Find the same Key(prefab) in Pools: " + pool.Prefab.name);
                continue;
            }
#endif
            _dictionary.Add(pool.Prefab, pool);
            var poolParent = new GameObject("Pool: " + pool.Prefab.name).transform;
            poolParent.parent = transform; //将对象池的父级对先挂载到PoolManager对象上
            pool.Initialize(poolParent);
        }
    }

    /// <summary>
    /// <para>根据传入的 prefab 参数，返回对象池中的预备好的对象</para>
    /// </summary>
    /// <param name="prefab">
    /// <para>指定对象的预制体</para>
    /// </param>
    /// <returns>
    /// <para>对象池中的预备对象</para>
    /// </returns>
    public static GameObject Release(GameObject prefab)
    {
#if UNITY_EDITOR
        if (!_dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager can't find the prefab: " + prefab.name);
            return null;
        }
#endif
        return _dictionary[prefab].PrepareObject();
    }

    /// <summary>
    /// 根据传入的 position 参数，在该位置上释放准备好的游戏对象
    /// </summary>
    /// <param name="prefab">指定对象的预制体</param>
    /// <param name="position">指定释放位置</param>
    /// <returns>对象池中的预备对象</returns>
    public static GameObject Release(GameObject prefab, Vector3 position)
    {
#if UNITY_EDITOR
        if (!_dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager can't find the prefab: " + prefab.name);
            return null;
        }
#endif
        return _dictionary[prefab].PrepareObject(position);
    }

    /// <summary>
    /// 根据传入的 position quaternion 参数，在该位置上释放准备好的对应旋转角度的游戏对象
    /// </summary>
    /// <param name="prefab">指定对象的预制体</param>
    /// <param name="position">指定释放位置</param>
    /// <param name="quaternion">指定的旋转值</param>
    /// <returns>对象池中的预备对象</returns>
    public static GameObject Release(GameObject prefab, Vector3 position, Quaternion quaternion)
    {
#if UNITY_EDITOR
        if (!_dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager can't find the prefab: " + prefab.name);
            return null;
        }
#endif
        return _dictionary[prefab].PrepareObject(position, quaternion);
    }

    /// <summary>
    /// 根据传入的 position quaternion scale 参数，在该位置上释放准备好的对应旋转角度和缩放大小的游戏对象
    /// </summary>
    /// <param name="prefab">指定对象的预制体</param>
    /// <param name="position">指定释放位置</param>
    /// <param name="quaternion">指定的旋转值</param>
    /// <param name="scale">指定的缩放值</param>
    /// <returns>对象池中的预备对象</returns>
    public static GameObject Release(GameObject prefab, Vector3 position, Quaternion quaternion, Vector3 scale)
    {
#if UNITY_EDITOR
        if (!_dictionary.ContainsKey(prefab))
        {
            Debug.LogError("Pool Manager can't find the prefab: " + prefab.name);
            return null;
        }
#endif
        return _dictionary[prefab].PrepareObject(position, quaternion, scale);
    }
}