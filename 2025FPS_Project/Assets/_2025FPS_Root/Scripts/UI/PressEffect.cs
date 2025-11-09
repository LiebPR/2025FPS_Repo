using UnityEngine;
using UnityEngine.EventSystems;

public class PressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float pressScale = 0.9f;   // Escala al presionar
    [SerializeField] float lerpSpeed = 10f;     // Velocidad de Lerp

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Lerp hacia la escala objetivo para suavizar la animación
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * lerpSpeed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressScale;  // Reducir escala
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;               // Volver a tamaño original
    }
}
