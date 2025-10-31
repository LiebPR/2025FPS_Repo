using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Referencias
    EnemyStateMachine fsm;
    VisionSystem visionSystem;
    ListenSystem listenSystem;
    EnemyMovement movement;
    Health health;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        visionSystem = GetComponent<VisionSystem>();
        listenSystem = GetComponent<ListenSystem>();
        movement = GetComponent<EnemyMovement>();
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Listen System
        listenSystem.OnListenPlayer += HandleListen;
        listenSystem.OnStopListen += HandleDontListen;

        //Movement
        movement.OnIdleEnter += HandleIdleEnter;
        movement.OnIdleExit += HandleIdleExit;

        //Health
        health.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Listen System
        listenSystem.OnListenPlayer -= HandleListen;
        listenSystem.OnStopListen -= HandleDontListen;

        //Movement
        movement.OnIdleEnter -= HandleIdleEnter;
        movement.OnIdleExit -= HandleIdleExit;

        //Health
        health.OnHit -= HandleHit;
    }

    #region Vision Hanlders
    void HandleTargetSee(Transform target) => fsm.OnChase();
    void HandleTargetLost(Transform target) => fsm.OnPatrol();
    #endregion

    #region Listen Handlers
    void HandleListen(Transform playerTransform)
    {
        fsm.OnChase();
    }
    void HandleDontListen(Transform playerTransform)
    {
        fsm.OnPatrol();
    }
    #endregion

    #region Movement Handlers
    //Idle: 
    void HandleIdleEnter() => fsm.OnIdle();
    void HandleIdleExit() => fsm.OnPatrol();
    #endregion

    #region Health Handlers
    void HandleHit(Vector3 hit)
    {
        fsm.OnChase();
    }
    #endregion
}
