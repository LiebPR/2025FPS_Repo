using UnityEngine;
using UnityEngine.EventSystems;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float hoverScale = 1.1f;     // Escala al poner el ratón encima
    [SerializeField] float scaleSpeed = 5f;       // Velocidad de Lerp para la escala

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void Update()
    {
        // Lerp hacia la escala objetivo para suavizar el efecto
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

    // Cuando el ratón entra sobre el objeto
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * hoverScale; // Agrandar
    }

    // Cuando el ratón sale del objeto
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale; // Volver a tamaño original
    }
}
