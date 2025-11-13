using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Referencias
    EnemyStateMachine fsm;
    VisionSystem visionSystem;
    ListenSystem listenSystem;
    EnemyMovement movement;
    Health health;
    InjectionDoorController doorController;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        visionSystem = GetComponent<VisionSystem>();
        listenSystem = GetComponent<ListenSystem>();
        movement = GetComponent<EnemyMovement>();
        health = GetComponent<Health>();
        doorController = FindAnyObjectByType<InjectionDoorController>();
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
        visionSystem.OnTargetSee -= HandleTargetSee;
        visionSystem.OnTargetLose -= HandleTargetLost;

        //Listen System
        listenSystem.OnListenPlayer -= HandleListen;
        listenSystem.OnStopListen -= HandleDontListen;

        //Movement
        movement.OnIdleEnter -= HandleIdleEnter;
        movement.OnIdleExit -= HandleIdleExit;

        //Health
        health.OnHit -= HandleHit;
    }

    bool AlarmActive => doorController != null && doorController.IsAlarmActive;
    private void Update()
    {
        if (AlarmActive)
        {
            fsm.OnChase();
        }

    }

    #region Vision Hanlders
    void HandleTargetSee(Transform target)
    {
        if (AlarmActive)
        {
            fsm.OnChase();
            return;
        }
        fsm.OnChase();
    }
    void HandleTargetLost(Transform target)
    {
        if (AlarmActive) return;
        fsm.OnPatrol();
    }
    #endregion

    #region Listen Handlers
    void HandleListen(Transform playerTransform)
    {
        if (AlarmActive) return;
        fsm.OnChase();
    }
    void HandleDontListen(Transform playerTransform)
    {
        if (AlarmActive) return;
        fsm.OnPatrol();
    }
    #endregion

    #region Movement Handlers
    //Idle: 
    void HandleIdleEnter()
    {
        if (AlarmActive) return;
        fsm.OnIdle();
    }
    void HandleIdleExit()
    {
        if (AlarmActive) return;
        fsm.OnPatrol();
    }
    #endregion

    #region Health Handlers
    void HandleHit(Vector3 hit)
    {
        if(AlarmActive) return;
        fsm.OnChase();
    }
    #endregion
}
