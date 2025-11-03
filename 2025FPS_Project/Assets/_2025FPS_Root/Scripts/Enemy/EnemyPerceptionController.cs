using UnityEngine;

public class EnemyPerceptionController : MonoBehaviour
{
    ListenSystem listen;
    VisionSystem vision;
    EnemyStateMachine fsm;

    private void Awake()
    {
        listen = GetComponent<ListenSystem>();
        vision = GetComponent<VisionSystem>();
        fsm = GetComponent<EnemyStateMachine>();

        //Suscripción al evento de cambio de estado
        fsm.OnStateChanged += HandleStateChange;
    }

    private void OnDestroy()
    {
        fsm.OnStateChanged -= HandleStateChange;
    }

    void HandleStateChange(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrol:
                listen?.SetListenActive(true); //Escucha activa, visión relajada
                if (vision != null) vision.EnableConeVision(false);
                break;
            case EnemyState.Alert:
            case EnemyState.Chase:
                listen?.SetListenActive(false); //Escucha desactivada, visión activada
                if(vision != null) vision.EnableConeVision(true);
                break;
        }
    }
}
