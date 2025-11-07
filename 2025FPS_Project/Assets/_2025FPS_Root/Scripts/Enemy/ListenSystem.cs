using System;
using UnityEngine;

/// <summary>
/// ListenSystem: Sistema de percepción auditiva del enemigo.
/// Detecta ruidos (jugador corriendo o disparos) dentro de un radio,
/// y tambien reacciona a impactos de bala que impactan contra el. 
/// </summary>
public class ListenSystem : MonoBehaviour
{
    #region State Variables
    bool iListen; //flag que controla si se está escuchando o no
    bool hasListenedOnce; //previene llamadas multiples de eventos inecesarias
    bool listenEnabled = true; //flag para permitir escuchar
    float distanceToPlayer; //distancia real al jugador
    float lostTimer = 0f;
    #endregion

    #region Referencias
    [SerializeField] Enemy enemyData;

    FPSController fpsController;
    GunSystem gunSystem;
    VisionSystem vision;
    #endregion

    #region Events
    public event Action<Transform> OnListenPlayer; //cuando el enemigo escucha al jugador
    public event Action<Transform> OnStopListen; //cuando deja de escucharlo
    #endregion

    private void Awake()
    {
        fpsController = FindFirstObjectByType<FPSController>();
        gunSystem = FindFirstObjectByType<GunSystem>();
        vision = GetComponent<VisionSystem>();
    }

    private void Update()
    {
        if(vision == null || fpsController == null) return;

        EvaluateHearing();
    }

    #region Hearing Logic
    void EvaluateHearing()
    {
        if (!listenEnabled)
        {
            iListen = false;
            hasListenedOnce = false;
            lostTimer = 0f;
            return;
        }

        if(fpsController != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, fpsController.transform.position);
        }
        else
            distanceToPlayer = float.MaxValue;

        bool previousListen = iListen;
        iListen = false;

        //Condiciones de escuchar por rango
        //Disparos:
        if (gunSystem.IsShooting && distanceToPlayer <= enemyData.longHearingRange)
        {
            iListen = true;
        }
        //Sprint:
        else if (fpsController.IsSprinting && distanceToPlayer <= enemyData.mediumHearingRange)
        {
            iListen = true;
        }
        //Caminar: 
        else if (fpsController.HasMovementInput() && !fpsController.IsSprinting && !fpsController.IsCrouching && distanceToPlayer <= enemyData.closeHearingRange)
        {
            iListen = true;
        }

        // Manejo de temporizador de pérdida
        if (iListen)
        {
            lostTimer = enemyData.lostHearingDelay; // resetea temporizador si todavía escucha
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
                if (enemyData.debugLogs) Debug.Log($"[ListenSystem] Escuchando al jugador a {distanceToPlayer:F1}m.");
            }
        }
        else if (!iListen && previousListen)
        {
            hasListenedOnce = false;
            OnStopListen?.Invoke(fpsController.transform);
            if (enemyData.debugLogs) Debug.Log("[ListenSystem] Se dejó de escuchar al jugador");
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
        if(enemyData == null) return; 
        // Si no está activo, dibujar todo en gris
        if (!listenEnabled)
        {
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, enemyData.closeHearingRange);
            Gizmos.DrawWireSphere(transform.position, enemyData.mediumHearingRange);
            Gizmos.DrawWireSphere(transform.position, enemyData.longHearingRange);
            return;
        }

        // Escucha activa: dibujar colores normales
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.closeHearingRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.mediumHearingRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, enemyData.longHearingRange);
    }
    #endregion
}
