using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

[System.Serializable]
public struct PoolConfig
{
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public int PrewarmCount { get; private set; }
}

[System.Serializable]
public struct PoolGroup
{
    [field: SerializeField] public string Header { get; private set; }
    [field: SerializeField] public List<PoolConfig> Configs { get; private set; }
}

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private List<PoolGroup> _poolGroups;

    private readonly Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, Transform> _groupRoots = new Dictionary<GameObject, Transform>();
    private IObjectResolver _resolver;

    [Inject]
    private void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        if (_poolGroups == null)
        {
            Debug.LogError("ObjectPool::InitializePools() _poolGroups reference is missing!");
            return;
        }

        for (int groupIndex = 0; groupIndex < _poolGroups.Count; groupIndex++)
        {
            PoolGroup group = _poolGroups[groupIndex];

            if (group.Configs == null)
            {
                Debug.LogWarning($"ObjectPool::InitializePools() Configs list in group at index {groupIndex} is missing!");
                continue;
            }

            Transform groupRoot = CreateGroupRoot(group.Header, groupIndex);

            foreach (PoolConfig config in group.Configs)
            {
                if (config.Prefab == null)
                {
                    Debug.LogWarning($"ObjectPool::InitializePools() Prefab reference in PoolConfig is null (group '{groupRoot.name}')!");
                    continue;
                }

                if (_poolDictionary.ContainsKey(config.Prefab))
                {
                    Debug.LogWarning($"ObjectPool::InitializePools() Duplicate entry found for prefab '{config.Prefab.name}' in pool configuration (group '{groupRoot.name}')!");
                    continue;
                }

                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < config.PrewarmCount; i++)
                {
                    GameObject instance = _resolver.Instantiate(config.Prefab, groupRoot);
                    instance.SetActive(false);
                    objectPool.Enqueue(instance);
                }

                _poolDictionary.Add(config.Prefab, objectPool);
                _groupRoots.Add(config.Prefab, groupRoot);
            }
        }
    }

    private Transform CreateGroupRoot(string header, int groupIndex)
    {
        string rootName = string.IsNullOrWhiteSpace(header) ? $"Group {groupIndex}" : header;

        Transform groupRoot = new GameObject(rootName).transform;
        groupRoot.SetParent(transform, false);

        return groupRoot;
    }

    public GameObject Get(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool::Get() Passed prefab parameter is null!");
            return null;
        }

        if (!_poolDictionary.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            Debug.LogWarning($"ObjectPool::Get() Prefab '{prefab.name}' was not found in _poolDictionary. Instantiating manually.");
            return CreateInstance(prefab);
        }

        if (pool.Count > 0)
        {
            GameObject poolObject = pool.Dequeue();
            poolObject.SetActive(true);
            return poolObject;
        }

        return CreateInstance(prefab);
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        Transform parent = _groupRoots.TryGetValue(prefab, out Transform groupRoot) ? groupRoot : transform;

        return _resolver.Instantiate(prefab, parent);
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