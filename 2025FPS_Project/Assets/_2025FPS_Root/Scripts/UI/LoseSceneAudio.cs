using UnityEngine;

public class LoseSceneAudio : MonoBehaviour
{

    public void OnReplayButtonPresed()
    {
        AudioManager.Instance.Play("StartPressButton");
        // Detener la música de fondo del menú y cargar la escena de juego
        AudioManager.Instance.StopLoop("LoseMenuBGM");
        SceneManagerSimple.Instance.LoadScene("SCN_GameMenu");
    }

    public void OnMenuButtonPresed()
    {
        AudioManager.Instance.Play("StartPressButton");
        AudioManager.Instance.StopLoop("LoseMenuBGM");
        SceneManagerSimple.Instance.LoadScene("SCN_MainMenu");
    }
}
