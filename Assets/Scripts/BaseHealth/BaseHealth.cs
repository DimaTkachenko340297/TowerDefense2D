using System;

public class BaseHealth : IDisposable
{
    private readonly IEnemyBaseArrivalSource _enemyBaseArrivalSource;
    private int _currentHealth;

    public int CurrentHealth
    {
        get => _currentHealth;
        private set => _currentHealth = value;
    }

    // (int newHealthValue)
    public event Action<int> OnBaseHealthChanged;
    public event Action OnBaseDestroyed;

    public BaseHealth(IEnemyBaseArrivalSource enemyBaseArrivalSource, int initialHealth)
    {
        _enemyBaseArrivalSource = enemyBaseArrivalSource;
        _currentHealth = Math.Max(initialHealth, 1);

        _enemyBaseArrivalSource.OnEnemyReachedBase += TakeDamage;
    }

    public void Dispose()
    {
        _enemyBaseArrivalSource.OnEnemyReachedBase -= TakeDamage;
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