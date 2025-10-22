using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// VisionSystem: Detecta objetos de un radio y ángulo de visión.
/// Dispara eventos específicos para el jugador y mantiene la última posición conocida.
/// </summary>
public class VisionSystem : MonoBehaviour
{
    #region General Variables
    [SerializeField] Enemy enemyData;

    [Header("Config")]
    [SerializeField] float perceptionDelay = 0.5f;
    [SerializeField] float lostDelay = 0.5f;
    [SerializeField] Transform visionPoint;

    float lostTimer = 0f;
    float perceptionTimer = 0f;

    bool canSeeTarget;
    bool isPlayerInPerceptionArea;

    RaycastHit rayObstacleDetector;
    #endregion

    #region References
    NavMeshAgent agent;
    EnemyStateMachine stateMachine;
    #endregion

    #region Getters
    public Transform Target {  get; private set; } //player
    public Vector3 LastKnownPosition { get; private set; } //última posición conocida del player
    public bool CanSeeTarget => canSeeTarget; //informa del si es true or false
    #endregion

    #region Eventos
    public event Action<Transform> OnTargetSee; //se suscribe cuando se ve al target
    public event Action<Transform> OnTargetLose; //se suscribe cuando se pierde al target
    #endregion

    private void Awake()
    {
        //Referencias: 
        agent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<EnemyStateMachine>();

        FindPlayer(); //busca al player automáticamente
    }

    private void Update()
    {
        if(Target != null)
        {
            EvaluateVision();
        }
    }

    #region Vision Evaluation
    void EvaluateVision()
    {
        Vector3 dirToTarget = (Target.position - visionPoint.position).normalized;
        float distToTarget = Vector3.Distance(visionPoint.position, Target.position);

        bool obstacle = CheckObstacle(dirToTarget, distToTarget, out rayObstacleDetector);
        bool inCone = CheckCone(dirToTarget, distToTarget);
        bool inPerceptionArea = PerceptionArea();

        UpdateVisionState(inCone, obstacle, inPerceptionArea);
    }

    void UpdateVisionState(bool inCone, bool obstacle, bool inPerceptionArea)
    {
        bool previusSee = canSeeTarget;

        //Detección Frontal
        if(inCone && !obstacle)
        {
            lostTimer = lostDelay;
            canSeeTarget = true;
        }
        else
        {
            lostTimer -= Time.deltaTime;
            if(lostTimer <= 0f)
            {
                canSeeTarget = false;
                lostTimer = 0f;
            }
        }

        //Percepción Cercana
        if(inPerceptionArea && !canSeeTarget)
        {
            if (!isPlayerInPerceptionArea)
            {
                isPlayerInPerceptionArea = true;
                perceptionTimer = perceptionDelay;
            }

            perceptionTimer -= Time.deltaTime;
            if(perceptionTimer <= 0f)
            {
                canSeeTarget = true;
                perceptionTimer = perceptionDelay;
            }
        }
        else
        {
            isPlayerInPerceptionArea = false;
            perceptionTimer = perceptionDelay;
        }

        //Eventos
        if(canSeeTarget && !previusSee)
        {
            OnTargetSee?.Invoke(Target);
            LastKnownPosition = Target.position;
        }
        else if(!canSeeTarget && previusSee)
        {
            OnTargetLose?.Invoke(Target);
            LastKnownPosition = Target.position;
        }
    }
    #endregion

    #region Vision Detectors

    //OBSTACLE DETECTOR
    //Detecta si hay un obstáculo entre el enemigo y el jugador. 
    bool CheckObstacle(Vector3 dirToTarget, float distToTarget, out RaycastHit hit)
    {
        //Raycast hasta la posición exacta del player
        bool hasHit = Physics.Raycast(visionPoint.position, dirToTarget, out hit, distToTarget, enemyData.obstacleMask);
        return hasHit;
    }

    //CONO
    //Verifica si el juagdor está dentro del ángulo de vision del enemigo.
    bool CheckCone(Vector3 dirToTarget, float distanceToTarget)
    {
        if (distanceToTarget > enemyData.visionRadius) return false; //fuera del rango máximo

        //Claculamos el ángulo entre el frente del enemigo y el objetivo
        float angle = Vector3.Angle(visionPoint.transform.forward, dirToTarget);

        //Comprobamos si el jugador está dentro del ángulo de visión
        return angle < enemyData.visionAngle * 0.5f;
    }

    //PERCEPTION AREA
    //Detecta si el jugador está cerca del enemigo
    bool PerceptionArea()
    {
        if(Target == null) return false;

        Vector3 enemyCenter = visionPoint.position; //centro del enemigo
        float perceptionRadius = enemyData.perceptionRadius;

        Collider[] hits = Physics.OverlapSphere(enemyCenter, perceptionRadius, LayerMask.GetMask("Player"));//devuelve el primer collider del jugador
        Collider hit = hits.Length > 0 ? hits[0] : null; 
        if (hit != null && hit.transform == Target)
        {
            //Verificamos que no haya obstáculos usando el raycast 3D existente
            Vector3 dirToTarget = (Target.position - visionPoint.position).normalized;
            float distanceToTarget = Vector3.Distance(visionPoint.position, Target.position);

            if(!CheckObstacle(dirToTarget, distanceToTarget, out _))
            {
                //Se respeta la lógica de tiempo para "darse cuenta"
                if (!canSeeTarget)
                {
                    perceptionTimer -= Time.deltaTime;
                    return true;
                }
            }
            else
            {
                //Si ya lo ve, reiniciamos el temporizador
                perceptionTimer = perceptionDelay;
                return true; 
            }
        }

        //Si hay detección válida, reiniciamos el temporizador
        perceptionTimer = perceptionDelay;
        return false;
    }
    #endregion

    #region Utilities

    void FindPlayer()
    {
        FPSController player = GameObject.FindFirstObjectByType<FPSController>();
        if(player != null) Target = player.transform;
    }

    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (visionPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.visionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.perceptionRadius);

        //Visualización del cono de visión
        Vector3 rightDir = Quaternion.Euler(0, enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;
        Vector3 leftDir = Quaternion.Euler(0, -enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + rightDir * enemyData.visionRadius);
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + leftDir * enemyData.visionRadius);

        //Raycast de obstáculos
        if(Target != null)
        {
            Gizmos.color = rayObstacleDetector.collider != null ? Color.magenta : Color.red;
            Vector3 rayEnd = rayObstacleDetector.collider != null ? rayObstacleDetector.point : Target.position;
            Gizmos.DrawLine(visionPoint.position, rayEnd);
        }

        //Estado general
        Gizmos.color = canSeeTarget ? Color.green : new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.visionRadius);
    }
    #endregion
}
