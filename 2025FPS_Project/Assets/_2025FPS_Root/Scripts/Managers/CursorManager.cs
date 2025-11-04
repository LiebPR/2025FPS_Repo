using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
    }

    void HandleGameStateChanged(GameManager.GameState newState)
    {
        bool isPlaying = newState == GameManager.GameState.Playing;

        Cursor.lockState = isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPlaying;
    }

    // Opcional: para asegurar el estado inicial al cargar escena
    void Start()
    {
        if (GameManager.Instance != null)
            HandleGameStateChanged(GameManager.Instance.CurrentState);
    }
}
