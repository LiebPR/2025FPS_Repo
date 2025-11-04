using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    public enum GameState { Menu, Playing, Paused, GameOver, Win }

    GameState currentState;

    #region Eventos
    public event Action OnGameOver;
    public event Action OnGameWin;
    public event Action<GameState> OnStateChanged;
    #endregion

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        currentState = GameState.Menu;
    }

    #region Estado
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnStateChanged?.Invoke(currentState);
    }
    #endregion

    public void TogglePause()
    {
        if (currentState == GameState.Paused)
            SetState(GameState.Playing);
        else if (currentState == GameState.Playing)
            SetState(GameState.Paused);
    }

    #region Condiciones
    public void GameOver()
    {
        if (currentState != GameState.Playing)
        {
            return;
        }

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();
        SceneManagerSimple.Instance.LoadScene("SCN_Lose");
    }

    public void GameWin()
    {
        if (currentState != GameState.Playing) return;

        SetState(GameState.Win);
        OnGameWin?.Invoke();
        SceneManagerSimple.Instance.LoadScene("SCN_Win");
    }
    #endregion

    #region Utilities
    public void StartGame()
    {
        SetState(GameState.Playing);
    }

    public void RestartGame()
    {
        SceneManagerSimple.Instance.ReloadCurrentScene();
        SetState(GameState.Playing);
    }

    public static GameManager Instance => instance;
    public GameState CurrentState => currentState;
    #endregion
}
