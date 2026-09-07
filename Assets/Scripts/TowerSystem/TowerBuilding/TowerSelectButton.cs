using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerSelectButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject _towerPrefab;
    public GameObject TowerPrefab => _towerPrefab;

    [SerializeField] private int _cost = 10;
    public int Cost => _cost;

    [Header("UI References")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _costText;

    [Header("Animation General Settings")]
    [SerializeField, Min(0f)] private float _animationDuration = 0.15f;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("State Multipliers")]
    [SerializeField, Range(0f, 1f)] private float _normalAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _selectedAlpha = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.35f;

    [Space(2)]
    [SerializeField, Min(0f)] private float _normalScaleMultiplier = 1f;
    [SerializeField, Min(0f)] private float _selectedScaleMultiplier = 0.9f;

    [Header("Cost Text Colors")]
    [SerializeField] private Color _affordableColor = Color.white;
    [SerializeField] private Color _unaffordableColor = new Color(1f, 0.3f, 0.3f, 1f);

    private Vector3 _defaultIconSize;
    private float _currentScaleMultiplier = 1f;
    private Coroutine _animationCoroutine;

    // (TowerSelectButton thisButton)
    public Action<TowerSelectButton> OnButtonSelected;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            UpdateVisualState();
        }
    }

    private bool _hasEnoughGold = true;

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
            Debug.LogWarning("TowerSelectButton::Awake() Image component is missing!");
        }

        if (_costText != null)
        {
            _costText.text = _cost.ToString();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_hasEnoughGold)
        {
            return;
        }

        OnButtonSelected?.Invoke(this);
    }

    public bool SetHasEnoughGold(int amount)
    {
        bool hasGold = _cost <= amount;

        if (_hasEnoughGold == hasGold)
        {
            return hasGold;
        }

        _hasEnoughGold = hasGold;

        if (_costText != null)
        {
            _costText.color = _hasEnoughGold ? _affordableColor : _unaffordableColor;
        }

        UpdateVisualState();

        return hasGold;
    }

    private void UpdateVisualState()
    {
        float targetAlpha;
        float targetScale;

        if (!_hasEnoughGold)
        {
            targetAlpha = _disabledAlpha;
            targetScale = _normalScaleMultiplier;
        }
        else if (_isSelected)
        {
            targetAlpha = _selectedAlpha;
            targetScale = _selectedScaleMultiplier;
        }
        else
        {
            targetAlpha = _normalAlpha;
            targetScale = _normalScaleMultiplier;
        }

        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }

        if (_icon != null)
        {
            _animationCoroutine = StartCoroutine(AnimateButtonRoutine(_icon.color.a, targetAlpha, _currentScaleMultiplier, targetScale));
        }
    }

    private IEnumerator AnimateButtonRoutine(float startAlpha, float targetAlpha, float startScale, float targetScale)
    {
        float currentTime = 0f;

        while (currentTime < _animationDuration)
        {
            currentTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(currentTime / _animationDuration);
            float curveProgress = _animationCurve.Evaluate(normalizedTime);

            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveProgress);
            _currentScaleMultiplier = Mathf.Lerp(startScale, targetScale, curveProgress);

            Color currentColor = _icon.color;
            currentColor.a = Mathf.Clamp01(currentAlpha);
            _icon.color = currentColor;

            _icon.transform.localScale = _defaultIconSize * _currentScaleMultiplier;

            yield return null;
        }

        Color finalColor = _icon.color;
        finalColor.a = Mathf.Clamp01(targetAlpha);
        _icon.color = finalColor;

        _currentScaleMultiplier = targetScale;
        _icon.transform.localScale = _defaultIconSize * _currentScaleMultiplier;
    }
}