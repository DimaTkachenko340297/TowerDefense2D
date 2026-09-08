using UnityEngine;
using TMPro;

public class BaseHealthCounter : MonoBehaviour
{
    [SerializeField] private BaseHealth _baseHealth;
    [SerializeField] private TMP_Text _healthText;
    private void Start()
    {
        if (_healthText == null)
        {
            Debug.LogError("BaseHealthCounter::Start() _healthText reference is missing!");
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
