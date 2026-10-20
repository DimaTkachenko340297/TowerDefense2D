using System.Collections.Generic;
using UnityEngine;
using VContainer;


public class ProjectileManager : MonoBehaviour
{

    private ObjectPool _objectPool;
    //If the new projectile type is implemented,
    //we will refactor the _active Projectiles list from List<Active Projectile> to List<IProjectile>.
    private readonly List<ActiveProjectile> _activeProjectiles = new List<ActiveProjectile>();

    [Inject]
    private void Construct(ObjectPool objectPool)
    {
        _objectPool = objectPool;
    }

    private void Awake()
    {
        if (_objectPool == null)
        {
            Debug.LogError("ProjectileManager::Awake() _objectPool reference is missing!");
        }
    }

    public void SpawnProjectile(GameObject prefab, Vector3 spawnPosition, Enemy target, float speed, float damage)
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
        if (!projectile.Setup(instance, prefab, target, speed, damage))
        {
            _objectPool.Release(prefab, instance);
            return;
        }

        _activeProjectiles.Add(projectile);
    }

    private void DespawnProjectile(int index, ActiveProjectile projectile)
    {
        _activeProjectiles.RemoveAt(index);
        projectile.Dispose();

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
