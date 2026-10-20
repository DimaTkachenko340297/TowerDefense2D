using System;
using UnityEngine;

public class ActiveProjectile : IDisposable
{
    public GameObject GameObject { get; private set; }
    public GameObject OriginPrefab { get; private set; }

    private Transform _transform;
    private Enemy _target;
    private Vector3 _lastKnownTargetPosition;
    private float _speed;
    private float _damage;

    private bool _arrived;

    public bool Setup(GameObject instance, GameObject originPrefab, Enemy target, float speed, float damage)
    {
        if (instance == null)
        {
            Debug.LogError("ActiveProjectile::Setup() GameObject instance is null!");
            return false;
        }

        if (speed <= 0f)
        {
            Debug.LogError($"ActiveProjectile::Setup() Speed must be greater than zero! Received: {speed}");
            return false;
        }

        if (damage < 0f)
        {
            Debug.LogWarning($"ActiveProjectile::Setup() Passed negative damage ({damage}). Clamping to 0.");
            damage = 0f;
        }

        if (!OriginPrefab)
        {
            OriginPrefab = originPrefab;
        }

        GameObject = instance;
        _transform = instance.transform;
        _target = target;
        _speed = speed;
        _damage = damage;

        if (_target != null)
        {
            _lastKnownTargetPosition = _target.transform.position;
            _target.OnDespawned += HandleTargetDespawned;
        }

        return true;
    }

    public bool Tick(float deltaTime)
    {
        if (_transform == null)
        {
            Debug.LogError("ActiveProjectile::Tick() _transform reference is missing!");
            return true;
        }

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            _lastKnownTargetPosition = _target.transform.position;
        }

        if (_arrived)
        {
            HitTarget();
            return true;
        }

        Vector3 direction = _lastKnownTargetPosition - _transform.position;
        float step = _speed * deltaTime;

        if (direction.sqrMagnitude <= step * step)
        {
            _transform.position = _lastKnownTargetPosition;
            _arrived = true;
            return false;
        }

        _transform.position += direction.normalized * step;
        _transform.right = direction;
        return false;
    }

    private void HitTarget()
    {
        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            _target.TakeDamage(_damage);
        }
    }

    private void HandleTargetDespawned(Enemy enemy)
    {
        if (_target != enemy)
        {
            return;
        }

        _lastKnownTargetPosition = enemy.transform.position;
        _target.OnDespawned -= HandleTargetDespawned;
        _target = null;
    }

    public void Dispose()
    {
        if (_target != null)
        {
            _target.OnDespawned -= HandleTargetDespawned;
            _target = null;
        }
    }
}