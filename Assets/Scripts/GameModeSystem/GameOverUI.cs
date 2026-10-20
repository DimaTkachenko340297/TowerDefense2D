using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GameOverUI : MonoBehaviour
{
    private GameModeManager _gameModeManager;
    private bool _isSubscribedToGameMode;

    [Inject]
    private void Construct(GameModeManager gameModeManager)
    {
        _gameModeManager = gameModeManager;
        SubscribeToGameMode();
    }

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

        SubscribeToGameMode();
    }

    private void OnDisable()
    {
        if (_restartButton != null)
        {
            _restartButton.onClick.RemoveListener(HandleRestartButtonClicked);
        }

        if (_isSubscribedToGameMode)
        {
            _gameModeManager.OnStateChanged -= HandleStateChanged;
            _isSubscribedToGameMode = false;
        }
    }

    private void SubscribeToGameMode()
    {
        if (!isActiveAndEnabled || _gameModeManager == null || _isSubscribedToGameMode)
        {
            return;
        }

        _gameModeManager.OnStateChanged += HandleStateChanged;
        _isSubscribedToGameMode = true;
        HandleStateChanged(_gameModeManager.CurrentState);
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