using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Referencias
    EnemyStateMachine fsm;
    VisionSystem visionSystem;
    ListenSystem listenSystem;
    EnemyMovement movement;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyStateMachine>();
        visionSystem = GetComponent<VisionSystem>();
        listenSystem = GetComponent<ListenSystem>();
        movement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Listen System
        listenSystem.OnListenPlayer += HandleListen;
        listenSystem.OnDontListenAnything += HandleDontListen;
        listenSystem.OnBulletImpact += HandleBulletImpact;

        //Movement
        movement.OnIdleEnter += HandleIdleEnter;
        movement.OnIdleExit += HandleIdleExit;
    }

    private void OnDisable()
    {
        //Vision System
        visionSystem.OnTargetSee += HandleTargetSee;
        visionSystem.OnTargetLose += HandleTargetLost;

        //Listen System
        listenSystem.OnListenPlayer -= HandleListen;
        listenSystem.OnDontListenAnything -= HandleDontListen;
        listenSystem.OnBulletImpact -= HandleBulletImpact;

        //Movement
        movement.OnIdleEnter -= HandleIdleEnter;
        movement.OnIdleExit -= HandleIdleExit;
    }

    #region Vision Hanlders
    void HandleTargetSee(Transform target) => fsm.OnChase();
    void HandleTargetLost(Transform target) => fsm.OnPatrol();
    #endregion

    #region Listen Handlers
    void HandleListen(Vector3 position)
    {
        fsm.OnChase();
    }
    void HandleDontListen()
    {
        fsm.OnPatrol();
    }
    void HandleBulletImpact(Vector3 position)
    {
        fsm.OnPatrol();
    }
    #endregion

    #region Movement Handlers
    //Idle: 
    void HandleIdleEnter() => fsm.OnIdle();
    void HandleIdleExit() => fsm.OnPatrol();
    #endregion
}
