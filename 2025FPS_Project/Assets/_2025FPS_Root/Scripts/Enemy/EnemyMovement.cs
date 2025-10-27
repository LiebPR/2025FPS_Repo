using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyMovement: Gestiona el movimiento del enemigo entre estados (Patrulla, Idle y Presecución)
/// utilizando un NavMeshAgent y un sistema de detección.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    #region Variables Generales
    [SerializeField] Enemy enemyData;

    [Header("Movement Speed")]
    [SerializeField] float patrolSpeed = 2f; //vel. de patrulla
    [SerializeField] float chaseSpeed = 5f; //vel. de perseguir

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 10f; //radio máximo de generación de puntos a perseguir
    Vector3 walkPoint; //posición del punto random a perseguir
    bool walkPointSet;

    [Header("Idle Stats")]
    [SerializeField] float idleTime = 1f;

    [Header("Stop Behavior")]
    [SerializeField] float minStopDistance = 1.5f;
    [SerializeField] float maxStopDistance = 3.5f;
    [SerializeField] float stopTransitionTime = 0.5f;
    bool isStopping;
    float currentStopDistance;
    float stopTimer;

    [Header("Stuck Detection")]
    [SerializeField] float stuckCheckTime = 2f; //tiempo que el agente espera para comprobar si está stuck
    [SerializeField] float stuckThreshold = 0.1f; //margen de detección de stuck
    [SerializeField] float maxStuckDuration = 3f; //tiempo máximo de estar stuck

    float stuckTimer; //reloj que cuenta el tiempo de estar stuck
    float lastCheckTime; //tiempo de chequeo previo de stuck
    float idleTimer;
    Vector3 lastPosition; //posición del último walkPoint perseguido
    #endregion

    #region References
    NavMeshAgent agent;
    EnemyStateMachine fsm;
    VisionSystem vision;
    #endregion

    #region Events
    public event Action OnIdleEnter;
    public event Action OnIdleExit;
    #endregion

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<EnemyStateMachine>();
        vision = GetComponent<VisionSystem>();

        lastPosition = transform.position;
        lastCheckTime = Time.time;
    }

    private void Update()
    {
        switch (fsm.currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
        }

        CheckIfStuck();
    }
    #region Patroling
    //Estado patrulla: Busca puntos aleatorios de patrulla en el terreno y se mueve entre ellos lentamente 
    void HandlePatrol()
    {
        agent.speed = patrolSpeed;

        // Generar un punto si no hay walkPoint
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        // Siempre intentar moverse al walkPoint
        if (walkPointSet)
        {
            if (agent.destination != walkPoint)
            {
                agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, Time.deltaTime * 3f);
                agent.SetDestination(walkPoint);
            }
                

            // Llego al punto, pasa a idle
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                walkPointSet = false;
                idleTimer = idleTime;
                OnIdleEnter?.Invoke();
            }
        }
    }

    void SearchWalkPoint()
    {
        int attempts = 0;
        const int maxAttempts = 5;

        while (!walkPointSet && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomPoint = transform.position + new Vector3(UnityEngine.Random.Range(-walkPointRange, walkPointRange), 0, UnityEngine.Random.Range(-walkPointRange, walkPointRange));

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                // Chequeo de suelo
                if (Physics.Raycast(hit.position + Vector3.up * 0.5f, Vector3.down, 1f, enemyData.groundLayer))
                {
                    walkPoint = hit.position;
                    walkPointSet = true;

                    agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, Time.deltaTime * 3f);
                    agent.SetDestination(walkPoint);
                }
            }
        }
    }
    #endregion

    #region Idle
    void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        
        agent.speed = Mathf.Lerp(agent.speed, 0f, Time.deltaTime * 2f);
        agent.SetDestination(transform.position); //Detener movimiento mientras está idle

        if (idleTimer <= 0)
        {
            OnIdleExit?.Invoke();
        }
    }
    #endregion  

    #region Chase
    void HandleChase()
    {
        if (vision.Target == null) return;

        float distance = Vector3.Distance(transform.position, vision.Target.position);

        agent.speed = chaseSpeed;

        // Si el player entra en el área de parada
        if (vision.IsPlayerInStopArea)
        {
            if (!isStopping)
            {
                // Genera una distancia aleatoria y marca que se está deteniendo
                currentStopDistance = UnityEngine.Random.Range(minStopDistance, maxStopDistance);
                isStopping = true;
                stopTimer = 0f; //reinicia temporizador de frenado
            }

            if(distance < currentStopDistance)
            {
                stopTimer += Time.deltaTime;

                //Calcular el factor de interpolación
                float t = Mathf.Clamp01(stopTimer / stopTransitionTime);

                //Lerp de velocidad: de chaseSpeed a 0
                agent.speed = Mathf.Lerp(chaseSpeed, 0f, t);

                //Mantiene al agente activo para conservar rotación automática
                agent.SetDestination(transform.position);

                //Rotación suave hacia el jugador
                Vector3 dir = vision.Target.position - transform.position;
                dir.y = 0;
                if(dir.sqrMagnitude > 0.01f)
                {
                    Quaternion look = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, look, 5f * Time.deltaTime);
                }
                return;
            }
        }
        else
        {
            isStopping = false;
            stopTimer = 0f;
        }
        //Si esta afuera del rango de parada, retoma persecución
        agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, Time.deltaTime * 3f);
        agent.SetDestination(vision.Target.position);
    }
    #endregion

    #region Stuck Detection
    void CheckIfStuck()
    {
        if(Time.time - lastCheckTime > stuckCheckTime)
        {
            float distMoved = Vector3.Distance(transform.position, lastPosition);

            if (distMoved < stuckThreshold && agent.hasPath)
                stuckTimer += stuckCheckTime;
            else
                stuckTimer = 0;

            if(stuckTimer >= maxStuckDuration)
            {
                walkPointSet = false;
                agent.SetDestination(transform.position);
                stuckTimer = 0f;
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }
    #endregion
}
