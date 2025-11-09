using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyMovement: Gestiona el movimiento del enemigo entre estados (Patrulla, Idle y Persecución)
/// utilizando un NavMeshAgent y un sistema de detección.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    #region State Variables
    //Puntos: 
    Vector3 walkPoint; //posición del punto random a perseguir
    Vector3 lastPosition; //posición del último walkPoint perseguido

    //Distancia: 
    float currentStopDistance;

    //Temporizadores:
    float stopTimer;
    float stuckTimer; //reloj que cuenta el tiempo de estar stuck
    float lastCheckTime; //tiempo de chequeo previo de stuck
    float idleTimer;

    //Flags
    bool walkPointSet;
    bool isStopping;
    #endregion

    #region References
    [SerializeField] Enemy enemyData;

    NavMeshAgent agent;
    EnemyStateMachine fsm;
    VisionSystem vision;

    // Materiales
    [SerializeField] Material chaseMaterial; // Material para el estado de persecución
    [SerializeField] Material idleMaterial;  // Material para los estados de idle o patrullaje
    Material[] enemyMaterials; // Materiales del enemigo

    Renderer[] renderers; // Para acceder a los renderers del enemigo y su hijo
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

        renderers = GetComponentsInChildren<Renderer>(); // Obtener todos los renderers del enemigo y sus hijos
        enemyMaterials = new Material[renderers.Length];

        // Guardamos los materiales originales del enemigo para restaurarlos si es necesario
        for (int i = 0; i < renderers.Length; i++)
        {
            enemyMaterials[i] = renderers[i].material;
        }

        // Asignamos el material correspondiente al estado inicial
        ApplyMaterialToSelfAndChildren(idleMaterial);
    }

    private void Update()
    {
        // Verificar si el estado actual ha cambiado
        bool wasChasing = fsm.currentState == EnemyState.Chase;
        bool wasIdle = fsm.currentState == EnemyState.Idle;

        // Realiza el cambio de estado
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

        // Si el estado anterior era Chase y ahora no lo es, restaurar materiales de idle
        if (wasChasing && fsm.currentState != EnemyState.Chase)
        {
            ApplyMaterialToSelfAndChildren(idleMaterial); // Cambiar a material de Idle cuando no esté en Chase
        }
        // Si el estado es Chase, aplicar el material de Chase
        else if (fsm.currentState == EnemyState.Chase)
        {
            ApplyMaterialToSelfAndChildren(chaseMaterial); // Cambiar a material de Chase
        }

        // Si el estado anterior era Idle y ahora no lo es, restaurar materiales de idle
        if (wasIdle && fsm.currentState != EnemyState.Idle)
        {
            ApplyMaterialToSelfAndChildren(idleMaterial); // Cambiar a material de Idle al salir de Idle
        }

        CheckIfStuck();
    }

    #region Chase
    void HandleChase()
    {
        if (vision.Target == null) return;

        float distance = Vector3.Distance(transform.position, vision.Target.position);

        agent.speed = enemyData.chaseSpeed;

        // Si está en Chase, ya habremos cambiado el material en Update

        // Si el player entra en el área de parada
        if (vision.IsPlayerInStopArea)
        {
            if (!isStopping)
            {
                // Genera una distancia aleatoria y marca que se está deteniendo
                currentStopDistance = UnityEngine.Random.Range(enemyData.minStopDistance, enemyData.maxStopDistance);
                isStopping = true;
                stopTimer = 0f; //reinicia temporizador de frenado
            }

            if (distance < currentStopDistance)
            {
                stopTimer += Time.deltaTime;

                //Calcular el factor de interpolación
                float t = Mathf.Clamp01(stopTimer / enemyData.stopTransitionTime);

                //Lerp de velocidad: de chaseSpeed a 0
                agent.speed = Mathf.Lerp(enemyData.chaseSpeed, 0f, t);

                //Mantiene al agente activo para conservar rotación automática
                agent.SetDestination(transform.position);

                //Rotación suave hacia el jugador
                Vector3 dir = vision.Target.position - transform.position;
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
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

        // Si esta fuera del rango de parada, retoma persecución
        agent.speed = Mathf.Lerp(agent.speed, enemyData.chaseSpeed, Time.deltaTime * 3f);
        agent.SetDestination(vision.Target.position);
    }
    #endregion

    #region Patroling
    void HandlePatrol()
    {
        agent.speed = enemyData.patrolSpeed;

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
                agent.speed = Mathf.Lerp(agent.speed, enemyData.chaseSpeed, Time.deltaTime * 3f);
                agent.SetDestination(walkPoint);
            }

            // Llego al punto, pasa a idle
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                walkPointSet = false;
                idleTimer = enemyData.idleTime;
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
            Vector3 randomPoint = transform.position + new Vector3(UnityEngine.Random.Range(-enemyData.walkPointRange, enemyData.walkPointRange), 0, UnityEngine.Random.Range(-enemyData.walkPointRange, enemyData.walkPointRange));

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                // Chequeo de suelo
                if (Physics.Raycast(hit.position + Vector3.up * 0.5f, Vector3.down, 1f, enemyData.groundLayer))
                {
                    walkPoint = hit.position;
                    walkPointSet = true;

                    agent.speed = Mathf.Lerp(agent.speed, enemyData.chaseSpeed, Time.deltaTime * 3f);
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

    #region Stuck Detection
    void CheckIfStuck()
    {
        if (Time.time - lastCheckTime > enemyData.stuckCheckTime)
        {
            float distMoved = Vector3.Distance(transform.position, lastPosition);

            if (distMoved < enemyData.stuckThreshold && agent.hasPath)
                stuckTimer += enemyData.stuckCheckTime;
            else
                stuckTimer = 0;

            if (stuckTimer >= enemyData.maxStuckDuration)
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

    #region Public Methods
    // Detiene el movimiento del enemigo y lo mantiene en su posición actual.
    public void StopMovement()
    {
        agent.isStopped = true;  // Detiene al agente de navegación
        agent.velocity = Vector3.zero;  // Detiene la velocidad del agente (si es necesario)

        // Detener la rotación (mantiene la rotación actual)
        agent.angularSpeed = 0f;  // Detiene la rotación automática del NavMeshAgent
        transform.rotation = transform.rotation;  // Asegura que no se realicen cambios en la rotación
    }

    //Reactiva el movimiento del enemigo y permite que continúe con su destino.
    public void ResumeMovement()
    {
        agent.isStopped = false;  // Reactiva el agente de navegación
        agent.angularSpeed = enemyData.angularSpeed;
    }
    #endregion

    //Aplica el material proporcionado tanto al enemigo como a sus hijos
    private void ApplyMaterialToSelfAndChildren(Material material)
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.material = material;
        }
    }
}
