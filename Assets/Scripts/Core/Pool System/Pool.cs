using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable] //由于该类未继承MonoBehaviour 使用此才可将其中序列化字段曝露出来 其表示可序列化
public class Pool
{
    public GameObject Prefab => prefab;
    public int Size => size;
    public int RuntimeSize => _queue.Count;
    
    private Transform parent;
    private Queue<GameObject> _queue;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int size = 1;

    /// <summary>
    /// 初始化对象池 根据size生成对应数量的游戏对象
    /// </summary>
    /// <param name="parent">
    /// <para>父级对象</para>
    /// </param>
    public void Initialize(Transform parent)
    {
        this.parent = parent;
        _queue = new Queue<GameObject>(size);
        for (int i = 0; i < size; i++)
        {
            _queue.Enqueue(Copy());
        }
    }

    /// <summary>
    /// 创造对象
    /// </summary>
    /// <returns></returns>
    GameObject Copy()
    {
        var copy = GameObject.Instantiate(prefab, parent); // 生成复制对象备用
        copy.SetActive(false); // 先将生成的对象禁用 以便后续启用
        return copy;
    }

    /// <summary>
    /// 取出可用对象
    /// </summary>
    /// <returns></returns>
    GameObject AvailableObject()
    {
        GameObject availableGameObject;
        if (_queue.Count > 0 && !_queue.Peek().activeSelf)
        {
            availableGameObject = _queue.Dequeue();
        }
        else
        {
            availableGameObject = Copy();
        }

        _queue.Enqueue(availableGameObject);
        return availableGameObject;
    }

    /// <summary>
    /// 启用可用对象
    /// </summary>
    /// <returns></returns>
    public GameObject PrepareObject()
    {
        GameObject prepareObject = AvailableObject();
        prepareObject.SetActive(true);
        return prepareObject;
    }

    /// <summary>
    /// 启用可用对象
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject PrepareObject(Vector3 position)
    {
        GameObject prepareObject = AvailableObject();
        prepareObject.SetActive(true);
        prepareObject.transform.position = position;
        return prepareObject;
    }

    /// <summary>
    /// 启用可用对象
    /// </summary>
    /// <param name="position"></param>
    /// <param name="quaternion"></param>
    /// <returns></returns>
    public GameObject PrepareObject(Vector3 position, Quaternion quaternion)
    {
        GameObject prepareObject = AvailableObject();
        prepareObject.SetActive(true);
        prepareObject.transform.position = position;
        prepareObject.transform.rotation = quaternion;
        return prepareObject;
    }

    /// <summary>
    /// 启用可用对象
    /// </summary>
    /// <param name="position"></param>
    /// <param name="quaternion"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public GameObject PrepareObject(Vector3 position, Quaternion quaternion, Vector3 scale)
    {
        GameObject prepareObject = AvailableObject();
        prepareObject.SetActive(true);
        prepareObject.transform.position = position;
        prepareObject.transform.rotation = quaternion;
        prepareObject.transform.localScale = scale;
        return prepareObject;
    }

    // 返回对象池
    // public void Return(GameObject gameObject)
    // {
    //     _queue.Enqueue(gameObject);
    // }
}