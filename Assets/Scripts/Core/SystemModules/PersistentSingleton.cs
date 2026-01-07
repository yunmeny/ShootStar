using UnityEngine;

namespace Core.SystemModules
{
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this as T; // 这里的this是继承了MonoBehaviour的类，所以可以直接转换为T类型
                DontDestroyOnLoad(gameObject);
            }
        }

        protected static bool IsInstance()
        {
            return Instance != null;
        }
    }
}
    

