using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using VContainer;


public class BuildingManager : MonoBehaviour
{
    // (Tower placedTower)
    public event Action<Tower> OnTowerPlaced;
    [SerializeField] private Tilemap _placementMap;
    private TowerSelectionPanel _selectionPanel;

    private Dictionary<Vector3Int, GameObject> _occupiedTiles = new();
    private ObjectPool _objectPool;
    private Camera _mainCamera;
    private IGoldProvider _goldProvider;
    private IGoldWallet _goldWallet;

    [Header("Preview Tower")]
    [SerializeField] private GameObject _ghostTowerPrefab;
    private GameObject _ghostTower;
    private SpriteRenderer _ghostSpriteRenderer;
    private TowerPlacementPreview _ghostPlacementPreview;

    [Inject]
    private void Construct(IGoldProvider goldProvider, IGoldWallet goldWallet, ObjectPool objectPool, TowerSelectionPanel selectionPanel)
    {
        _goldProvider = goldProvider;
        _goldWallet = goldWallet;
        _objectPool = objectPool;
        _selectionPanel = selectionPanel;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("BuildingManager::Start() didn't find Camera in the scene");
            return;
        }

        if (_placementMap == null)
        {
            Debug.LogError("BuildingManager::Start() didn't find _placementMap");
            return;
        }

        if (_selectionPanel == null)
        {
            Debug.LogError("BuildingManager::Start() TowerSelectionPanel was not injected!");
            return;
        }

        _selectionPanel.OnTowerSelected += StartPlacement;
        _selectionPanel.OnTowerDeselected += CancelPlacementPreview;

        if (_ghostTowerPrefab == null)
        {
            Debug.LogError("BuildingManager::Start() didn't find _ghostTowerPrefab");
            return;
        }

        _ghostTower = Instantiate(_ghostTowerPrefab);
        _ghostTower.SetActive(false);
        
        _ghostSpriteRenderer = _ghostTower.GetComponent<SpriteRenderer>();
        _ghostPlacementPreview = _ghostTower.GetComponent<TowerPlacementPreview>();

        if (_ghostPlacementPreview == null)
        {
            Debug.LogWarning("BuildingManager::Start() didn't find TowerPlacementPreview on _ghostTowerPrefab");
        }
    }

    private IEnumerator ProcessPlacementPreviewRoutine(GameObject towerPrefab, int cost)
    {
        if (_goldProvider == null || _goldWallet == null)
        {
            Debug.LogWarning("BuildingManager::ProcessPlacementPreviewRoutine() gold services were not injected");
            StopPlacement();
            yield break;
        }

        if (_goldProvider.CurrentGold < cost)
        {
            StopPlacement();
            yield break;
        }

        if (Mouse.current == null)
        {
            Debug.LogError("BuildingManager::ProcessPlacementPreviewRoutine() didn't find Mouse.current");
            StopPlacement();
            yield break;
        }

        if (towerPrefab == null)
        {
            Debug.LogWarning("BuildingManager::ProcessPlacementPreviewRoutine() received null towerPrefab");
            StopPlacement();
            yield break;
        }
                
        _ghostSpriteRenderer.sprite = towerPrefab.GetComponent<SpriteRenderer>().sprite;
        _ghostTower.SetActive(true);

        yield return null;

        while (true)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                StopPlacement();
                yield break;
            }

            Vector3 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0;
            Vector3Int cellPosition = _placementMap.WorldToCell(mouseWorldPosition);
            Vector3 worldPosition = _placementMap.GetCellCenterWorld(cellPosition);
            worldPosition.z = 0;
            Vector3 offsetPosition = worldPosition + new Vector3(0, _placementMap.cellSize.y * 0.25f);

            _ghostTower.transform.position = offsetPosition;

            bool isValidTile = _goldWallet.HasEnoughGold(cost) && _placementMap.HasTile(cellPosition) && !_occupiedTiles.ContainsKey(cellPosition);
            _ghostPlacementPreview.IsValid = isValidTile;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    StopPlacement();
                    yield break;
                }

                if (isValidTile)
                {
                    _goldWallet.TakeGold(cost);
                    _occupiedTiles[cellPosition] = towerPrefab;
                    GameObject tower = _objectPool.Get(towerPrefab);
                    tower.transform.position = offsetPosition;
                    if (tower.TryGetComponent<Tower>(out Tower towerComponent))
                    {
                        OnTowerPlaced?.Invoke(towerComponent);
                    }
                }
                StopPlacement();
            }

            yield return null;
        }
    }

    private void StartPlacement(GameObject towerPrefab, int cost)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessPlacementPreviewRoutine(towerPrefab, cost));
    }

    private void CancelPlacementPreview()
    {
        StopAllCoroutines();
        if (_ghostTower != null)
        {
            _ghostTower.SetActive(false);
        }
    }

    private void StopPlacement()
    {
        StopAllCoroutines();
        _selectionPanel.DeselectTower();
        _ghostTower.SetActive(false);
    }
}
