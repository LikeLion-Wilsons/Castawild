using Fusion;
using UnityEngine;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    private static bool _isReady = false;
    public bool IsInitialized => _instance != null && _isReady;
    public static T Instance
    {
        get
        {
            if (_instance != null && _isReady)
                return _instance;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        Debug.LogError($"[NetworkSingleton] {typeof(T).Name} not found in scene! Please add it to the scene with NetworkObject component.");
                        return null;
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
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        if (!_isReady)
        {
            _isReady = true;
            Debug.Log($"[NetworkSingleton<{typeof(T).Name}>] Spawned and ready.");
        }
    }
}
