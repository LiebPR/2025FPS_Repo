using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
public class Enemy : ScriptableObject
{
    public bool debugLogs; //activar logs de depuración

    //SISTEMAS DE PERCEPCIÓN

    [Header("Vision Settings")]
    public float visionRadius = 6f; //rango máximo de visión frontal
    public float visionAngle = 45f; //ángulo de visión del cono frontal
    public float perceptionRadius = 1f; //rango de percepción cercana
    public LayerMask obstacleMask;

    [Header("Stop Area")]
    public float stopAreaRadius = 2f; //radio del área donde el enemigo se detiene
    public float minStopDistance = 1.5f; //distancia minima de parada
    public float maxStopDistance = 3.5f; //distancia máxima de parada
    public float stopTransitionTime = 0.5f; //tiempo de transición al frenar

    [Header("Vision Timers")]
    public float perceptionDelay = 0.5f;
    public float lostDelay = 0.5f;

    [Header("Hearing Settings")]
    public float closeHearingRange = 5f; //escucha cualquier ruido cercano
    public float mediumHearingRange = 10f; //escucha ruidos medios
    public float longHearingRange = 20f; //escucha disparos lejanos
    public float lostHearingDelay = 1f; //tiempo para perder la escucha

    //MOVIMIENTO Y ESTADOS

    public LayerMask groundLayer;

    [Header("Patrol Settings")]
    public float patrolSpeed = 2f; //velocidad en patrol
    public float walkPointRange = 10f; //radio máximo para generar puntos de patrulla

    [Header("Chase Settings")]
    public float chaseSpeed = 5f; //velocidad en chase

    [Header("Idle Settings")]
    public float idleTime = 1f; //tiempo de idle

    [Header("Stuck Detection")]
    public float stuckCheckTime = 2f; //tiempo para comprobar si está stuck
    public float stuckThreshold = 0.1f; //margen para detectar stuck
    public float maxStuckDuration = 3f; //tiempo máximo que puede estar stuck

    //ATAQUE

    [Header("Attack Settings")]
    public LayerMask impactLayer; //capas con las que puede colisionar el disparo.
    public int damage = 10; //daño que hace cada disaparo
    public float range = 6f; //alcance del raycast del disparo
    public float shootingCooldown = 1f; //tiempo entre disparos
    public float attackRange = 9f; //distancia máxima a la que puede atacar
}
