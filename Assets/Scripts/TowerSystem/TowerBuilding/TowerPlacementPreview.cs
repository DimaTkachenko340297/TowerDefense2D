using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TowerPlacementPreview : MonoBehaviour
{ 
    [SerializeField] private Color _validPlacementColor;
    [SerializeField] private Color _placementBlockedColor;
    [SerializeField] [Range(0f, 1f)] private float _previewAlpha;

    private SpriteRenderer _spriteRenderer;
    private bool _isValid;
    public bool IsValid
    {
        get => _isValid;
        set
        {
            _isValid = value;
            ApplyColor();
        }
    }


    private void Awake() 
    {
        ApplyColor();
    }

    private void ApplyColor()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Color targetColor = _isValid ? _validPlacementColor : _placementBlockedColor;
        targetColor.a = _previewAlpha;
        _spriteRenderer.color = targetColor;
    }
}
