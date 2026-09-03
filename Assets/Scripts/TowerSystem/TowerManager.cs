using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ObjectPool))]
public class TowerManager : MonoBehaviour
{
    [SerializeField] private BuildingManager _buildingManager;
    private readonly List<Tower> _awakeTowers = new List<Tower>();

    private void Start()
    {
        if (_buildingManager == null)
        {
            Debug.LogError("TowerManager::Start() BuildingManager reference is missing!");
            return;
        }
        
        _buildingManager.OnTowerPlaced += RegisterTower;
    }

    private void RegisterTower(Tower tower)
    {
        if (tower == null)
        {
            Debug.LogError("TowerManager::RegisterTower() Passed tower is null!");
            return;
        }

        if (!tower.TryGetComponent<TowerTargeting>(out var towerTargeting))
        {
            Debug.LogError("TowerManager::RegisterTower() TowerTargeting component missing on registered tower!");
            return;
        }

        towerTargeting.OnBecameActive += ActivateTower;
        towerTargeting.OnBecameInactive += InactiveTower;

        tower.OnUnregisterTower += UnregisterTower;
    }

    private void UnregisterTower(Tower tower)
    {
        if (tower == null)
        {
            return;
        }

        if (tower.TryGetComponent<TowerTargeting>(out var towerTargeting))
        {
            towerTargeting.OnBecameActive -= ActivateTower;
            towerTargeting.OnBecameInactive -= InactiveTower;
        }
        else
        {
            Debug.LogWarning("TowerManager::UnregisterTower() TowerTargeting component missing during unregistration!");
        }

        InactiveTower(tower);

        tower.OnUnregisterTower -= UnregisterTower;
    }

    private void ActivateTower(Tower tower)
    {
        if (tower == null)
        {
            Debug.LogWarning("TowerManager::ActivateTower() Attempted to activate a null tower!");
            return;
        }

        if (!_awakeTowers.Contains(tower))
        {
            _awakeTowers.Add(tower);
        }
    }

    private void InactiveTower(Tower tower)
    {
        if (_awakeTowers.Contains(tower))
        {
            _awakeTowers.Remove(tower);
        }
    }

    private void Update()
    {
        float currentTime = Time.time;

        for (int i = _awakeTowers.Count - 1; i >= 0; i--)
        {
            _awakeTowers[i].Tick(currentTime);
        }
    }
}