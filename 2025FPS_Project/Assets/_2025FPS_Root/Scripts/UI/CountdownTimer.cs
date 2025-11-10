using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// CountdownTimer: Temporizador con barra de progreso y porcentaje.
/// Al llegar a 0, muestra Game Over y notifica al GameManager.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    #region General Variables
    [SerializeField] TextMeshProUGUI timerText; //referencia al TMPRO de la UI
    [SerializeField] Image progressBar; //imagen UI con Fill Amount
    [SerializeField] int startMinutes = 10; //minutos iniciales (ej: 10 -> 10:00)
    [SerializeField] bool startOnAwake = true; //arranca automáticamente al iniciar
    #endregion

    #region Variables internas
    float currentTime;
    float totalTime;
    bool isRunning;
    bool isFlashing;
    bool isGameOverTriggered;
    Color originalColor;
    float flashSpeed = 2f;
    bool isOxygenBeingConsumed;
    #endregion

    #region Getter
    public bool IsFull => currentTime >= totalTime - 0.0001f;
    #endregion

    void Start()
    {
        if (!timerText || !progressBar)
        {
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
        if (!isRunning || isGameOverTriggered) return; //no avanza si el oxígeno está siendo consumido

        currentTime -= Time.deltaTime;

        if (currentTime <= 10f && !isFlashing)
        {
            isFlashing = true;
        }
            

        if (isFlashing)
            FlashText();

        if (currentTime <= 0f)
        {
            TriggerGameOver();
            return;
        }

        UpdateTimerDisplay();
    }

    #region GameOver Logic
    void TriggerGameOver()
    {
        currentTime = 0f;
        isRunning = false;
        isFlashing = false;
        isGameOverTriggered = true;
        timerText.color = Color.red;

        UpdateTimerDisplay();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
    #endregion

    #region Public API
    public void StartTimer()
    {
        isRunning = true;
        isGameOverTriggered = false;
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
        isGameOverTriggered = false;
        timerText.color = originalColor;
        UpdateTimerDisplay();
    }

    //Añade tiempo extra al temporizador
    public bool AddTime(float seconds)
    {
        if (isGameOverTriggered) return false;
        currentTime = Mathf.Min(currentTime + seconds, totalTime);
        UpdateTimerDisplay();
        return IsFull;
    }

    // Controla el consumo de oxígeno y detiene el temporizador
    public void StartOxygenConsumption()
    {
        isOxygenBeingConsumed = true;
    }

    public void StopOxygenConsumption()
    {
        isOxygenBeingConsumed = false;
    }
    #endregion

    #region Utilidades
    void UpdateTimerDisplay()
    {
        float normalized = Mathf.Clamp01(currentTime / totalTime);
        float percentage = normalized * 100f;

        //Parpadeo solo si está debajo del 10%
        if (percentage <= 10f)
        {
            if (!isFlashing)
                isFlashing = true;
        }
        else
        {
            if (isFlashing)
            {
                isFlashing = false;
                timerText.color = originalColor;
            }
        }

        timerText.text = $"{percentage:0}%";

        if (progressBar)
            progressBar.fillAmount = normalized;
    }


    void FlashText()
    {
        float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
        timerText.color = Color.Lerp(originalColor, Color.red, t);
    }
    #endregion
}
