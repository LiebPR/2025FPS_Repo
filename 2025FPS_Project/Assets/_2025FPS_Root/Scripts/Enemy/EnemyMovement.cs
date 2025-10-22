using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    #region Variables Generales
    [SerializeField] Enemy enemyData;

    [Header("Patroling Stats")]
    [SerializeField] float walkPointRange = 10f; //radio máximo de generación de puntos a perseguir
    Vector3 walkPoint; //posición del punto random a perseguir
    bool walkPointSet;

    [Header("Idle Stats")]
    [SerializeField] float idleTime = 1f;

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
    void HandlePatrol()
    {
        // Generar un punto si no hay walkPoint
        if (!walkPointSet)
        {
            SearchWalkPoint();
        }

        // Siempre intentar moverse al walkPoint
        if (walkPointSet)
        {
            if (agent.destination != walkPoint)
                agent.SetDestination(walkPoint);

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
        agent.ResetPath(); //Detener movimiento mientras está idle

        if(idleTimer <= 0)
        {
            OnIdleExit?.Invoke();
        }
    }
    #endregion  

    #region Chase
    void HandleChase()
    {
        if (vision.Target == null) return;
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
                agent.ResetPath();
                stuckTimer = 0f;
            }

            lastPosition = transform.position;
            lastCheckTime = Time.time;
        }
    }
    #endregion
}
