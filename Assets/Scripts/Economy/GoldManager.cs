using System;
using UnityEngine;

public class GoldManager : IGoldProvider, IGoldWallet
{
    private int _currentGold;
    public int CurrentGold => _currentGold;

    // (int newValue)
    public event Action<int> OnGoldChanged;

    public GoldManager(int initialGold)
    {
        _currentGold = initialGold;
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
