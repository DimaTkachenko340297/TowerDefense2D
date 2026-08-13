using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct PoolConfig
{
    public GameObject Prefab;
    public int PrewarmCount;
}
public class ObjectPool : MonoBehaviour
{
    [SerializeField] private List<PoolConfig> _poolsConfig;

    private Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new();

    private void Awake()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        if (_poolsConfig == null)
            return;

        foreach(PoolConfig config in _poolsConfig)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < config.PrewarmCount; i++)
            {
                if (config.Prefab == null) 
                    continue;

                GameObject instance = Instantiate(config.Prefab, transform);
                instance.SetActive(false);
                objectPool.Enqueue(instance);
            }
            _poolDictionary.Add(config.Prefab, objectPool);
        }
    }

    public GameObject Get(GameObject prefab)
    {
        if (!_poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"ObjectPool::Get() didn't find prefab {prefab.name} in _poolDictionary");
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
            return;

        if (_poolDictionary.ContainsKey(prefab))
        {
            instance.SetActive(false);
            _poolDictionary[prefab].Enqueue(instance);
        }
        else
        {
            Debug.LogWarning($"ObjectPool::Release() trying to release untracked prefab {prefab.name}");
            Destroy(instance);
        }
    }
}
