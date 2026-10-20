using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameModeManager : IDisposable
{
    private readonly BaseHealth _baseHealth;
    private GameState _currentState = GameState.Active;
    public GameState CurrentState => _currentState;

    public GameModeManager(BaseHealth baseHealth)
    {
        _baseHealth = baseHealth ?? throw new ArgumentNullException(nameof(baseHealth));
        _baseHealth.OnBaseDestroyed += HandleBaseDestroyed;
    }

    // (GameState newState)
    public event Action<GameState> OnStateChanged;

    public void Dispose()
    {
        _baseHealth.OnBaseDestroyed -= HandleBaseDestroyed;
    }

    public void SetState(GameState newState)
    {
        if (_currentState == newState)
        {
            return;
        }

        _currentState = newState;

        Time.timeScale = _currentState == GameState.Active ? 1f : 0f;

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