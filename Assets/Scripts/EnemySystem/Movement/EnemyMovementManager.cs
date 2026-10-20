using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class EnemyMovementManager : MonoBehaviour, IEnemyRegistry, IEnemyRouteTracker, IEnemyBaseArrivalSource
{
    [SerializeField] private WaypointPath _path;
    public WaypointPath Path => _path;
    private ObjectPool _objectPool;
    private IGoldWallet _goldWallet;

    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;
    private List<Enemy>[] _enemiesPerSegment;
    private Coroutine _movementCoroutine;
    
    // (int activatedSegmentIndex)
    public event Action<int> OnSegmentActivated;
    // (int deactivatedSegmentIndex)
    public event Action<int> OnSegmentDeactivated;

    public event Action OnEnemyReachedBase;

    [Inject]
    private void Construct(IGoldWallet goldWallet, ObjectPool objectPool)
    {
        _goldWallet = goldWallet;
        _objectPool = objectPool;
    }

    private void Awake()
    {
        if (_path == null)
        {
            Debug.LogError("EnemyMovementManager::Awake() _path reference is missing!");
        }

        if (_objectPool == null)
        {
            Debug.LogError("EnemyMovementManager::Awake() _objectPool reference is missing!");
        }

        InitializeSegments();
    }

    private void InitializeSegments()
    {
        if (_path == null || _path.PointCount < 2)
        {
            _enemiesPerSegment = Array.Empty<List<Enemy>>();
            return;
        }

        int segmentCount = _path.PointCount - 1;
        _enemiesPerSegment = new List<Enemy>[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            _enemiesPerSegment[i] = new List<Enemy>();
        }
    }

    public IReadOnlyList<Enemy> GetEnemiesOnSegment(int segmentIndex)
    {
        if (segmentIndex < 0 || segmentIndex >= _enemiesPerSegment.Length)
        {
            return null;
        }

        return _enemiesPerSegment[segmentIndex];
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("EnemyMovementManager::RegisterEnemy() Passed enemy component is null!");
            return;
        }

        if (_path == null || _path.PointCount < 2)
        {
            Debug.LogError("EnemyMovementManager::RegisterEnemy() _path is invalid or has insufficient points!");
            return;
        }

        enemy.ResetState();
        enemy.transform.position = _path.GetPoint(0);

        enemy.OnDied += UnregisterEnemy;

        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            _enemiesPerSegment[0].Add(enemy);

            if (_enemiesPerSegment[0].Count == 1)
            {
                OnSegmentActivated?.Invoke(0);
            }
        }

        if (_movementCoroutine == null)
        {
            _movementCoroutine = StartCoroutine(MoveEnemiesRoutine());
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        enemy.OnDied -= UnregisterEnemy;

        _goldWallet.AddGold(enemy.GoldPerKill);

        if (_activeEnemies.Remove(enemy))
        {
            enemy.NotifyDespawned();

            if (_objectPool != null && enemy.OriginPrefab != null)
            {
                _objectPool.Release(enemy.OriginPrefab, enemy.gameObject);
            }
            else
            {
                Debug.LogWarning("EnemyMovementManager::UnregisterEnemy() _objectPool or enemy.OriginPrefab is missing!");
                enemy.gameObject.SetActive(false);
            }
        }

        int segment = enemy.CurrentWaypointIndex;
        if (segment >= 0 && segment < _enemiesPerSegment.Length && _enemiesPerSegment[segment].Remove(enemy))
        {
            if (_enemiesPerSegment[segment].Count == 0)
            {
                OnSegmentDeactivated?.Invoke(segment);
            }
        }
    }

    private IEnumerator MoveEnemiesRoutine()
    {
        while (_activeEnemies.Count > 0)
        {
            float deltaTime = Time.deltaTime;

            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _activeEnemies[i];
                if (enemy == null)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                if (!enemy.gameObject.activeInHierarchy)
                {
                    enemy.OnDied -= UnregisterEnemy;
                    enemy.NotifyDespawned();
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                int currentSegment = enemy.CurrentWaypointIndex;
                if (currentSegment >= _path.PointCount - 1)
                {
                    DespawnEnemy(i, enemy);
                    continue;
                }

                Vector3 startPosition = _path.GetPoint(currentSegment);
                Vector3 targetPosition = _path.GetPoint(currentSegment + 1);

                float segmentDistance = _path.GetSegmentDistance(currentSegment);

                if (segmentDistance > 0.0001f)
                {
                    enemy.LerpProgress += (enemy.MoveSpeed * deltaTime) / segmentDistance;
                }
                else
                {
                    enemy.LerpProgress = 1f;
                }

                enemy.transform.position = Vector3.Lerp(startPosition, targetPosition, enemy.LerpProgress);

                if (enemy.LerpProgress >= 1f)
                {
                    _enemiesPerSegment[currentSegment].Remove(enemy);

                    if (_enemiesPerSegment[currentSegment].Count == 0)
                    {
                        OnSegmentDeactivated?.Invoke(currentSegment);
                    }

                    enemy.CurrentWaypointIndex++;
                    enemy.LerpProgress = 0f;

                    if (enemy.CurrentWaypointIndex < _path.PointCount - 1)
                    {
                        _enemiesPerSegment[enemy.CurrentWaypointIndex].Add(enemy);

                        if (_enemiesPerSegment[enemy.CurrentWaypointIndex].Count == 1)
                        {
                            OnSegmentActivated?.Invoke(enemy.CurrentWaypointIndex);
                        }
                    }
                    else
                    {
                        OnEnemyReachedBase?.Invoke();
                        DespawnEnemy(i, enemy);
                    }
                }
            }

            yield return null;
        }

        _movementCoroutine = null;
    }

    private void DespawnEnemy(int index, Enemy enemy)
    {
        _activeEnemies.RemoveAt(index);
        enemy.OnDied -= UnregisterEnemy;
        enemy.NotifyDespawned();

        int currentSegment = enemy.CurrentWaypointIndex;
        if (currentSegment < _enemiesPerSegment.Length && _enemiesPerSegment[currentSegment].Remove(enemy))
        {
            if (_enemiesPerSegment[currentSegment].Count == 0)
            {
                OnSegmentDeactivated?.Invoke(currentSegment);
            }
        }

        if (_objectPool != null && enemy.OriginPrefab != null)
        {
            _objectPool.Release(enemy.OriginPrefab, enemy.gameObject);
        }
        else
        {
            Debug.LogWarning("EnemyMovementManager::DespawnEnemy() _objectPool or enemy.OriginPrefab reference is missing. Disabling gameObject manually.");
            enemy.gameObject.SetActive(false);
        }
    }
}
