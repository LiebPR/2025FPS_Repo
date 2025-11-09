using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneManagerSimple : MonoBehaviour
{
    static SceneManagerSimple instance;

    void Awake()
    {
        // Si ya existe una instancia, destruye la nueva
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Cargar una escena por nombre.
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithMusicControl(sceneName));
    }

    // Cargar una escena por índice (según el orden del Build Settings).
    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithMusicControl(sceneIndex));
    }

    // Recargar la escena actual.
    public void ReloadCurrentScene()
    {
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(current.name);
    }

    // Cierra la aplicación (solo funciona en build).
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Método para detener la música y cambiar de escena de manera controlada.
    private IEnumerator LoadSceneWithMusicControl(string sceneName)
    {
        // Detener la música del menú con fade-out
        AudioManager.Instance.FadeOutBGM(0.5f);

        // Esperar un poco para que el fade-out termine
        yield return new WaitForSeconds(0.5f);

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneName);

        // Iniciar la música del juego en la nueva escena
        // Aquí asumimos que el nombre del clip de música en el gameplay es "GameplayBGM"
        AudioManager.Instance.Play("GameplayBGM");
    }

    private IEnumerator LoadSceneWithMusicControl(int sceneIndex)
    {
        // Detener la música del menú con fade-out
        AudioManager.Instance.FadeOutBGM(0.5f);

        // Esperar un poco para que el fade-out termine
        yield return new WaitForSeconds(0.5f);

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneIndex);

        // Iniciar la música del juego en la nueva escena
        // Aquí asumimos que el nombre del clip de música en el gameplay es "GameplayBGM"
        AudioManager.Instance.Play("GameplayBGM");
    }

    // Acceso global al SceneManagerSimple.
    public static SceneManagerSimple Instance => instance;
}
