using UnityEngine;
using System;


public class TowerSelectionPanel : MonoBehaviour
{
    private TowerSelectButton[] _selectButtons;
    private TowerSelectButton _currentButton;
    public Action<GameObject> OnTowerSelected;
    private void Start()
    {
        _selectButtons = GetComponentsInChildren<TowerSelectButton>();
        if (_selectButtons == null) return;
        if (_selectButtons.Length == 0)
        {
            Debug.LogWarning("TowerSelectionPanel::Start() didn't find TowerSelectButtons in children");
            return;
        }

        foreach (TowerSelectButton button in _selectButtons)
            button.OnButtonSelected += HandleTowerSelection;
    }

    private void OnDisable()
    {
        if (_selectButtons == null || _selectButtons.Length == 0) return;

        foreach (TowerSelectButton button in _selectButtons)
            button.OnButtonSelected -= HandleTowerSelection;
    }

    private void HandleTowerSelection(TowerSelectButton button)
    {
        if (_currentButton == button)
        {
            DeselectTower();
            return;
        }
        
        if (_currentButton != null)
            _currentButton.IsSelected = false;

        _currentButton = button;
        _currentButton.IsSelected = true;
        OnTowerSelected?.Invoke(_currentButton.TowerPrefab);
    }

    public void DeselectTower()
    {
        if (_currentButton == null) return;

        _currentButton.IsSelected = false;
        _currentButton = null;
    }
}
