using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NextWaveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private Animation _tooltipAnimation;
    [SerializeField] private AnimationClip _hideClip;

    [Header("Hold Settings")]
    [SerializeField, Min(0f)] private float _holdThresholdDuration = 0.5f;
    [SerializeField] private TMP_Text _text;

    [Header("Animation General Settings")]
    [SerializeField, Min(0f)] private float _animationDuration = 0.2f;
    [SerializeField] private AnimationCurve _animationCurve;

    [Space(2)]
    [SerializeField, Range(0f, 1f)] private float _normalAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _pressedAlpha = 0.5f;

    [Space(2)]
    [SerializeField, Min(0f)] private float _normalScaleMultiplier = 1f;
    [SerializeField, Min(0f)] private float _pressedScaleMultiplier = 0.9f;

    private Vector3 _defaultTextSize;
    private bool _isTooltipHidden;
    private float _pointerDownTime;
    private float _currentScaleMultiplier = 1f;
    private CancellationTokenSource _holdCancellation;
    private CancellationTokenSource _buttonAnimationCancellation;
    private CancellationTokenSource _tooltipAnimationCancellation;

    public Action OnWaveRequested;

    public bool IsAuto { get; private set; }
    private bool _isBlocked;
    public bool IsBlocked
    {
        get => _isBlocked;
        set
        {
            _isBlocked = value;
            if (!IsAuto)
            {
                SetPressedState(_isBlocked);
            }
        }
    }

    private void Awake()
    {
        if (_text == null)
        {
            _text = GetComponentInChildren<TMP_Text>();
            if (_text == null)
            {
                Debug.LogError($"NextWaveButton::Awake() _text component is missing on {gameObject.name} and its children!");
                enabled = false;
                return;
            }

            Debug.LogWarning($"NextWaveButton::Awake() _text was not assigned in Inspector. Auto-assigned to child: {_text.name}");
        }

        _defaultTextSize = _text.transform.localScale;
        if (_tooltipAnimation == null || _hideClip == null)
        {
            Debug.LogWarning("NextWaveButton::Awake() _tooltipAnimation or _hideClip is missing. Tooltip system disabled.");
            _isTooltipHidden = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownTime = Time.time;
        if (!_isBlocked)
        {
            SetPressedState(true);
        }

        CancelAndDispose(ref _holdCancellation);
        _holdCancellation = new CancellationTokenSource();
        HoldAsync(_holdCancellation.Token).Forget();

        if (!_isTooltipHidden)
        {
            CancelAndDispose(ref _tooltipAnimationCancellation);
            _tooltipAnimationCancellation = new CancellationTokenSource();
            HideTooltipAsync(_tooltipAnimationCancellation.Token).Forget();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelAndDispose(ref _holdCancellation);
        float holdDuration = Time.time - _pointerDownTime;
        if (holdDuration <= _holdThresholdDuration)
        {
            if (!_isBlocked)
            {
                SetPressedState(false);
            }

            _text.text = "Go";
            IsAuto = false;
            OnWaveRequested?.Invoke();
        }
    }

    private void SetPressedState(bool isPressed)
    {
        float targetAlpha = isPressed ? _pressedAlpha : _normalAlpha;
        float targetScale = isPressed ? _pressedScaleMultiplier : _normalScaleMultiplier;
        CancelAndDispose(ref _buttonAnimationCancellation);
        _buttonAnimationCancellation = new CancellationTokenSource();
        AnimateButtonAsync(_text.color.a, targetAlpha, _currentScaleMultiplier, targetScale, _buttonAnimationCancellation.Token).Forget();
    }

    private async UniTask HoldAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DelayOrYieldAsync(_holdThresholdDuration, cancellationToken);
            _text.text = "Auto";
            IsAuto = true;
            OnWaveRequested?.Invoke();
        }
        catch (OperationCanceledException) { }
    }

    private async UniTask HideTooltipAsync(CancellationToken cancellationToken)
    {
        try
        {
            _tooltipAnimation.Play(_hideClip.name);
            await DelayOrYieldAsync(_hideClip.length, cancellationToken);
            _tooltipAnimation.gameObject.SetActive(false);
            _isTooltipHidden = true;
        }
        catch (OperationCanceledException) { }
    }

    private static UniTask DelayOrYieldAsync(float seconds, CancellationToken cancellationToken)
    {
        return seconds <= 0f
            ? UniTask.Yield(PlayerLoopTiming.Update, cancellationToken)
            : UniTask.Delay(TimeSpan.FromSeconds(seconds), cancellationToken: cancellationToken);
    }
    private async UniTask AnimateButtonAsync(float startAlpha, float targetAlpha, float startScaleMultiplier, float targetScaleMultiplier, CancellationToken cancellationToken)
    {
        try
        {
            float currentTime = 0f;
            while (currentTime < _animationDuration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                currentTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(currentTime / _animationDuration);
                float curveProgress = _animationCurve.Evaluate(normalizedTime);
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveProgress);
                _currentScaleMultiplier = Mathf.Lerp(startScaleMultiplier, targetScaleMultiplier, curveProgress);

                Color currentColor = _text.color;
                currentColor.a = Mathf.Clamp01(currentAlpha);
                _text.color = currentColor;
                _text.transform.localScale = _defaultTextSize * _currentScaleMultiplier;
                await UniTask.NextFrame(cancellationToken);
            }

            Color finalColor = _text.color;
            finalColor.a = Mathf.Clamp01(targetAlpha);
            _text.color = finalColor;
            _currentScaleMultiplier = targetScaleMultiplier;
            _text.transform.localScale = _defaultTextSize * _currentScaleMultiplier;
        }
        catch (OperationCanceledException) { }
    }

    private void OnDisable()
    {
        CancelAndDispose(ref _holdCancellation);
        CancelAndDispose(ref _buttonAnimationCancellation);
        CancelAndDispose(ref _tooltipAnimationCancellation);
    }

    private static void CancelAndDispose(ref CancellationTokenSource cancellationTokenSource)
    {
        if (cancellationTokenSource == null)
        {
            return;
        }

        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = null;
    }
}