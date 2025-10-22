using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Referencias
    EnemyStateMachine fsm;
    VisionSystem visionSystem;
    EnemyMovement movement;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        visionSystem = GetComponent<VisionSystem>();
        movement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Movement
        movement.OnIdleEnter += HandleIdleEnter;
        movement.OnIdleExit += HandleIdleExit;
    }

    private void OnDisable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Movement
        movement.OnIdleEnter -= HandleIdleEnter;
        movement.OnIdleExit -= HandleIdleExit;
    }

    #region Vsion Hanlders
    void HandleTargetSee(Transform target) => fsm.OnChase();
    void HandleTargetLost(Transform target) => fsm.OnPatrol();
    #endregion

    #region Movement Handlers
    //Idle: 
    void HandleIdleEnter() => fsm.OnIdle();
    void HandleIdleExit() => fsm.OnPatrol();
    #endregion
}
