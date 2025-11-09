using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    void Start()
    {
        // Reproducir música de fondo del menú
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("MainMenuBGM");
    }

    public void OnStartButtonPressed()
    {
        // Detener la música de fondo del menú y cargar la escena de juego
        SceneManagerSimple.Instance.LoadScene("SCN_AlejandroTask");
    }
}
