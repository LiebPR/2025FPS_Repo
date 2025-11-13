using System.Collections;
using TMPro;
using UnityEngine;
using System;

/// <summary>
/// InjectionDoorController: Controla la apertura de la puerta (con animación),
/// el contador visible (mm:ss) y la alarma visual (Point Light parpadeante).
/// </summary>
public class InjectionDoorController : MonoBehaviour
{
    #region Gneral Variables
    [Header("Animación")]
    Animator doorAnimator; // Referencia al Animator de la puerta
    [SerializeField] string openAnimationTrigger = "OpenDoor"; // Nombre del trigger de animación

    [Header("Alarma")]
    [SerializeField] Light alarmLight; // Point Light para la alarma
    [SerializeField] float blinkInterval = 0.35f; // Intervalo de parpadeo de la luz
    bool alarmActive; // Indica si la alarma está activa

    [Header("Contador")]
    [SerializeField] TMP_Text countdownText; // Texto TMP en world space sobre la puerta

    // Estado Interno:
    bool isOpening; // Evita múltiples llamadas simultáneas
    #endregion

    #region Getters
    // Informe público sobre si la alarma está activa.
    public bool IsAlarmActive => alarmActive;
    #endregion

    private void Awake()
    {
        doorAnimator = GetComponent<Animator>();
    }

    #region API
    // El contador mostrará el tiempo restante en formato mm:ss y la alarma parpadeará durante todo el proceso
    public void OpenDoor()
    {
        if (isOpening) return;
        StopAllCoroutines();
        StartCoroutine(OpenDoorAndCountdown());
    }

    // Fuerza detener cualquier proceso y resetea la puerta a la posición inicial (opcional).
    public void StopAndReset()
    {
        StopAllCoroutines();
        alarmActive = false;
        isOpening = false;
        if (alarmLight != null) alarmLight.enabled = false;
        if (countdownText != null) countdownText.text = "";

        // Resetea la animación a la posición inicial
        if (doorAnimator != null) doorAnimator.SetTrigger("ResetDoor");
    }
    #endregion

    #region Core Routine
    IEnumerator OpenDoorAndCountdown()
    {
        isOpening = true;

        AudioManager.Instance.Play("BigElevatorDoor");

        // Activar alarma y corrutina de parpadeo
        alarmActive = true;
        if (alarmLight != null) alarmLight.color = Color.red;
        StartCoroutine(AlarmBlink());

        // Inicia la animación de apertura
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(openAnimationTrigger);
        }

        float openDuration = 60f; // Duración de la animación en segundos (ajustar según la animación)

        // Mostrar inicialmente la cuenta 01:00 (ceil para mostrar 60s al inicio)
        UpdateCountdown(openDuration);

        // Corutina para la cuenta atrás
        float elapsedTime = 0f;
        while (elapsedTime < openDuration)
        {
            elapsedTime += Time.deltaTime;

            // Actualiza el contador
            float remainingTime = Mathf.Max(0f, openDuration - elapsedTime);
            UpdateCountdown(remainingTime);

            yield return null;
        }

        // Asegurar que el contador se detenga
        if (countdownText != null) countdownText.text = "";

        isOpening = false;
    }
    #endregion

    #region Alarm Routine
    IEnumerator AlarmBlink()
    {
        AudioManager.Instance.PlayLoop("Alarma");

        // Protección: si no hay luz, salir
        if (alarmLight == null)
        {
            // Si no hay luz, igualmente mantenemos el flag de alarma hasta que termine la apertura
            while (alarmActive) yield return null;
            yield break;
        }

        // Forzamos color rojo por seguridad
        alarmLight.color = Color.red;

        while (alarmActive)
        {
            alarmLight.enabled = !alarmLight.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    #endregion

    #region Utilities
    // Actualiza el TMP_Text con formato mm:ss. Recibe segundos (float)
    void UpdateCountdown(float seconds)
    {
        if (countdownText == null) return;
        seconds = Mathf.Max(0f, seconds);
        int total = Mathf.CeilToInt(seconds); // ceil para que el contador muestre 01:00 al inicio
        int mm = total / 60;
        int ss = total % 60;
        countdownText.text = $"{mm:00}:{ss:00}";
    }
    #endregion
}