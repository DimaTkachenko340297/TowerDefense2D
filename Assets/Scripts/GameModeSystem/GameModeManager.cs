using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : MonoBehaviour
{
    [SerializeField] private BaseHealth _baseHealth;

    public GameState _currentState = GameState.Active;
    public GameState CurrentState => _currentState;

    // (GameState newState)
    public Action<GameState> OnStateChanged;

    private void Start()
    {
        if (_baseHealth == null)
        {
            Debug.LogError("GameModeManager::Start() _baseHealth reference is missing!");
            Destroy(this);
            return;
        }

        _baseHealth.OnBaseDestroyed += HandleBaseDestroyed;
        OnStateChanged?.Invoke(_currentState);
    }

    private void OnDisable()
    {
        if (_baseHealth != null)
        {
            _baseHealth.OnBaseDestroyed -= HandleBaseDestroyed;
        }
    }

    public void SetState(GameState newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        _currentState = newState;

        if (_currentState == GameState.Active)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }

        OnStateChanged?.Invoke(_currentState);
    }

    public void PauseGame()
    {
        if (_currentState == GameState.GameOver)
        {
            return;
        }

        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentState == GameState.GameOver)
        {
            return;
        }

        SetState(GameState.Active);
    }

    private void HandleBaseDestroyed()
    {
        SetState(GameState.GameOver);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}