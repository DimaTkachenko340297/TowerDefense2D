using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameModeManager _gameModeManager;
    [SerializeField] private EnemySpawner _enemySpawner;

    [Header("UI References")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private TMP_Text _completedWavesText;

    private void OnEnable()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.AddListener(HandleRestartButtonClicked);
        }
        else
        {
            Debug.LogError("GameOverUI::OnEnable() _restartButton reference is missing!");
            Destroy(this);
            return;
        }

        if (_gameModeManager != null)
        {
            _gameModeManager.OnStateChanged += HandleStateChanged;
        }
        else
        {
            Debug.LogError("GameOverUI::OnEnable() _gameModeManager reference is missing!");
        }
    }

    private void OnDisable()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
        }

        if (_gameModeManager != null)
        {
            _gameModeManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleRestartButtonClicked()
    {
        if (_gameModeManager != null)
        {
            _gameModeManager.RestartGame();
        }
    }

    private void HandleStateChanged(GameState newState)
    {
        bool isGameOver = newState == GameState.GameOver;
        _gameOverPanel.SetActive(isGameOver);

        if (isGameOver && _completedWavesText != null && _enemySpawner != null)
        {
            _completedWavesText.text = $"Waves Reached: {_enemySpawner.WavesCount}";
        }
    }
}