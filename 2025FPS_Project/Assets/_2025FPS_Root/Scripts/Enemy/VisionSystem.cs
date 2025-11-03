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
    [SerializeField] Transform visionPoint;
    #region State Variables
    float lostTimer = 0f;
    float perceptionTimer = 0f;
    bool canSeeTarget;
    bool isPlayerInPerceptionArea;
    bool visionEnable = true; 

    RaycastHit rayObstacleDetector;
    #endregion

    #region References
    [SerializeField] Enemy enemyData;

    NavMeshAgent agent;
    EnemyStateMachine stateMachine;
    #endregion

    #region Getters
    public Transform Target {  get; private set; } //player
    public Vector3 LastKnownPosition { get; private set; } //última posición conocida del player
    public bool IsPlayerInStopArea { get; private set; } //geter para movement
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

        bool inCone = CheckCone(dirToTarget, distToTarget);
        bool inPerceptionArea = PerceptionArea();
        bool obstacle = CheckObstacle(dirToTarget, distToTarget, out rayObstacleDetector);

        UpdateVisionState(inCone, obstacle, inPerceptionArea);

        CheckStopArea();
    }

    void UpdateVisionState(bool inCone, bool obstacle, bool inPerceptionArea)
    {
        bool previusSee = canSeeTarget;

        //Detección Frontal
        if(inCone && !obstacle)
        {
            lostTimer = enemyData.lostDelay;
            canSeeTarget = true;
            LastKnownPosition = Target.position;
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
                perceptionTimer = enemyData.perceptionDelay;
            }

            perceptionTimer -= Time.deltaTime;
            if(perceptionTimer <= 0f)
            {
                canSeeTarget = true;
                perceptionTimer = enemyData.perceptionDelay;
            }
        }
        else
        {
            isPlayerInPerceptionArea = false;
            perceptionTimer = enemyData.perceptionDelay;
        }

        //Eventos
        if(canSeeTarget && !previusSee)
        {
            OnTargetSee?.Invoke(Target);
        }
        else if(!canSeeTarget && previusSee && lostTimer <= 0f)
        {
            OnTargetLose?.Invoke(Target);
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

    public void EnableConeVision(bool enable)
    {
        visionEnable = enable;
    }

    //CONO
    //Verifica si el juagdor está dentro del ángulo de vision del enemigo.
    bool CheckCone(Vector3 dirToTarget, float distanceToTarget)
    {
        if (!visionEnable) return false;
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
                perceptionTimer = enemyData.perceptionDelay;
                return true; 
            }
        }

        //Si hay detección válida, reiniciamos el temporizador
        perceptionTimer = enemyData.perceptionDelay;
        return false;
    }
    void CheckStopArea()
    {
        if(!visionEnable || Target == null)
        {
            IsPlayerInStopArea = false;
            return;
        }

        float distance = Vector3.Distance(visionPoint.position, Target.position);
        IsPlayerInStopArea = distance <= enemyData.stopAreaRadius;
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

        //COLOR BASE SEGÚN ESTADO
        Color baseColor;

        if (!visionEnable)
            baseColor = new Color(1f, 1f, 0f, 0.2f); // apagado → amarillo opaco
        else if (canSeeTarget)
            baseColor = Color.green; // viendo al jugador
        else
            baseColor = Color.yellow; // activo pero sin ver

        // Radio de visión
        Gizmos.color = baseColor;
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.visionRadius);

        // Área de percepción
        Gizmos.color = new Color(0, 1, 1, visionEnable ? 1f : 0.2f);
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.perceptionRadius);

        // Cono visual
        Vector3 rightDir = Quaternion.Euler(0, enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;
        Vector3 leftDir = Quaternion.Euler(0, -enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;

        Gizmos.color = baseColor;
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + rightDir * enemyData.visionRadius);
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + leftDir * enemyData.visionRadius);

        // Raycast de obstáculos
        if (Target != null)
        {
            Gizmos.color = rayObstacleDetector.collider != null ? Color.magenta : Color.red;
            Vector3 rayEnd = rayObstacleDetector.collider != null ? rayObstacleDetector.point : Target.position;
            Gizmos.DrawLine(visionPoint.position, rayEnd);
        }

        // Área de parada
        Gizmos.color = new Color(1f, 0.3f, 0.3f, visionEnable ? 1f : 0.2f);
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.stopAreaRadius);
    }
    #endregion
}
