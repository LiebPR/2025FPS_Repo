using NUnit.Framework.Internal.Commands;
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

    [Header("Jumping")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;
    bool isGrounded;

    [Header("Player State Bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrouching;

    [Header("FOV Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] float normalFOV = 60f;
    [SerializeField] float sprintFOV = 75f;
    [SerializeField] float fovChangeSpeed = 8f;

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
        InputManager.OnJumpEvent += HandleJump;
        InputManager.OnCrouchEvent += HandleCrouch;
        InputManager.OnSprintEvent += HandleSprint;
    }
    private void OnDisable()
    {
        InputManager.OnMoveEvent -= HandleMove;
        InputManager.OnLookEvent -= HandleLook;
        InputManager.OnJumpEvent -= HandleJump;
        InputManager.OnCrouchEvent -= HandleCrouch;
        InputManager.OnSprintEvent -= HandleSprint;
    }
    #endregion

    void Start()
    {
        //Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    void Update()
    {
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.transform.position, groundCheckRadius, groundLayer);
        //Debug ray: visible only in Scene
        Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);

        UpdateFOV();

    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
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

    void Jump()
    {
        if (isGrounded) playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

    public void HandleJump()
    {
        Jump();
    }

    public void HandleCrouch()
    {
        isCrouching = !isCrouching;
        anim.SetBool("isCrouching", isCrouching);
    }

    public void HandleSprint(bool isPressed)
    {
        if (isCrouching && isPressed) return;
        isSprinting = isPressed;
    }
    #endregion
}
