using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    void Start()
    {
        // Reproducir música de fondo del menú
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoop("MainMenuBGM");
    }

    public void OnStartButtonPressed()
    {
        AudioManager.Instance.Play("StartPressButton");
        AudioManager.Instance.FadeOutLoop("MainMenuBGM", 1f);
        // Detener la música de fondo del menú y cargar la escena de juego
        SceneManagerSimple.Instance.LoadScene("SCN_AlejandroTask");
    }

    public void OnExitButtonPressed()
    {
        AudioManager.Instance.Play("ExitPressButton");
        SceneManagerSimple.Instance.QuitGame();
    }
}
