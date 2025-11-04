using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OxygenPickUp : MonoBehaviour, IInteractable
{
    #region General Variables
    [SerializeField] float totalOxygen = 30f;    // Total de oxígeno disponible
    [SerializeField] float oxygenRate = 10f;     // Oxígeno que se consume por segundo
    [SerializeField] float shrinkSpeed = 1f;     // Velocidad de encogimiento por segundo
    [SerializeField] float minScale = 0.1f;      // Factor mínimo de escala antes de desaparecer

    [Header("Highlight")]
    [SerializeField] Material highlightMaterial;

    private float currentOxygen;
    private bool isConsuming;
    private Renderer objRenderer;
    private Material originalMaterial;
    private Vector3 originalScale;

    #endregion

    #region References
    private CountdownTimer countdownTimer;
    #endregion

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;

        originalScale = transform.localScale; // Guardamos la escala original
    }

    private void Start()
    {
        countdownTimer = FindAnyObjectByType<CountdownTimer>();
        currentOxygen = totalOxygen;
    }

    #region Interacción
    public void OnPress()
    {
        if (!isConsuming && currentOxygen > 0f)
        {
            isConsuming = true;
        }
    }

    public void OnRelease()
    {
        isConsuming = false;
    }
    #endregion

    #region Highlight
    public void OnHighlight()
    {
        if (objRenderer != null && highlightMaterial != null)
            objRenderer.material = highlightMaterial;
    }

    public void OnRemoveHighlight()
    {
        if (objRenderer != null && originalMaterial != null)
            objRenderer.material = originalMaterial;
    }
    #endregion

    #region Funciones Auxiliares
    private void Update()
    {
        if (isConsuming && currentOxygen > 0f)
        {
            // Consumir oxígeno poco a poco
            float oxygenToConsume = Mathf.Min(oxygenRate * Time.deltaTime, currentOxygen);
            currentOxygen -= oxygenToConsume;
            countdownTimer.AddTime(oxygenToConsume);

            // Encoger gradualmente el objeto
            ShrinkObject();
        }
    }

    private void ShrinkObject()
    {
        // Calculamos el factor de escala uniforme según el oxígeno restante
        float scaleFactor = Mathf.Max(currentOxygen / totalOxygen, minScale);

        // Escala objetivo proporcional a la original
        Vector3 targetScale = originalScale * scaleFactor;

        // Escalamos suavemente usando shrinkSpeed
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, shrinkSpeed * Time.deltaTime);

        // Si llegamos al tamaño mínimo, desaparece
        if (transform.localScale.x <= originalScale.x * minScale)
        {
            gameObject.SetActive(false);
        }
    }
    #endregion
}
