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
    public static event Action OnJumpEvent;
    public static event Action OnCrouchEvent;
    public static event Action<bool> OnSprintEvent;

    //Para GunSystem
    public static event Action OnShootEvent;
    public static event Action OnReloadEvent;
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

        //Saltar
        inputActions.Gameplay.Jump.performed += ctx => OnJumpEvent?.Invoke();

        //Agacharse
        inputActions.Gameplay.Crouch.performed += ctx => OnCrouchEvent?.Invoke();

        //Correr
        inputActions.Gameplay.Sprint.performed += ctx => OnSprintEvent?.Invoke(true);
        inputActions.Gameplay.Sprint.canceled += ctx => OnSprintEvent?.Invoke(false);

        //Disparar
        inputActions.Gameplay.Shoot.performed += ctx => OnShootEvent?.Invoke();

        //Recargar
        inputActions.Gameplay.Reload.performed += ctx => OnReloadEvent?.Invoke();

        inputActions.Enable();
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

        //Saltar
        inputActions.Gameplay.Jump.performed -= ctx => OnJumpEvent?.Invoke();

        //Agacharse
        inputActions.Gameplay.Crouch.performed -= ctx => OnCrouchEvent?.Invoke();

        //Correr
        inputActions.Gameplay.Sprint.performed -= ctx => OnSprintEvent?.Invoke(true);
        inputActions.Gameplay.Sprint.canceled -= ctx => OnSprintEvent?.Invoke(false);

        //Disparar
        inputActions.Gameplay.Shoot.performed -= ctx => OnShootEvent?.Invoke();

        //Recargar
        inputActions.Gameplay.Reload.performed -= ctx => OnReloadEvent?.Invoke();
    }
}
