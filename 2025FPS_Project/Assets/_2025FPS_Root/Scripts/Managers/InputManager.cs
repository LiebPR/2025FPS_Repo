using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Referencias
    PlayerInput inputActions;
    #endregion

    #region Eventos
    //Para FPSController
    public static event Action<Vector2> OnMoveEvent;
    public static event Action<Vector2> OnLookEvent;
    public static event Action<bool> OnCrouchEvent;
    public static event Action<bool> OnSprintEvent;

    //Para GunSystem
    public static event Action OnShootEvent;

    //Para InteractionSystem
    public static event Action OnInteractHoldStart;
    public static event Action OnInteractHoldEnd;
    #endregion

    private void Awake()
    {
        inputActions = new PlayerInput();
    }

    private void OnEnable()
    {
        //SUSCRIPCIONES 

        //Movimiento
        inputActions.Gameplay.Move.performed += ctx => OnMoveEvent?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Gameplay.Move.canceled += ctx => OnMoveEvent?.Invoke(Vector2.zero);

        //Mirar
        inputActions.Gameplay.Look.performed += ctx => OnLookEvent?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Gameplay.Look.canceled += ctx => OnLookEvent?.Invoke(Vector2.zero);

        //Agacharse
        inputActions.Gameplay.Crouch.performed += ctx => OnCrouchEvent?.Invoke(true);
        inputActions.Gameplay.Crouch.canceled += ctx => OnCrouchEvent?.Invoke(false);

        //Correr
        inputActions.Gameplay.Sprint.performed += ctx => OnSprintEvent?.Invoke(true);
        inputActions.Gameplay.Sprint.canceled += ctx => OnSprintEvent?.Invoke(false);

        //Disparar
        inputActions.Gameplay.Shoot.performed += ctx => OnShootEvent?.Invoke();

        //Interacción
        inputActions.Gameplay.Interact.performed += ctx => OnInteractHoldStart?.Invoke();
        inputActions.Gameplay.Interact.canceled += ctx => OnInteractHoldEnd?.Invoke();

        inputActions.Enable();

        //Suscribirse al cambio de estado del GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void Shoot_performed(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void OnDisable()
    {
        //LIMPIEZA DE SUSCRIPCIONES
        inputActions.Disable();

        //Movimiento
        inputActions.Gameplay.Move.performed -= ctx => OnMoveEvent?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Gameplay.Move.canceled -= ctx => OnMoveEvent?.Invoke(Vector2.zero);

        //Mirar
        inputActions.Gameplay.Look.performed -= ctx => OnLookEvent?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Gameplay.Look.canceled -= ctx => OnLookEvent?.Invoke(Vector2.zero);

        //Agacharse
        inputActions.Gameplay.Crouch.performed -= ctx => OnCrouchEvent?.Invoke(true);
        inputActions.Gameplay.Crouch.canceled -= ctx => OnCrouchEvent?.Invoke(false);

        //Correr
        inputActions.Gameplay.Sprint.performed -= ctx => OnSprintEvent?.Invoke(true);
        inputActions.Gameplay.Sprint.canceled -= ctx => OnSprintEvent?.Invoke(false);

        //Disparar
        inputActions.Gameplay.Shoot.performed -= ctx => OnShootEvent?.Invoke();

        //Interacción
        inputActions.Gameplay.Interact.performed -= ctx => OnInteractHoldStart?.Invoke();
        inputActions.Gameplay.Interact.canceled -= ctx => OnInteractHoldEnd?.Invoke();

        // Desuscribirse
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
    }

    #region Control de Cursor
    void HandleGameStateChanged(GameManager.GameState newState)
    {
        bool isPlaying = newState == GameManager.GameState.Playing;

        Cursor.lockState = isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isPlaying;

        // Deshabilita los controles fuera del modo "Playing"
        if (isPlaying)
            inputActions.Gameplay.Enable();
        else
            inputActions.Gameplay.Disable();
    }
    #endregion
}
