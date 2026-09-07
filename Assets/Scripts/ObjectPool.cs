using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PoolConfig
{
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public int PrewarmCount { get; private set; }
}

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private List<PoolConfig> _poolsConfig;

    private readonly Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        if (_poolsConfig == null)
        {
            Debug.LogError("ObjectPool::InitializePools() _poolsConfig reference is missing!");
            return;
        }

        foreach (PoolConfig config in _poolsConfig)
        {
            if (config.Prefab == null)
            {
                Debug.LogWarning("ObjectPool::InitializePools() Prefab reference in PoolConfig is null!");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < config.PrewarmCount; i++)
            {
                GameObject instance = Instantiate(config.Prefab, transform);
                instance.SetActive(false);
                objectPool.Enqueue(instance);
            }

            if (!_poolDictionary.ContainsKey(config.Prefab))
            {
                _poolDictionary.Add(config.Prefab, objectPool);
            }
            else
            {
                Debug.LogWarning($"ObjectPool::InitializePools() Duplicate entry found for prefab '{config.Prefab.name}' in pool configuration!");
            }
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool::Get() Passed prefab parameter is null!");
            return null;
        }

        if (!_poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"ObjectPool::Get() Prefab '{prefab.name}' was not found in _poolDictionary. Instantiating manually.");
            return Instantiate(prefab);
        }

        Queue<GameObject> pool = _poolDictionary[prefab];

        if (pool.Count > 0)
        {
            GameObject poolObject = pool.Dequeue();
            poolObject.SetActive(true);
            return poolObject;
        }
        else
        {
            return Instantiate(prefab, transform);
        }
    }

    public void Release(GameObject prefab, GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogWarning("ObjectPool::Release() Passed instance parameter is null!");
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning("ObjectPool::Release() Passed prefab key is null! Destroying instance manually.");
            Destroy(instance);
            return;
        }

        if (_poolDictionary.ContainsKey(prefab))
        {
            instance.SetActive(false);
            _poolDictionary[prefab].Enqueue(instance);
        }
        else
        {
            Debug.LogWarning($"ObjectPool::Release() Attempted to release untracked prefab '{prefab.name}'! Destroying instance manually.");
            Destroy(instance);
        }
    }
}