using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
    public Image fadeImage; // Referencia a la imagen del fade
    public float fadeDuration = 2f; // Duración del fade

    void Start()
    {
        // Inicializa la imagen con opacidad 0 (completamente transparente)
        fadeImage.gameObject.SetActive(true);
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutSet(sceneName));
    }

    private IEnumerator FadeOutSet(string sceneName)
    {
        // Fade Out (de 0 a 1)
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, Mathf.Clamp01(timeElapsed / fadeDuration));
            yield return null;
        }

        // Cargar la nueva escena
        SceneManager.LoadScene(sceneName);
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }

    private IEnumerator FadeInCoroutine()
    {
        // Fade In (de 1 a 0)
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            fadeImage.color = new Color(0f, 0f, 0f, 1 - Mathf.Clamp01(timeElapsed / fadeDuration));
            yield return null;
        }

        fadeImage.gameObject.SetActive(false); // Desactivar la imagen después del fade
    }
}
