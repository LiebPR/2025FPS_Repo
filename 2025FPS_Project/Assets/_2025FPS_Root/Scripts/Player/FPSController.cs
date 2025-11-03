using NUnit.Framework.Internal.Commands;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Look")]
    [SerializeField] GameObject camHolder; //Ref en inspector al objeto a rotar
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float maxForce = 1f; //Fuerza máxima de aceleración
    [SerializeField] float sensitivity = 0.1f;
    bool isSprinting;
    bool isCrouching;

    [Header("Jumping")]
    [SerializeField] GameObject groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;

    [Header("FOV Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float sprintFOV = 75f;
    [SerializeField] float fovChangeSpeed = 8f;

    [Header("Head Bob Settings")]
    [SerializeField] float walkBobSpeed = 10f;
    [SerializeField] float walkBobAmount = 0.05f;
    [SerializeField] float sprintBobSpeed = 14f;
    [SerializeField] float sprinBobAmount = 0.1f;
    [SerializeField] float crouchBobSpeed = 6f;
    [SerializeField] float crouchBobAmount = 0.025f;
    Vector3 camOriginalPos;
    float bobTimer = 0f;

    [Header("Slide Settings")]
    [SerializeField] float slideForce = 10f;
    [SerializeField] float slideDuration = 1f;
    [SerializeField] float slideFriction = 5f;
    bool isSliding;
    Coroutine slideCoroutine;

    //Input Variables
    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;
    #endregion

    #region References
    Rigidbody playerRb;
    Animator anim;
    #endregion

    #region Getters
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;
    public bool IsGrounded => isGrounded;
    #endregion

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    #region Input Events Suscription
    private void OnEnable()
    {
        InputManager.OnMoveEvent += HandleMove;
        InputManager.OnLookEvent += HandleLook;
        InputManager.OnCrouchEvent += HandleCrouch;
        InputManager.OnSprintEvent += HandleSprint;
    }
    private void OnDisable()
    {
        InputManager.OnMoveEvent -= HandleMove;
        InputManager.OnLookEvent -= HandleLook;
        InputManager.OnCrouchEvent -= HandleCrouch;
        InputManager.OnSprintEvent -= HandleSprint;
    }
    #endregion

    void Start()
    {
        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Head bob: 
        camOriginalPos = camHolder.transform.localPosition;
    }

    
    void Update()
    {
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
        //Debug ray: visible only in Scene
        Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);

        UpdateFOV();

        //Actualizar sprint dinamicamente
        if (isSprinting && !HasMovementInput())
            isSprinting = false;

    }

    private void FixedUpdate()
    {
        if(isSliding) return; //bloquear movimiento normal durante el slide
        Movement();
    }

    private void LateUpdate()
    {
        HandleHeadBob();
        CameraLook();
    }

    void Movement()
    {
        Vector3 currentVelocity = playerRb.linearVelocity;
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);
        targetVelocity *= isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : speed);

        //Convertir la dirección local en global
        targetVelocity = transform.TransformDirection(targetVelocity);

        // Calcular el cambio de velocidad (aceleración)
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        //Aplicar la fuerza de movimiento
        playerRb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    #region Camera
    void CameraLook()
    {
        //Horizontal rotation (player body)
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
        //Vertical rotation (camera)
        lookRotation += (-lookInput.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.localEulerAngles = new Vector3(lookRotation, 0f, 0f);
    }

    void UpdateFOV()
    {
        if (playerCamera == null) return;
        
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovChangeSpeed);
    }

    void HandleHeadBob()
    {
        if(!HasMovementInput() || !isGrounded)
        {
            //Volver suavemente a la posición original si no hay movimiento
            camHolder.transform.localPosition = Vector3.Lerp(camHolder.transform.localPosition, camOriginalPos, Time.deltaTime * 5f);

            //Reiniciar temporizador para evitar que siga la animación
            bobTimer = 0f;
            return;
        }

        float speed = isCrouching ? crouchBobSpeed : (isSprinting ? sprintBobSpeed : walkBobSpeed);
        float amount = isCrouching ? crouchBobAmount : (isSprinting ? sprinBobAmount : walkBobAmount);

        bobTimer += Time.deltaTime * speed;

        //Movimiento sinusoidal
        float xBob = Mathf.Sin(bobTimer) * amount;
        float yBob = Mathf.Cos(bobTimer * 2f) * amount;

        camHolder.transform.localPosition = camOriginalPos + new Vector3(xBob, yBob, 0);
    }
    #endregion

    #region Slide System
    void StartSlide()
    {
        if(slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }
        slideCoroutine = StartCoroutine(SlideRoutine());
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;

        Vector3 slideDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        if (slideDir.magnitude < 0.1f)
            slideDir = transform.forward; //fallback a la dirección de la cámara

        slideDir = transform.TransformDirection(slideDir);

        float currentForce = slideForce;
        float startTime = Time.time;

        while(Time.time < startTime + slideDuration)
        {
            if(!isGrounded) break;
            playerRb.AddForce(slideDir * currentForce, ForceMode.Acceleration);
            currentForce = Mathf.Lerp(currentForce, 0, Time.deltaTime * slideFriction);
            yield return null;
        }

        isSliding = false;
    }
    #endregion

    #region Auxiliar
    public bool HasMovementInput()
    {
        return moveInput.sqrMagnitude > 0.01f;
    }
    #endregion

    #region Input Handlers
    public void HandleMove(Vector2 input)
    {
        moveInput = input;
    }

    public void HandleLook(Vector2 input)
    {
        lookInput = input;
    }

    public void HandleCrouch(bool isPressed)
    {
        isCrouching = isPressed;
        anim.SetBool("isCrouching", isCrouching);

        //Si el jugador está sprintando, en el suelo y presiona crouch desliza.
        if(isPressed && isSprinting && isGrounded && !isSliding)
        {
            StartSlide();
        }
    }

    public void HandleSprint(bool isPressed)
    {
        // Sprint solo si el botón está presionado y hay movimiento
        isSprinting = isPressed && HasMovementInput();

        // Evitar sprint si estás agachado
        if (isCrouching) isSprinting = false;
    }
    #endregion
}
