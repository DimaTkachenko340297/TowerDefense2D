using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private EnemySpawner _enemySpawner;

    private void Start()
    {
        if (_waveText == null)
        {
            Debug.LogError("WaveUI::Start() TMP_Text reference is missing!");
            Destroy(this);
            return;
        }

        if (_enemySpawner == null)
        {
            Debug.LogError("WaveUI::Start() EnemySpawner reference is missing!");
            Destroy(this);
            return;
        }

        _enemySpawner.OnCompletedWavesCountChanged += UpdateWaveText;
    }

    private void OnDisable()
    {
        if (_enemySpawner != null)
        {
            _enemySpawner.OnCompletedWavesCountChanged -= UpdateWaveText;
        }
    }

    private void UpdateWaveText(int wavesCount)
    {
        _waveText.text = wavesCount.ToString();
    }
}