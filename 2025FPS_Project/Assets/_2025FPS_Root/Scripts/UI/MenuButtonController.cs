using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Controla la animación de escala y opacidad de la nave/botón principal,
/// así como la aparición/desaparición de los botones Start y Exit.
/// </summary>
public class MenuButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Button startButton;
    [SerializeField] Button exitButton;
    [SerializeField] float scaleAmount = 0.8f;       // Escala al presionar
    [SerializeField] float scaleSpeed = 5f;          // Velocidad Lerp para la escala
    [SerializeField] float fadeSpeed = 3f;           // Velocidad Lerp para opacidad

    private Vector3 originalScale;
    private bool isButtonPressedOnce = false;  // Alternancia primera/segunda pulsación

    private CanvasGroup startButtonCanvasGroup;
    private CanvasGroup exitButtonCanvasGroup;
    private CanvasGroup buttonCanvasGroup;    // CanvasGroup de la nave

    void Start()
    {
        originalScale = transform.localScale;

        // Botones Start y Exit
        startButtonCanvasGroup = startButton.GetComponent<CanvasGroup>();
        exitButtonCanvasGroup = exitButton.GetComponent<CanvasGroup>();
        startButtonCanvasGroup.alpha = 0;
        exitButtonCanvasGroup.alpha = 0;

        // Nave
        buttonCanvasGroup = GetComponent<CanvasGroup>();
        buttonCanvasGroup.alpha = 1f;
    }

    void Update()
    {
        // Escala suavizada según el estado
        Vector3 targetScale = isButtonPressedOnce ? originalScale * scaleAmount : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);

        // Opacidad suavizada
        float targetAlpha = isButtonPressedOnce ? 0.5f : 1f;
        buttonCanvasGroup.alpha = Mathf.Lerp(buttonCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isButtonPressedOnce)
        {
            isButtonPressedOnce = true;
            StartCoroutine(FadeInButtons());
        }
        else
        {
            isButtonPressedOnce = false;
            StartCoroutine(FadeOutButtons());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // No hacemos nada por ahora
    }

    private IEnumerator FadeInButtons()
    {
        float duration = 0.5f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float alphaValue = Mathf.Lerp(0f, 1f, timeElapsed / duration);
            startButtonCanvasGroup.alpha = alphaValue;
            exitButtonCanvasGroup.alpha = alphaValue;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        startButtonCanvasGroup.alpha = 1f;
        exitButtonCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutButtons()
    {
        float duration = 0.5f;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float alphaValue = Mathf.Lerp(1f, 0f, timeElapsed / duration);
            startButtonCanvasGroup.alpha = alphaValue;
            exitButtonCanvasGroup.alpha = alphaValue;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        startButtonCanvasGroup.alpha = 0f;
        exitButtonCanvasGroup.alpha = 0f;
    }
}
