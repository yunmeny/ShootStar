using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = this as T;
    }

    // protected virtual void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this as T;
    //     }
    //     else
    //     {
    //         // 如果已经存在实例，则销毁当前实例
    //         Destroy(this);
    //     }
    // }
}