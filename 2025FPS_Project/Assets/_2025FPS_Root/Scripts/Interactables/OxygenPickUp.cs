using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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

    [Header("SoundFade")]
    [SerializeField] float fadeOutSpeed = 3f; // Ajusta la velocidad del desvanecido
    private bool isFadingOut = false;

    private float currentOxygen;
    private bool isConsuming;
    private Renderer objRenderer;
    private Material originalMaterial;
    private Vector3 originalScale;

    #endregion

    #region Getters
    public static bool IsConsumingOxygen { get; private set; }
    #endregion

    #region References
    private CountdownTimer countdownTimer;
    AudioSource audioSource;
    #endregion

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
            originalMaterial = objRenderer.material;

        originalScale = transform.localScale; // Guardamos la escala original
        audioSource = GetComponent<AudioSource>();

        //Si no se asignó en el inspector, intentamos encontrar uno (seguro)
        if(countdownTimer == null)
        {
            countdownTimer = FindAnyObjectByType<CountdownTimer>();
            if(countdownTimer == null)
                Debug.LogWarning($"[OxygenPickUp] No se encontró CountdownTimer en la escena. Asigna uno en el inspector del objeto {name}.");
        }
    }

    private void Start()
    {
        currentOxygen = totalOxygen;
    }

    #region Interacción
    public void OnPress()
    {
        if (countdownTimer.IsFull) return;

        isConsuming = true;
        IsConsumingOxygen = true;
        countdownTimer?.StartOxygenConsumption();
        isFadingOut = false;

        if (audioSource != null)
        {
            audioSource.loop = true;
            if (!audioSource.isPlaying)
                audioSource.Play();
            audioSource.volume = 0.5f;
        }
    }

    public void OnRelease()
    {
        isConsuming = false;
        IsConsumingOxygen = false;

        // Reanudar el temporizador
        countdownTimer?.StopOxygenConsumption();

        // Comenzar a desvanecer el sonido
        isFadingOut = true;
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
            // Si el temporizador ya está lleno → detener consumo automáticamente
            if (countdownTimer != null && countdownTimer.IsFull)
            {
                OnRelease();
                return;
            }

            float oxygenToConsume = Mathf.Min(oxygenRate * Time.deltaTime, currentOxygen);
            currentOxygen -= oxygenToConsume;

            bool reached = countdownTimer.AddTime(oxygenToConsume);
            if (reached)
            {
                OnRelease(); // ← Forzamos detener
                return;
            }

            // Añadir al temporizador (puede llegar a llenar aquí)
            if (countdownTimer != null)
                countdownTimer.AddTime(oxygenToConsume);

            //CHECK ADICIONAL: si tras sumar llegó al máximo, forzamos release =====
            if (countdownTimer != null && countdownTimer.IsFull)
            {
                OnRelease();
                return;
            }

            // Encoger gradualmente el objeto
            ShrinkObject();
        }

        // Fade out del audio
        if (isFadingOut && audioSource != null && audioSource.isPlaying)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, fadeOutSpeed * Time.deltaTime);

            if (audioSource.volume <= 0.01f)
            {
                audioSource.Stop();
                audioSource.volume = 0.5f; // Reset para el próximo uso
                isFadingOut = false;
            }
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
