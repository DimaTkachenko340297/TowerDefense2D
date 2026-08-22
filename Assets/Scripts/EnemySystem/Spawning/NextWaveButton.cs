using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NextWaveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private Animation _tooltipAnimation;
    [SerializeField] private AnimationClip _hideClip;

    [Header("Hold Settings")]
    [SerializeField] private float _holdThresholdDuration = 0.5f;
    [SerializeField] private TMP_Text _text;

    [Header("Animation General Settings")]
    [SerializeField] private float _animationDuration = 0.2f;
    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Space(2)]
    [SerializeField] private float _normalAlpha = 1f;
    [SerializeField] private float _pressedAlpha = 0.5f;

    [Space(2)]
    [SerializeField] private float _normalScaleMultiplier = 1f;
    [SerializeField] private float _pressedScaleMultiplier = 0.9f;

    private Vector3 _defaultTextSize;
    private WaitForSeconds _holdWaitInstruction;
    private bool _isTooltipHidden;
    private float _pointerDownTime;
    private float _currentScaleMultiplier = 1f;

    private Coroutine _holdCoroutine;
    private Coroutine _buttonAnimationCoroutine;
    private Coroutine _tooltipAnimationCoroutine;

    public Action OnPressedButton;
    public Action OnButtonHeld;

    private void Awake()
    {
        _holdWaitInstruction = new WaitForSeconds(_holdThresholdDuration);

        if (_text == null)
        {
            _text = GetComponentInChildren<TMP_Text>();
            
            if (_text == null)
            {
                Debug.LogError($"NextWaveButton:Awake() TMP_Text component is missing on {gameObject.name} and its children!");
                enabled = false;
                return;
            }
            
            Debug.LogWarning($"NextWaveButton:Awake() _text was not assigned in Inspector. Auto-assigned to child: {_text.name}");
        }

        _defaultTextSize = _text.transform.localScale;

        if (_tooltipAnimation == null || _hideClip == null)
        {
            Debug.LogWarning($"NextWaveButton:Awake() Tooltip Animation or Clip is missing. Tooltip system disabled.");
            _isTooltipHidden = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownTime = Time.time;
        
        SetPressedState(true);

        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
        }
        _holdCoroutine = StartCoroutine(HoldRoutine());

        if (!_isTooltipHidden)
        {
            if (_tooltipAnimationCoroutine != null) 
            {
                StopCoroutine(_tooltipAnimationCoroutine);
            }
            _tooltipAnimationCoroutine = StartCoroutine(HideTooltipRoutine());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
        }

        float holdDuration = Time.time - _pointerDownTime;

        if (holdDuration <= _holdThresholdDuration)
        {
            SetPressedState(false);
            _text.text = "Go";
            OnPressedButton?.Invoke();
        }
    }

    private void SetPressedState(bool isPressed)
    {
        float targetAlpha = isPressed ? _pressedAlpha : _normalAlpha;
        float targetScale = isPressed ? _pressedScaleMultiplier : _normalScaleMultiplier;

        if (_buttonAnimationCoroutine != null) 
        {
            StopCoroutine(_buttonAnimationCoroutine);
        }

        _buttonAnimationCoroutine = StartCoroutine(AnimateButtonRoutine(_text.color.a, targetAlpha, _currentScaleMultiplier, targetScale));
    }

    private IEnumerator HoldRoutine()
    {
        yield return _holdWaitInstruction;
        
        _text.text = "Auto";
        OnButtonHeld?.Invoke();
    }

    private IEnumerator HideTooltipRoutine()
    {
        _tooltipAnimation.Play(_hideClip.name);

        yield return new WaitForSeconds(_hideClip.length);

        _tooltipAnimation.gameObject.SetActive(false);
        _isTooltipHidden = true;
    }

    private IEnumerator AnimateButtonRoutine(float startAlpha, float targetAlpha, float startScaleMultiplier, float targetScaleMultiplier)
    {
        float currentTime = 0f;

        while (currentTime < _animationDuration)
        {
            currentTime += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(currentTime / _animationDuration);
            float curveProgress = _animationCurve.Evaluate(normalizedTime);

            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveProgress);
            _currentScaleMultiplier = Mathf.Lerp(startScaleMultiplier, targetScaleMultiplier, curveProgress);

            Color currentColor = _text.color;
            currentColor.a = Mathf.Clamp01(currentAlpha);
            _text.color = currentColor;

            _text.transform.localScale = _defaultTextSize * _currentScaleMultiplier;

            yield return null;
        } 

        Color finalColor = _text.color;
        finalColor.a = Mathf.Clamp01(targetAlpha);
        _text.color = finalColor;

        _currentScaleMultiplier = targetScaleMultiplier;
        _text.transform.localScale = _defaultTextSize * _currentScaleMultiplier;
    }
}