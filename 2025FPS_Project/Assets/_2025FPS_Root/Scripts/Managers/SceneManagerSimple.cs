using UnityEngine;
using UnityEngine.SceneManagement;

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
    //Caraga una escena por nombre.
    public void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    //Cargar una escena por índice (según el orden del Build Settings).
    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    //Recargar la escena actual.
    public void ReloadCurrentScene()
    {
        var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(current.name);
    }

    //Cierra la aplicación (solo funciona en build)
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    //Acceso global al SceneManagerSimple.
    public static SceneManagerSimple Instance => instance;
}
