using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// Temporizador regresivo que muestra MM:SS en un TextMeshProUGUI.
/// Definir minutos en startMinutes (ej: 10 para 10:00).
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    #region General Variables
    [SerializeField] TextMeshProUGUI timerText; //referencia al TMPRO de la UI
    [SerializeField] int startMinutes = 10; //minutos iniciales (ej: 10 -> 10:00)
    bool startOnAwake = true; //arrancar al Start automáticamente
    UnityEvent onTimerEnd; //evento opcional al llegar a 00:00
    #endregion

    #region Variables internas
    float currentTime; // Tiempo actual en segundos
    float totalTime; //tiempo total en segundos
    bool isRunning; // Control de ejecución
    Color originalColor;
    bool isFlashing;
    float flashSpeed = 2f;
    #endregion

    void Start()
    {
        if (timerText == null)
        {
            Debug.LogError("CountdownTimer: No se ha asignado el TextMeshProUGUI.");
            enabled = false;
            return;
        }

        totalTime = Mathf.Max(1f, startMinutes * 60f);
        currentTime = totalTime;
        originalColor = timerText.color;

        if (startOnAwake)
            StartTimer();
        else
            UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        // Activar parpadeo cuando queden 10 segundos o menos
        if (currentTime <= 10f && !isFlashing)
            isFlashing = true;

        if (isFlashing)
            FlashText();

        // Verificar si terminó
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            isFlashing = false;
            timerText.color = Color.red;
            UpdateTimerDisplay();
            onTimerEnd?.Invoke();
        }

        UpdateTimerDisplay();
    }

    #region API pública
    public void StartTimer()
    {
        isRunning = true;
        UpdateTimerDisplay();
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = Mathf.Max(0, startMinutes * 60);
        isRunning = startOnAwake;
        isFlashing = false;
        timerText.color = originalColor;
        UpdateTimerDisplay();
    }
    #endregion

    #region Utilidades
    void UpdateTimerDisplay()
    {
        float percentage = Mathf.Clamp01(currentTime / totalTime) * 100f;
        timerText.text = $"{percentage:0}%";
    }

    void FlashText()
    {
        // Oscila entre el color original y rojo según el tiempo
        float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
        timerText.color = Color.Lerp(originalColor, Color.red, t);
    }
    #endregion
}
