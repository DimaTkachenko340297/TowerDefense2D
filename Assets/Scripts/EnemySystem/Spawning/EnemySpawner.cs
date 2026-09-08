using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnemyWave
{
    [field: SerializeField] public int WaveDifficultyBonus { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float HardEnemySpawnRate { get; private set; }
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool System")]
    [SerializeField] private ObjectPool _objectPool;

    [Header("Waves Systems")]
    [SerializeField] private List<EnemyWave> _listWaves;
    [SerializeField, Min(0f)] private float _timeBetweenSpawnNextEnemy = 0.5f;

    [Header("Timing Settings")]
    [SerializeField, Min(0f)] private float _manualWaveDelay = 1.5f;
    [SerializeField, Min(0f)] private float _autoWaveDelay = 3.5f;

    [Header("Base Difficulty Settings")]
    [SerializeField, Min(1)] private int _initialBaseEnemyCount = 10;

    [Space(2)]
    [SerializeField] private GameObject _baseEnemy;
    [SerializeField] private GameObject _hardEnemy;

    [Space(2)]
    [SerializeField] private NextWaveButton _button;

    // (int wavesCount)
    public Action<int> OnCompletedWavesCountChanged;

    private WaitForSeconds _waitForManualWaveDelay;
    private WaitForSeconds _waitForAutoWaveDelay;
    private WaitForSeconds _waitForSpawnNextEnemy;

    private int _accumulatedEnemyCount;
    private int _currentWaveIndex;
    private int _wavesCount = 0;
    public int WavesCount => _wavesCount;
    private bool _isSpawning;

    private void Awake()
    {
        _waitForManualWaveDelay = new WaitForSeconds(_manualWaveDelay);
        _waitForAutoWaveDelay = new WaitForSeconds(_autoWaveDelay);
        _waitForSpawnNextEnemy = new WaitForSeconds(_timeBetweenSpawnNextEnemy);

        ResetSpawner();
    }

    private void Start()
    {
        if (_objectPool == null)
        {
            Debug.LogError("EnemySpawner::Start() _objectPool reference was not assigned in Inspector!");
            gameObject.SetActive(false);
            return;
        }

        if (_button != null)
        {
            _button.OnWaveRequested += StartWave;
        }
        else
        {
            Debug.LogError("EnemySpawner::Start() _button reference was not found!");
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.OnWaveRequested -= StartWave;
        }
    }

    private void StartWave()
    {
        if (!_isSpawning)
        {
            StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        _isSpawning = true;
        _button.IsBlocked = true;

        if (_listWaves == null || _listWaves.Count == 0)
        {
            Debug.LogWarning("EnemySpawner::SpawnWave() _listWaves is empty or not initialized!");
            _isSpawning = false;
            _button.IsBlocked = false;
            yield break;
        }

        _wavesCount += 1;
        OnCompletedWavesCountChanged?.Invoke(_wavesCount);

        EnemyWave currentWaveConfig = _listWaves[_currentWaveIndex];

        _accumulatedEnemyCount += currentWaveConfig.WaveDifficultyBonus;
        _accumulatedEnemyCount = Mathf.Max(_accumulatedEnemyCount, 1);
        int hardEnemyCount = Mathf.RoundToInt(_accumulatedEnemyCount * currentWaveConfig.HardEnemySpawnRate);
        int baseEnemyCount = _accumulatedEnemyCount - hardEnemyCount;

        for (int i = 0; i < baseEnemyCount; i++)
        {
            SpawnSingleEnemy(_baseEnemy);
            yield return _waitForSpawnNextEnemy;
        }

        for (int i = 0; i < hardEnemyCount; i++)
        {
            SpawnSingleEnemy(_hardEnemy);
            yield return _waitForSpawnNextEnemy;
        }

        _currentWaveIndex = (_currentWaveIndex + 1) % _listWaves.Count;

        WaitForSeconds currentDelay = _button.IsAuto ? _waitForAutoWaveDelay : _waitForManualWaveDelay;

        yield return currentDelay;

        if (_button.IsAuto)
        {
            StartCoroutine(SpawnWave());
            yield break;
        }

        _isSpawning = false;
        _button.IsBlocked = false;
    }

    private void SpawnSingleEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner::SpawnSingleEnemy() enemyPrefab reference is null!");
            return;
        }

        if (_objectPool == null)
        {
            Debug.LogError("EnemySpawner::SpawnSingleEnemy() _objectPool reference is null!");
            return;
        }

        GameObject enemyInstance = _objectPool.Get(enemyPrefab);

        if (enemyInstance.TryGetComponent(out Enemy enemy))
        {
            enemy.OriginPrefab = enemyPrefab;

            if (EnemyMovementManager.Instance != null)
            {
                EnemyMovementManager.Instance.RegisterEnemy(enemy);
            }
            else
            {
                Debug.LogError("EnemySpawner::SpawnSingleEnemy() EnemyMovementManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogError("EnemySpawner::SpawnSingleEnemy() Spawned prefab is missing the Enemy component!");
        }
    }

    public void ResetSpawner()
    {
        _currentWaveIndex = 0;
        _wavesCount = 0;
        _accumulatedEnemyCount = _initialBaseEnemyCount;
        _isSpawning = false;
    }
}