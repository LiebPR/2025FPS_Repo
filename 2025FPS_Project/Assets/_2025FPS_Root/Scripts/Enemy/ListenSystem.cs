using System;
using UnityEngine;

/// <summary>
/// ListenSystem: Sistema de percepción auditiva del enemigo.
/// Detecta ruidos (jugador corriendo o disparos) dentro de un radio,
/// y tambien reacciona a impactos de bala que impactan contra el. 
/// </summary>
public class ListenSystem : MonoBehaviour
{
    #region General Variables
    [Header("Hearing Ranges")]
    [SerializeField] float closeRange = 25f; //escucha cualquier ruido
    [SerializeField] float mediumRange = 60f; //escucha alguno ruidos
    [SerializeField] float longRange = 100f; //ecucha los disparos

    [Header("Audio Lost Settings")]
    [SerializeField] float lostDelay = 1.5f; //tiempo de perder la escucha

    [Header("Activate logs")]
    [SerializeField] bool debugLogs = false;

    bool iListen; //flag que controla si se está escuchando o no
    bool hasListenedOnce; //previene llamadas multiples de eventos inecesarias
    bool listenEnabled = true; //flag para permitir escuchar
    float distanceToPlayer; //distancia real al jugador
    float lostTimer = 0f;
    #endregion

    #region Referencias
    VisionSystem vision;
    FPSController fpsController;
    GunSystem gunSystem;
    #endregion

    #region Events
    public event Action<Transform> OnListenPlayer; //cuando el enemigo escucha al jugador
    public event Action<Transform> OnStopListen; //cuando deja de escucharlo
    #endregion

    private void Awake()
    {
        vision = GetComponent<VisionSystem>();
        fpsController =FindFirstObjectByType<FPSController>();
        gunSystem = FindFirstObjectByType<GunSystem>();
    }

    private void Update()
    {
        if(vision == null || fpsController == null) return;

        EvaluateHearing();
    }

    #region Core Logic
    void EvaluateHearing()
    {
        if (!listenEnabled)
        {
            iListen = false;
            hasListenedOnce = false;
            lostTimer = 0f;
            return;
        }

        //Distancia obtenida desde la última posición conocida del VisionSystem
        distanceToPlayer = Vector3.Distance(transform.position, vision.LastKnownPosition);

        bool previousListen = iListen;
        iListen = false;

        //Condiciones de escuchar por rango
        if(gunSystem.IsShooting && distanceToPlayer <= longRange)
        {
            iListen = true;
        }
        else if(distanceToPlayer <= mediumRange && fpsController.IsSprinting)
        {
            iListen = true;
        }
        else if (distanceToPlayer <= closeRange && fpsController.HasMovementInput() && !fpsController.IsCrouching)
        {
            iListen = true;
        }

        // Manejo de temporizador de pérdida
        if (iListen)
        {
        lostTimer = lostDelay; // resetea temporizador si todavía escucha
        }
        else
        {
            if (listenEnabled) // solo decrementa si la escucha está habilitada
            {
                lostTimer -= Time.deltaTime;
                if (lostTimer > 0f)
                {
                    iListen = true; // sigue escuchando mientras el timer no expire
                }
            }
        }
        

        //Control de eventos
        if (iListen && !previousListen)
        {
            if (!hasListenedOnce)
            {
                hasListenedOnce = true;
                OnListenPlayer?.Invoke(fpsController.transform);
                if (debugLogs) Debug.Log($"[ListenSystem] Escuchando al jugador a {distanceToPlayer:F1}m.");
            }
        }
        else if (!iListen && previousListen)
        {
            hasListenedOnce = false;
            OnStopListen?.Invoke(fpsController.transform);
            if (debugLogs) Debug.Log("[ListenSystem] Se dejó de escuchar al jugador");
        }
    }
    #endregion

    #region Public Controls
    public void SetListenActive(bool active)
    {
        listenEnabled = active;

        if (!active)
        {
            iListen = false;
            hasListenedOnce = false;
            lostTimer = 0f;
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        // Si no está activo, dibujar todo en gris
        if (!listenEnabled)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, closeRange);
            Gizmos.DrawWireSphere(transform.position, mediumRange);
            Gizmos.DrawWireSphere(transform.position, longRange);
            return;
        }

        // Escucha activa: dibujar colores normales
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, mediumRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, longRange);
    }
    #endregion
}
