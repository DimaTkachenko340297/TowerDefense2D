using UnityEngine;
using TMPro;
using VContainer;

public class BaseHealthCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;

    private BaseHealth _baseHealth;

    [Inject]
    private void Construct(BaseHealth baseHealth)
    {
        _baseHealth = baseHealth;
    }
    private void Start()
    {
        if (_healthText == null)
        {
            Debug.LogError("BaseHealthCounterUI::Start() _healthText reference is missing!");
            Destroy(this);
            return;
        }

        if (_baseHealth != null)
        {
            _baseHealth.OnBaseHealthChanged += UpdateText;
            UpdateText(_baseHealth.CurrentHealth);
        }
    }

    private void OnDisable()
    {
        if (_baseHealth != null)
        {
            _baseHealth.OnBaseHealthChanged -= UpdateText;
        }
    }

    private void UpdateText(int newValue)
    {
        _healthText.text = newValue.ToString();
    }
}
