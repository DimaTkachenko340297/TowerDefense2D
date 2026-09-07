using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }
    [SerializeField] private int _initialGold = 100;
    
    private int _currentGold;
    public int CurrentGold => _currentGold;

    // (int newValue)
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GoldManager::Awake() Duplicate instance detected! Destroying component.");
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _currentGold = _initialGold;
        OnGoldChanged?.Invoke(_currentGold);
    }

    public bool AddGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogError($"GoldManager::AddGold() Attempt to add an invalid amount of gold: {amount}");
            return false;
        }

        _currentGold += amount;
        OnGoldChanged?.Invoke(_currentGold);
        return true;
    }

    public bool TakeGold(int amount)
    {
        amount = Mathf.Abs(amount);

        if (_currentGold < amount)
        {
            return false;
        }

        _currentGold -= amount;
        OnGoldChanged?.Invoke(_currentGold);
        return true;
    }

    public bool HasEnoughGold(int amount)
    {
        return amount > 0 && _currentGold >= amount;
    }
}
