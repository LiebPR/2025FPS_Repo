using System;
using UnityEngine;

public class ListenSystem : MonoBehaviour
{
    #region General Variables
    [Header("Listen Settings")]
    [SerializeField] float listenRadius = 10f; //radio de escucha
    [SerializeField] float lostDelay = 1f; //tiempo de perdida
    [SerializeField] LayerMask targetLayer; 

    float lostTimer = 0f;
    bool iListen; //estado de detección actual
    bool isEnable = true; //Control interno del sistema
    #endregion

    #region Eventos
    public event Action<Vector3> OnListenPlayer; //cuando se escucha al jugador
    public event Action<Vector3> OnBulletImpact; //cuando detecta un jugador
    public event Action OnDontListenAnything; //no escucha nada
    #endregion

    private void Update()
    {
        if (!isEnable) return; //Ignoramos toda la lógica si está desactivado

        Collider[] hits = DetectionAudio(); //detecta jugadores y arma en el radio
        bool detectedThisFrame = ProcessDetection(hits); //procesa detecciones y dispara eventos
        HandleLostDetection(detectedThisFrame); //gestiona pérdida de detección
    }

    #region Public Controls
    //Activa o desactiva la percepción auditiva.
    public void SetListenActive(bool active)
    {
        isEnable = active;
        if (!active)
        {
            //reiniciamos variables al apagar
            iListen = false;
            lostTimer = 0f;
        }
    }
    #endregion

    #region Detección
    Collider[] DetectionAudio()
    {
        return Physics.OverlapSphere(transform.position, listenRadius, targetLayer);
    }

    bool ProcessDetection(Collider[] hits)
    {
        bool detected = false;

        foreach (var hit in hits)
        {
            FPSController player = hit.GetComponent<FPSController>();
            GunSystem gun = hit.GetComponent<GunSystem>();

            if(player != null)
            {
                if(player.IsSprinting || (gun != null && gun.IsShooting))
                {
                    detected = true;
                    if (!iListen)
                    {
                        OnListenPlayer?.Invoke(player.transform.position);
                        iListen = true;
                    }
                }
                if(gun != null && gun.IsShooting)
                {
                    Vector3 impactPoint = gun.LastHitPoint;
                    OnListenPlayer?.Invoke(impactPoint);
                    detected = true;
                }
            }
        }
        return detected;
    }

    void HandleLostDetection(bool detectedThisFrame)
    {
        if (detectedThisFrame)
        {
            lostTimer = lostDelay;
        }
        else
        {
            lostTimer -= Time.deltaTime;
            if(iListen && lostTimer <= 0f)
            {
                iListen = false;
                lostTimer = 0f;
                OnDontListenAnything?.Invoke();
            }
        }
    }
    #endregion


    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (!isEnable)
        {
            // Desactivado → color gris-azulado transparente
            Gizmos.color = new Color(0, 0, 1, 0.15f);
        }
        else if (iListen)
        {
            // Escuchando activamente → rojo
            Gizmos.color = Color.red;
        }
        else
        {
            // Activo pero sin escuchar → azul
            Gizmos.color = Color.blue;
        }

        Gizmos.DrawWireSphere(transform.position, listenRadius);
    }
    #endregion
}
