using UnityEngine;

public struct ActiveProjectile
{
    public GameObject GameObject { get; private set; }
    public GameObject OriginPrefab { get; private set; }

    private Transform _transform;
    private Enemy _target;
    private Vector3 _lastKnownTargetPosition;
    private float _speed;
    private float _damage;
    private float _hitRadiusSqr;

    public bool Setup(GameObject instance, GameObject originPrefab, Enemy target, float speed, float damage, float hitRadius)
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

        if (hitRadius <= 0f)
        {
            Debug.LogError($"ActiveProjectile::Setup() HitRadius must be greater than zero! Received: {hitRadius}");
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
        _hitRadiusSqr = hitRadius * hitRadius;

        if (_target != null)
        {
            _lastKnownTargetPosition = _target.transform.position;
        }

        return true;
    }

    public bool Tick(float deltaTime)
    {
        if (_transform == null)
        {
            Debug.LogError("ActiveProjectile::Tick() Transform reference is missing!");
            return true;
        }

        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            _lastKnownTargetPosition = _target.transform.position;
        }

        Vector3 currentPos = _transform.position;
        Vector3 direction = _lastKnownTargetPosition - currentPos;
        
        float distanceSqr = direction.sqrMagnitude;
        float moveDistance = _speed * deltaTime;

        if (distanceSqr <= _hitRadiusSqr || distanceSqr <= (moveDistance * moveDistance))
        {
            HitTarget();
            return true;
        }

        Vector3 moveVector = direction.normalized * moveDistance;
        _transform.position += moveVector;
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
}