using Fusion;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (NetworkSingleton)";

                        // 네트워크 관련 싱글턴은 DontDestroyOnLoad 해도 씬마다 새로 생성하는 게 보통이라
                        // 필요에 따라 조절 가능
                    }
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // 필요하면 DontDestroyOnLoad(gameObject); 추가
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
