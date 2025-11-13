using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroVideo : MonoBehaviour
{
    [Header("Referencias")]
    public VideoPlayer videoPlayer;
    public Image fadeImage;

    [Header("Configuración")]
    public string nextSceneName = "MainMenu";
    public float fadeDuration = 1.5f; // segundos de fundido

    private bool hasFaded = false;

    void Start()
    {
        // Cuando el vídeo termine, ejecuta la función
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (!hasFaded)
            StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        hasFaded = true;

        float elapsed = 0f;
        Color c = fadeImage.color;

        // Desvanecer progresivamente a negro
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        // Cargar la siguiente escena
        SceneManager.LoadScene(nextSceneName);
    }
}
