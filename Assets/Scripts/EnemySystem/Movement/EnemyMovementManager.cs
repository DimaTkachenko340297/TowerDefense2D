using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementManager : MonoBehaviour
{
    [SerializeField] private WaypointPath _path;
    [SerializeField] private ObjectPool _objectPool;

    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private Coroutine _movementCoroutine;

    public static EnemyMovementManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EnemyMovementManager::Awake() Duplicate instance detected! Destroying component.");
            Destroy(this);
            return;
        }

        Instance = this;

        if (_path == null)
        {
            Debug.LogError("EnemyMovementManager::Awake() WaypointPath reference is missing!");
        }

        if (_objectPool == null)
        {
            Debug.LogError("EnemyMovementManager::Awake() ObjectPool reference is missing!");
        }
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
            Debug.LogError("EnemyMovementManager::RegisterEnemy() Waypoint path is invalid or has insufficient points!");
            return;
        }

        enemy.ResetMovementData();
        enemy.transform.position = _path.GetPoint(0);

        if (!_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
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

        if (_activeEnemies.Remove(enemy))
        {
            if (_objectPool != null && enemy.OriginPrefab != null)
            {
                _objectPool.Release(enemy.OriginPrefab, enemy.gameObject);
            }
            else
            {
                Debug.LogWarning("EnemyMovementManager::UnregisterEnemy() ObjectPool or OriginPrefab is missing!");
                enemy.gameObject.SetActive(false);
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

                if (enemy == null || !enemy.gameObject.activeInHierarchy)
                {
                    _activeEnemies.RemoveAt(i);
                    continue;
                }

                int nextWaypointIndex = enemy.CurrentWaypointIndex + 1;

                if (nextWaypointIndex >= _path.PointCount)
                {
                    _activeEnemies.RemoveAt(i);

                    if (_objectPool != null && enemy.OriginPrefab != null)
                    {
                        _objectPool.Release(enemy.OriginPrefab, enemy.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("EnemyMovementManager::MoveEnemiesRoutine() ObjectPool or OriginPrefab reference is missing. Disabling gameObject manually.");
                        enemy.gameObject.SetActive(false);
                    }
                    continue;
                }

                Vector3 startPosition = _path.GetPoint(enemy.CurrentWaypointIndex);
                Vector3 targetPosition = _path.GetPoint(nextWaypointIndex);

                float segmentDistance = _path.GetSegmentDistance(enemy.CurrentWaypointIndex);

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
                    enemy.CurrentWaypointIndex++;
                    enemy.LerpProgress = 0f;
                }
            }

            yield return null;
        }

        _movementCoroutine = null;
    }
}