using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// InjectionDoorController: Controla la apertura de la puerta (el propio GameObject),
/// el contador visible (mm:ss) y la alarma visual (Point Light parpadeante).
/// </summary>
public class InjectionDoorController : MonoBehaviour
{
    #region Gneral Variables
    [Header("Movimiento")]
    [SerializeField] float openHeight = 3f; //altura total a desplazar en unidades
    [SerializeField] float openDuration = 60f; //duración de la apertura en segundos (ej. 60 = 01:00)

    [Header("Alarma")]
    [SerializeField] Light alarmLight; //point Light para la alarma
    [SerializeField] float blinkInterval = 0.35f; //intervalo de parpadeo de la luz
    bool alarmActive; //indica si la alarma está activa

    [Header("Contador")]
    [SerializeField] TMP_Text countdownText; //texto TMP en world space sobre la puerta

    //Estado Interno: 
    bool isOpening; //evita múltiples llamadas simultáneas
    #endregion

    #region Getters
    //Informe público sobre si la alarma está activa.
    public bool IsAlarmActive => alarmActive;
    #endregion

    #region API
    //El contador mostrará el tiempo restante en formato mm:ss y la alarma parpadeará durante todo el proceso
    public void OpenDoor()
    {
        if (isOpening) return;
        StopAllCoroutines();
        StartCoroutine(OpenDoorAndCountdown());
    }

    //Fuerza detener cualquier proceso y resetea la puerta a la posición inicial (opcional).
    public void StopAndReset()
    {
        StopAllCoroutines();
        alarmActive = false;
        isOpening = false;
        if (alarmLight != null) alarmLight.enabled = false;
        if (countdownText != null) countdownText.text = "";
    }
    #endregion

    #region Core Routine
    IEnumerator OpenDoorAndCountdown()
    {
        isOpening = true;

        //Activar alarma y corrutina de parpadeo
        alarmActive = true;
        if (alarmLight != null) alarmLight.color = Color.red;
        StartCoroutine(AlarmBlink());

        //Posiciones absolutas (la puerta es este mismo transform)
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * openHeight;

        float elapsed = 0f;

        //Mostrar inicialmente la cuenta 01:00 (ceil para mostrar 60s al inicio)
        UpdateCountdown(openDuration);

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            //Movimiento suave interpolado en el tiempo total
            transform.position = Vector3.Lerp(startPos, endPos, t);

            //Actualizar contador con tiempo restante (ceil para que muestre 01:00 ... 00:01, luego 00:00)
            float remaining = Mathf.Max(0f, openDuration - elapsed);
            UpdateCountdown(remaining);

            yield return null;
        }

        //Asegurar posición final exacta
        transform.position = endPos;

        //Finalizar: apagar alarma y limpiar contador
        alarmActive = false;
        //Dejar un frame para que AlarmBlink detecte el cambio y apague la luz
        yield return null;
        if (countdownText != null) countdownText.text = "";

        isOpening = false;
    }
    #endregion

    #region Alarm Routine
    IEnumerator AlarmBlink()
    {
        AudioManager.Instance.PlayLoop("Alarma");

        //Protección: si no hay luz, salir
        if (alarmLight == null)
        {
            //Si no hay luz, igualmente mantenemos el flag de alarma hasta que termine la apertura
            while (alarmActive) yield return null;
            yield break;
        }

        //Forzamos color rojo por seguridad
        alarmLight.color = Color.red;

        while (alarmActive)
        {
            alarmLight.enabled = !alarmLight.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        //Al terminar aseguramos que la luz quede apagada
        alarmLight.enabled = false;
        AudioManager.Instance.StopLoop("Alarma");

    }
    #endregion

    #region Utilities
    //Actualiza el TMP_Text con formato mm:ss. Recibe segundos (float)
    void UpdateCountdown(float seconds)
    {
        if (countdownText == null) return;
        seconds = Mathf.Max(0f, seconds);
        int total = Mathf.CeilToInt(seconds); //ceil para que el contador muestre 01:00 al inicio
        int mm = total / 60;
        int ss = total % 60;
        countdownText.text = $"{mm:00}:{ss:00}";
    }
    #endregion
}
