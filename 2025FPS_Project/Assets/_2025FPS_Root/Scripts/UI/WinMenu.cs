using UnityEngine;

public class WinMenu : MonoBehaviour
{
    void Start()
    {
        // Reproducir música de fondo del menú
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoop("WinMenuBGM");
    }

    public void OnReplayButtonPressed()
    {
        AudioManager.Instance.Play("StartPressButton");

        // Detener la música de fondo del menú y cargar la escena de juego
        SceneManagerSimple.Instance.LoadScene("SCN_GameMenu");
    }

    public void OnMenuButtonPressed()
    {
        AudioManager.Instance.Play("ExitPressButton");
        SceneManagerSimple.Instance.LoadScene("SCN_MainMenu");
    }
}
