using TMPro;
using UnityEngine;
using VContainer;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;
    private IGoldProvider _goldProvider;

    [Inject]
    private void Construct(IGoldProvider goldProvider)
    {
        _goldProvider = goldProvider;
    }

    private void OnEnable()
    {
        if (_goldText == null)
        {
            Debug.LogError("GoldUI::OnEnable() _goldText reference is missing!");
            enabled = false;
            return;
        }

        if (_goldProvider == null)
        {
            Debug.LogError("GoldUI::OnEnable() IGoldProvider was not injected!");
            enabled = false;
            return;
        }

        _goldProvider.OnGoldChanged += UpdateGoldText;
        UpdateGoldText(_goldProvider.CurrentGold);
    }

    private void OnDisable()
    {
        if (_goldProvider != null)
        {
            _goldProvider.OnGoldChanged -= UpdateGoldText;
        }
    }

    private void UpdateGoldText(int newAmount)
    {
        _goldText.text = newAmount.ToString();
    }
}
