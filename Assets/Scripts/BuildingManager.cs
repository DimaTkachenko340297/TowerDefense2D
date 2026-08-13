using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(ObjectPool))]
public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Tilemap _placementMap;
    [SerializeField] private TowerSelectionPanel _selectionPanel;

    private Dictionary<Vector3Int, GameObject> _occupiedTiles = new();
    private ObjectPool _objectPool;
    private Camera _mainCamera;

    [Header("Preview Tower")]
    [SerializeField] private GameObject _ghostTowerPrefab;
    private GameObject _ghostTower;
    private SpriteRenderer _ghostSpriteRenderer;
    private TowerPlacementPreview _ghostPlacementPreview;

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
            Debug.LogWarning("BuildingManager::Start() didn't find _selectionPanel, trying to search in scene...");
            _selectionPanel = FindFirstObjectByType<TowerSelectionPanel>();

            if (_selectionPanel == null)
            {
                Debug.LogError("BuildingManager::Start() didn't find TowerSelectionPanel in the scene");
                return;
            }   
        }

        _selectionPanel.OnTowerSelected += StartPlacement;
        _objectPool = GetComponent<ObjectPool>();

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
            Debug.LogWarning("BuildingManager::Start() didn't find TowerPlacementPreview on _ghostTowerPrefab");
    }

    private IEnumerator ProcessPlacementPreviewRoutine(GameObject towerPrefab)
    {
        if (Mouse.current == null)
        {
            Debug.LogError("BuildingManager::ProcessPlacementPreviewRoutine() didn't find Mouse device");
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

            bool isValidTile = _placementMap.HasTile(cellPosition) && !_occupiedTiles.ContainsKey(cellPosition);
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
                    _occupiedTiles[cellPosition] = towerPrefab;
                    GameObject tower = _objectPool.Get(towerPrefab);
                    tower.transform.position = offsetPosition;
                }
                StopPlacement();
            }

            yield return null;
        }
    }

    private void StartPlacement(GameObject towerPrefab)
    {
        StopAllCoroutines();
        StartCoroutine(ProcessPlacementPreviewRoutine(towerPrefab));
    }

    private void StopPlacement()
    {
        StopAllCoroutines();
        _selectionPanel.DeselectTower();
        _ghostTower.SetActive(false);
    }
}
