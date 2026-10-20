using UnityEngine;
using System;
using VContainer;


public class TowerSelectionPanel : MonoBehaviour
{
    private TowerSelectButton[] _selectButtons;
    private TowerSelectButton _currentButton;
    private IGoldProvider _goldProvider;
    // (GameObject towerPrefabe, int cost)
    public Action<GameObject, int> OnTowerSelected;
    [Inject]
    private void Construct(IGoldProvider goldProvider)
    {
        _goldProvider = goldProvider;
    }

    private void Start()
    {
        _selectButtons = GetComponentsInChildren<TowerSelectButton>();
        if (_selectButtons == null) 
        {
            return;
        }

        if (_selectButtons.Length == 0)
        {
            Debug.LogWarning("TowerSelectionPanel::Start() didn't find _selectButtons in children");
            return;
        }

        foreach (TowerSelectButton button in _selectButtons)
        {
            button.OnButtonSelected += HandleTowerSelection;
        }

        if (_goldProvider != null)
        {
            _goldProvider.OnGoldChanged += RefreshButtonsAvailability;
            RefreshButtonsAvailability(_goldProvider.CurrentGold);
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

        if (_goldProvider != null)
        {
            _goldProvider.OnGoldChanged -= RefreshButtonsAvailability;
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
