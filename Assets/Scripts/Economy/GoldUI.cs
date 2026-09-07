

using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;
    private void Start()
    {
        if (_goldText == null)
        {
            Debug.LogError("GoldUI::Start() TMP_Text reference is missing!");
            Destroy(this);
        }

        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateGoldText;
            UpdateGoldText(GoldManager.Instance.CurrentGold);
        }
    }

    private void OnDisable()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged -= UpdateGoldText;
        }
    }

    private void UpdateGoldText(int newAmount)
    {
        _goldText.text = newAmount.ToString();
    }
}
