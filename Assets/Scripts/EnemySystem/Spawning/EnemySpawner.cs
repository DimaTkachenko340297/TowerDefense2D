using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

[System.Serializable]
public struct EnemyWave
{
    [field: SerializeField] public int WaveDifficultyBonus { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float HardEnemySpawnRate { get; private set; }
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool System")]
    private ObjectPool _objectPool;

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

    private CancellationTokenSource _spawnCancellationTokenSource;

    private int _accumulatedEnemyCount;
    private int _currentWaveIndex;
    private int _wavesCount = 0;
    public int WavesCount => _wavesCount;
    private bool _isSpawning;
    private IEnemyRegistry _enemyRegistry;

    [Inject]
    private void Construct(IEnemyRegistry enemyRegistry, ObjectPool objectPool)
    {
        _enemyRegistry = enemyRegistry;
        _objectPool = objectPool;
    }

    private void Awake()
    {
ResetSpawner();
    }

    private void Start()
    {
        if (_objectPool == null)
        {
            Debug.LogError("EnemySpawner::Start() ObjectPool was not injected!");
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
            _button.IsBlocked = false;
        }

        CancelSpawning();
    }

    private void StartWave()
    {
        if (_isSpawning)
        {
            return;
        }

        _spawnCancellationTokenSource = new CancellationTokenSource();
        SpawnWaveAsync(_spawnCancellationTokenSource.Token).Forget();
    }

    private async UniTask SpawnWaveAsync(CancellationToken cancellationToken)
    {
        _isSpawning = true;
        if (_button != null)
        {
            _button.IsBlocked = true;
        }

        try
        {
            if (_listWaves == null || _listWaves.Count == 0)
            {
                Debug.LogWarning("EnemySpawner::SpawnWaveAsync() _listWaves is empty or not initialized!");
                return;
            }

            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                _wavesCount += 1;
                OnCompletedWavesCountChanged?.Invoke(_wavesCount);

                EnemyWave currentWaveConfig = _listWaves[_currentWaveIndex];
                _accumulatedEnemyCount += currentWaveConfig.WaveDifficultyBonus;
                _accumulatedEnemyCount = Mathf.Max(_accumulatedEnemyCount, 1);
                int hardEnemyCount = Mathf.RoundToInt(_accumulatedEnemyCount * currentWaveConfig.HardEnemySpawnRate);
                int baseEnemyCount = _accumulatedEnemyCount - hardEnemyCount;

                for (int i = 0; i < baseEnemyCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SpawnSingleEnemy(_baseEnemy);
                    await DelayOrYieldAsync(_timeBetweenSpawnNextEnemy, cancellationToken);
                }

                for (int i = 0; i < hardEnemyCount; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SpawnSingleEnemy(_hardEnemy);
                    await DelayOrYieldAsync(_timeBetweenSpawnNextEnemy, cancellationToken);
                }

                _currentWaveIndex = (_currentWaveIndex + 1) % _listWaves.Count;
                float waveDelay = _button.IsAuto ? _autoWaveDelay : _manualWaveDelay;
                await DelayOrYieldAsync(waveDelay, cancellationToken);
            }
            while (_button != null && _button.IsAuto);
        }
        catch (OperationCanceledException) { }

        finally
        {
            _isSpawning = false;
            if (_button != null)
            {
                _button.IsBlocked = false;
            }

            if (_spawnCancellationTokenSource != null && _spawnCancellationTokenSource.Token == cancellationToken)
            {
                _spawnCancellationTokenSource.Dispose();
                _spawnCancellationTokenSource = null;
            }
        }
    }

    private static UniTask DelayOrYieldAsync(float seconds, CancellationToken cancellationToken)
    {
        return seconds <= 0f
            ? UniTask.NextFrame(cancellationToken)
            : UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
    }
    private void CancelSpawning()
    {
        if (_spawnCancellationTokenSource == null)
        {
            return;
        }

        _spawnCancellationTokenSource.Cancel();
        _spawnCancellationTokenSource.Dispose();
        _spawnCancellationTokenSource = null;
        _isSpawning = false;
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

            if (_enemyRegistry == null)
            {
                Debug.LogError("EnemySpawner::SpawnSingleEnemy() IEnemyRegistry was not injected!");
                return;
            }

            _enemyRegistry.RegisterEnemy(enemy);
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