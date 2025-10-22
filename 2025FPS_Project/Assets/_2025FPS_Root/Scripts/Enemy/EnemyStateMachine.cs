using UnityEngine;

public enum EnemyState
{
    Patrol,
    Idle,
    Alert,
    Chase,
    Attack
}

public class EnemyStateMachine : MonoBehaviour
{

    #region Getter
    public EnemyState currentState { get; private set; } = EnemyState.Patrol;
    #endregion

    #region Eventos
    public event System.Action<EnemyState> OnStateChanged; //Se dispara cuando cambia de estado
    #endregion

    //OBJETIVO: Cambiar de estado si es diferente al actual.
    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return; // No cambiar si ya está en ese estado
        Debug.Log($"Enemy changed from {currentState} to {newState}");
        currentState = newState;
        OnStateChanged?.Invoke(currentState);
    }



    #region Handlers públicos (Para eventos)

    public void OnChase() => ChangeState(EnemyState.Chase);
    public void OnPatrol() => ChangeState(EnemyState.Patrol);
    public void OnAlert() => ChangeState(EnemyState.Alert);
    public void OnIdle() => ChangeState(EnemyState.Idle);
    public void OnAttack() => ChangeState(EnemyState.Attack);

    #endregion
}
