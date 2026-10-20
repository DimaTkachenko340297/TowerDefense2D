using System;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(TowerTargeting))]
public class Tower : MonoBehaviour
{
    // (Tower thisTower)
    public Action<Tower> OnUnregisterTower;
    [SerializeField] private float _fireRate = 1f;

    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 15f;
    [SerializeField] private float _damage = 25f;

    private TowerTargeting _targeting;
    private ProjectileManager _projectileManager;
    private float _nextFireTime;

    [Inject]
    private void Construct(ProjectileManager projectileManager)
    {
        _projectileManager = projectileManager;
    }

    private void Awake()
    {
        _targeting = GetComponent<TowerTargeting>();
        if (_targeting == null)
        {
            Debug.LogError("Tower::Awake() _targeting component is missing!");
        }

        _nextFireTime = Time.time;
    }

    private void OnDisable()
    {
        OnUnregisterTower?.Invoke(this);
    }

    public void Tick(float currentTime)
    {
        if (currentTime < _nextFireTime) 
        {
            return;
        }

        Enemy target = _targeting.GetFirstTarget();
        if (target != null)
        {
            Shoot(target);
            _nextFireTime = currentTime + _fireRate;
        }
    }

    private void Shoot(Enemy target)
    {
        if (target == null)
        {
            Debug.LogWarning("Tower::Shoot() Passed target enemy is null!");
            return;
        }

        if (_projectilePrefab == null)
        {
            Debug.LogError("Tower::Shoot() _projectilePrefab reference is missing!");
            target.TakeDamage(_damage); 
            return;
        }

        if (_projectileManager == null)
        {
            Debug.LogError("Tower::Shoot() ProjectileManager was not injected!");
            return;
        }

        _projectileManager.SpawnProjectile(
            _projectilePrefab, 
            transform.position, 
            target, 
            _projectileSpeed, 
            _damage
        );
    }
}