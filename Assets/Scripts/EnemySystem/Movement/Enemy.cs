using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private GameObject _originPrefab;
    public GameObject OriginPrefab
    {
        get => _originPrefab;
        set
        {
            if (_originPrefab == null && value != null)
            {
                _originPrefab = value;
            }
        }
    }
    // (Enemy thisEnemy)
    public event Action<Enemy> OnDied;

    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;
    
    [SerializeField, Min(0.1f)] private float _moveSpeed = 3f;

    public float MoveSpeed => _moveSpeed;
    private int _currentWaypointIndex;
    public int CurrentWaypointIndex { 
        get =>_currentWaypointIndex; 
        set 
        {
            _currentWaypointIndex = Mathf.Max(value, 0);
        } 
    }
    private float _lerpProgress;
    public float LerpProgress {
        get => _lerpProgress;
        set
        {
            _lerpProgress = Mathf.Clamp01(value);
        } 
    }

    public float GetTotalProgress() 
    { 
        return CurrentWaypointIndex + LerpProgress;
    }

    public void ResetState()
    {
        CurrentWaypointIndex = 0;
        LerpProgress = 0f;
        _currentHealth = _maxHealth;
        OnDied = null;
    }

    public void TakeDamage(float amount)
    {
        if (_currentHealth <= 0f)
        {
            return;
        }

        _currentHealth -= amount;

        if (_currentHealth <= 0f)
        {
            _currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke(this);
    }
}