using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameModeManager _gameModeManager;

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

        if (_gameModeManager != null)
        {
            _gameModeManager.OnStateChanged += HandleStateChanged;
        }
        else
        {
            Debug.LogError("PauseMenuUI::OnEnable() _gameModeManager reference is missing!");
        }
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

        if (_gameModeManager != null)
        {
            _gameModeManager.OnStateChanged -= HandleStateChanged;
        }
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