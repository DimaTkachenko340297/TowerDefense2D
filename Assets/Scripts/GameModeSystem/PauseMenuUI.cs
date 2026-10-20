using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class PauseMenuUI : MonoBehaviour
{
    private GameModeManager _gameModeManager;
    private bool _isSubscribedToGameMode;

    [Inject]
    private void Construct(GameModeManager gameModeManager)
    {
        _gameModeManager = gameModeManager;
        SubscribeToGameMode();
    }

    [Header("Buttons")]
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;

    [Header("Panels")]
    [SerializeField] private GameObject _pauseMenuPanel;

    private void OnEnable()
    {
        if (_pauseButton != null)
        {
            _pauseButton.onClick.AddListener(HandlePauseButtonClicked);
        }
        else
        {
            Debug.LogWarning("PauseMenuUI::OnEnable() _pauseButton reference is missing!");
        }

        if (_resumeButton != null)
        {
            _resumeButton.onClick.AddListener(HandleResumeButtonClicked);
        }
        else
        {
            Debug.LogWarning("PauseMenuUI::OnEnable() _resumeButton reference is missing!");
        }

        SubscribeToGameMode();
    }

    private void OnDisable()
    {
        if (_pauseButton != null)
        {
            _pauseButton.onClick.RemoveListener(HandlePauseButtonClicked);
        }

        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(HandleResumeButtonClicked);
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

    private void HandlePauseButtonClicked()
    {
        if (_gameModeManager != null)
        {
            _gameModeManager.PauseGame();
        }
    }

    private void HandleResumeButtonClicked()
    {
        if (_gameModeManager != null)
        {
            _gameModeManager.ResumeGame();
        }
    }

    private void HandleStateChanged(GameState newState)
    {
        bool isPaused = newState == GameState.Paused;
        
        if (_pauseMenuPanel != null)
        {
            _pauseMenuPanel.SetActive(isPaused);
        }

        if (_pauseButton != null)
        {
            _pauseButton.gameObject.SetActive(!isPaused);
        }
    }
}