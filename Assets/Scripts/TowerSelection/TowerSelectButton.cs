using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerSelectButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _towerPrefab;
    public GameObject TowerPrefab => _towerPrefab;
    [SerializeField] private Image _icon;
    private Vector3 _defaultIconSize;
    // (TowerSelectButton thisButton)
    public Action<TowerSelectButton> OnButtonSelected;
    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (_isSelected == true)
            { ChangeIcon(0.5f, 0.9f); }
            else
            { ChangeIcon(1f, 1f); }
        }
    }
    private void Awake()
    {
        if (_icon == null)
        {
            if (transform.childCount > 0)
            {
                _icon = transform.GetChild(0).GetComponent<Image>();
            }
        }

        if (_icon != null)
        { 
            _defaultIconSize = _icon.transform.localScale; 
        }
        else
        {
            Debug.LogWarning("TowerSelectButton::Awake() didn't find Image component");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnButtonSelected?.Invoke(this);
    }

    private void ChangeIcon(float alpha, float size)
    {
        if (_icon != null)
        {
            alpha = Mathf.Clamp01(alpha); 

            Color currentColor = _icon.color;
            currentColor.a = alpha;
            _icon.color = currentColor;

            _icon.transform.localScale = _defaultIconSize * size;
        }
    }
}
