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

    private WaitForSeconds _waitForManualWaveDelay;
    private WaitForSeconds _waitForAutoWaveDelay;
    private WaitForSeconds _waitForSpawnNextEnemy;

    private int _accumulatedEnemyCount;
    private int _currentWaveIndex;
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
        if (_button != null)
        {
            _button.OnWaveRequested += StartWave;
        }
        else
        {
            Debug.LogError("EnemySpawner::Start() _button was not found!");
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
            Debug.LogWarning("EnemySpawner::SpawnWave() Wave list is empty!");
            _isSpawning = false;
            _button.IsBlocked = false;
            yield break;
        }

        EnemyWave currentWaveConfig = _listWaves[_currentWaveIndex];

        _accumulatedEnemyCount += currentWaveConfig.WaveDifficultyBonus;
        int hardEnemyCount = Mathf.RoundToInt(_accumulatedEnemyCount * currentWaveConfig.HardEnemySpawnRate);
        int baseEnemyCount = _accumulatedEnemyCount - hardEnemyCount;

        Debug.Log($"<color=yellow>EnemySpawner::SpawnWave() Starting Wave #{_currentWaveIndex + 1}</color>\n" +
                  $"Total Enemies: {_accumulatedEnemyCount} (Base: {baseEnemyCount}, Hard: {hardEnemyCount})");

        for (int i = 0; i < baseEnemyCount; i++)
        {
            Debug.Log($"EnemySpawner::SpawnWave() Spawned <color=green>base Enemy</color> ({i + 1}/{baseEnemyCount})");
            yield return _waitForSpawnNextEnemy;
        }

        for (int i = 0; i < hardEnemyCount; i++)
        {
            Debug.Log($"EnemySpawner::SpawnWave() Spawned <color=red>hard Enemy</color> ({i + 1}/{hardEnemyCount})");
            yield return _waitForSpawnNextEnemy;
        }

        Debug.Log($"<color=green>EnemySpawner::SpawnWave() Wave #{_currentWaveIndex + 1} completed!</color>");

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

    public void ResetSpawner()
    {
        _currentWaveIndex = 0;
        _accumulatedEnemyCount = _initialBaseEnemyCount;
        _isSpawning = false;
    }
}