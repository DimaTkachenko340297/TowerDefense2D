using UnityEngine;
using System;


public class TowerSelectionPanel : MonoBehaviour
{
    private TowerSelectButton[] _selectButtons;
    private TowerSelectButton _currentButton;
    // (GameObject towerPrefabe, int cost)
    public Action<GameObject, int> OnTowerSelected;
    private void Start()
    {
        _selectButtons = GetComponentsInChildren<TowerSelectButton>();
        if (_selectButtons == null) 
        {
            return;
        }

        if (_selectButtons.Length == 0)
        {
            Debug.LogWarning("TowerSelectionPanel::Start() didn't find TowerSelectButtons in children");
            return;
        }

        foreach (TowerSelectButton button in _selectButtons)
        {
            button.OnButtonSelected += HandleTowerSelection;
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += RefreshButtonsAvailability;
            RefreshButtonsAvailability(GoldManager.Instance.CurrentGold);
        }
    }

    private void OnDisable()
    {
        if (_selectButtons == null || _selectButtons.Length == 0) 
        {
            return;
        }

        foreach (TowerSelectButton button in _selectButtons)
        {
            button.OnButtonSelected -= HandleTowerSelection;
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= RefreshButtonsAvailability;
        }
    }

    private void RefreshButtonsAvailability(int amount)
    {
        if (_selectButtons == null)
        {
            return;
        }

        foreach (TowerSelectButton button in _selectButtons)
        {
            bool canAfford;
            canAfford = button.SetHasEnoughGold(amount);

            if (!canAfford && button.IsSelected)
            {
                DeselectTower();
            }
        }
    }

    private void HandleTowerSelection(TowerSelectButton button)
    {
        if (_currentButton == button)
        {
            DeselectTower();
            return;
        }
        
        if (_currentButton != null)
        {
            _currentButton.IsSelected = false;
        }
        _currentButton = button;
        _currentButton.IsSelected = true;
        OnTowerSelected?.Invoke(_currentButton.TowerPrefab, _currentButton.Cost);
    }

    public void DeselectTower()
    {
        if (_currentButton == null)
        { 
            return;
        }

        _currentButton.IsSelected = false;
        _currentButton = null;
    }
}
