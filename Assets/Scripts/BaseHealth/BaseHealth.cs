using System;
using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int _initializeHealth;
    private int _currentHealth;
    public int CurrentHealth => _currentHealth;

    // (int newHealthValue)
    public Action<int> OnBaseHealthChanged;
    public Action OnBaseDestroyed;

    private void Start()
    {
        _currentHealth = _initializeHealth;
        OnBaseHealthChanged?.Invoke(_currentHealth);

        if (EnemyMovementManager.Instance == null)
        {
            Debug.LogError("BaseHealth::Start() EnemyMovementManager.Instance reference is missing!");
            Destroy(this);
            return;
        }

        EnemyMovementManager.Instance.OnEnemyReachedBase += TakeDamage;
    }

    private void OnDisable()
    {
        if (EnemyMovementManager.Instance != null)
        {
            EnemyMovementManager.Instance.OnEnemyReachedBase -= TakeDamage;
        }
    }

    private void TakeDamage()
    {
        if (_currentHealth <= 0)
        {
            return;
        }

        _currentHealth -= 1;
        OnBaseHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            OnBaseDestroyed?.Invoke();
        }
    }
}
