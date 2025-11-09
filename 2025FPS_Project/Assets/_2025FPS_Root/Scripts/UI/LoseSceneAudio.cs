using UnityEngine;

public class LoseSceneAudio : MonoBehaviour
{
    void Start()
    {
        // Reproducir música de fondo del menú
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLoop("LoseMenuBGM");
    }

    public void OnReplayButtonPresed()
    {
        // Detener la música de fondo del menú y cargar la escena de juego
        SceneManagerSimple.Instance.LoadScene("SCN_AlejandroTask");
    }

    public void OnMenuButtonPresed()
    {
        SceneManagerSimple.Instance.LoadScene("SCN_MainMenu");
    }
}
