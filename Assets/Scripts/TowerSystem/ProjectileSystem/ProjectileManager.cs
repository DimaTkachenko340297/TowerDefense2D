using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(ObjectPool))]
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [SerializeField] private ObjectPool _objectPool;
    private readonly List<ActiveProjectile> _activeProjectiles = new List<ActiveProjectile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (_objectPool == null)
        {
            Debug.LogError("ProjectileManager::Awake() _objectPool reference is missing!");
        }
    }

    public void SpawnProjectile(GameObject prefab, Vector3 spawnPosition, Enemy target, float speed, float damage, float hitRadius = 0.2f)
    {
        if (prefab == null || target == null || _objectPool == null)
        {
            Debug.LogError("ProjectileManager::SpawnProjectile() Invalid parameters or ObjectPool is null!");
            return;
        }

        GameObject instance = _objectPool.Get(prefab);
        instance.transform.position = spawnPosition;
        instance.transform.rotation = Quaternion.identity;
        
        ActiveProjectile projectile = new ActiveProjectile();
        projectile.Setup(instance, prefab, target, speed, damage, hitRadius);

        _activeProjectiles.Add(projectile);
    }

    private void DespawnProjectile(int index, ActiveProjectile projectile)
    {
        _activeProjectiles.RemoveAt(index);

        if (_objectPool != null && projectile.OriginPrefab != null)
        {
            _objectPool.Release(projectile.OriginPrefab, projectile.GameObject);
        }
        else
        {
            projectile.GameObject.SetActive(false);
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
        {
            if (_activeProjectiles[i].Tick(deltaTime))
            {
                DespawnProjectile(i, _activeProjectiles[i]);
            }
        }
    }
}
