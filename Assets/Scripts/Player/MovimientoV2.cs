using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Character movement controller with 1D animation support (forward-only movement)
/// Character rotates towards input direction and only moves forward
/// Similar to Dark Souls, Monster Hunter, etc.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MovimientoV2 : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private Transform cameraTransform;

    [Header("Acceleration")]
    [SerializeField] private float accelerationTime = 0.2f; // Time to reach max speed
    [SerializeField] private float decelerationTime = 0.15f; // Time to stop

    [Header("Animation Speed Limits")]
    [SerializeField] private float walkAnimationSpeedMax = 1f; // Animation speed value for walking
    [SerializeField] private float runAnimationSpeedMax = 2f; // Animation speed value for running

    [Header("Input (Unity Input System)")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference runAction;

    [Header("Audio Settings")]
    [SerializeField] public AudioSource pasos;
    [SerializeField] private float walkspeedmusic = 1f;
    [SerializeField] private float runspeedmusic = 1.4f;

    
    

    // Components
    private Animator animator;
    private CharacterController cc;

    // Current state
    private Vector2 currentInput;
    private float currentAnimationSpeed;

    private float animationSpeedVelocity;
    private bool isRunning;
    private bool canMove = true; // Movement can be disabled (e.g., during battle)

    // Animation parameter hash (for performance)
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Enable input actions
        if (moveAction != null)
            moveAction.action.Enable();
        if (runAction != null)
            runAction.action.Enable();
        
    }

    private void OnDisable()
    {
        // Disable input actions
        if (moveAction != null)
            moveAction.action.Disable();
        if (runAction != null)
            runAction.action.Disable();
       
    }

    private void Update()
    {
        HandleInput();
        UpdateAnimation();
        HandleMovement();
        HandleRotation();
        HandleAudio();
        
        


        
    }

    /// <summary>
    /// Read input from Unity Input System or legacy input
    /// </summary>
    private void HandleInput()
{
    // If movement is disabled, clear input and return
    if (!canMove)
    {
        currentInput = Vector2.zero;
        isRunning = false;
        return;
    }

    // Read movement input
    if (moveAction != null && moveAction.action != null)
    {
        currentInput = moveAction.action.ReadValue<Vector2>();  
    }
    else
    {
        // Fallback to legacy input
        currentInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
    }

    // Clamp input to circular deadzone
    currentInput = Vector2.ClampMagnitude(currentInput, 1f);

    // Read run input - CORREGIDO
    if (runAction != null && runAction.action != null)
    {
        // Leer como float para Button type
        isRunning = runAction.action.ReadValue<float>() > 0.5f;
    }
    else
    {
        // Fallback to legacy input
        isRunning = Input.GetKey(KeyCode.LeftShift);
    }
}

    /// <summary>
    /// Update animation speed with acceleration/deceleration
    /// </summary>
    private void UpdateAnimation()
    {
        // Determine target speed based on input and run state
        float targetSpeed = 0f;

        if (currentInput.magnitude > 0.1f)
        {
            // Moving - set speed based on run state
            targetSpeed = isRunning ? runAnimationSpeedMax : walkAnimationSpeedMax;
        }
        // else targetSpeed stays 0 (idle)

        // Calculate smoothing time based on acceleration/deceleration
        float smoothTime = currentInput.magnitude > 0.1f
            ? accelerationTime
            : decelerationTime;

        // Smooth towards target using SmoothDamp
        currentAnimationSpeed = Mathf.SmoothDamp(
            currentAnimationSpeed,
            targetSpeed,
            ref animationSpeedVelocity,
            smoothTime
        );

        // Set animation speed parameter for 1D blend tree
        animator.SetFloat(SpeedHash, currentAnimationSpeed);
    }

    /// <summary>
    /// Handle actual character movement (forward-only along character's facing)
    /// </summary>
    private void HandleMovement()
    {
        // Only move if there's input
        if (currentInput.magnitude < 0.01f)
        {
            return;
        }

        // Move forward along character's facing direction
        Vector3 moveDir = transform.forward;

        // Apply speed based on run state
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Apply input magnitude for analog control
        moveDir *= currentInput.magnitude;

        // Move character
        cc.SimpleMove(moveDir * currentSpeed);
    }

    /// <summary>
    /// Handle character rotation towards input direction (camera-relative)
    /// </summary>
    private void HandleRotation()
    {
        // Only rotate if there's significant input
        if (currentInput.sqrMagnitude < 0.01f) return;

        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera transform not assigned!");
            return;
        }

        // Calculate desired direction relative to camera
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;
        Vector3 targetDirection = (camForward * currentInput.y + camRight * currentInput.x).normalized;

        if (targetDirection.sqrMagnitude > 0.001f)
        {
            // Rotate towards input direction
            Quaternion targetRot = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // --- AÑADE ESTAS DOS FUNCIONES NUEVAS A TU SCRIPT ---


    /// <summary>
    /// Handles camera rotation based on look input (mouse or right stick)
    /// </summary>
    


    #region Public Getters

    /// <summary>
    /// Check if character is currently running
    /// </summary>
    public bool IsRunning => isRunning;

    /// <summary>
    /// Get current movement input magnitude (0-1)
    /// </summary>
    public float CurrentMovementMagnitude => currentInput.magnitude;

    /// <summary>
    /// Get current animation speed value
    /// </summary>
    public float CurrentAnimationSpeed => currentAnimationSpeed;

    /// <summary>
    /// Check if character is currently moving
    /// </summary>
    public bool IsMoving => currentInput.magnitude > 0.01f;

    /// <summary>
    /// Check if movement is currently enabled
    /// </summary>
    public bool CanMove => canMove;

    #endregion

    #region Movement Control

    /// <summary>
    /// Enable or disable movement (e.g., during battle, cutscenes, etc.)
    /// </summary>
    /// <param name="enabled">True to enable movement, false to disable</param>
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;

        if (!canMove)
        {
            // Clear input when disabling movement
            currentInput = Vector2.zero;
            isRunning = false;

            // Animation will smoothly decelerate to idle via UpdateAnimation()
            Debug.Log("Movement disabled");
        }
        else
        {
            Debug.Log("Movement enabled");
        }
    }

    /// <summary>
    /// Immediately stop movement and reset to idle (no smooth deceleration)
    /// </summary>
    public void ForceStopMovement()
    {
        currentInput = Vector2.zero;
        currentAnimationSpeed = 0f;
        animationSpeedVelocity = 0f;
        isRunning = false;

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
        }

        #endregion
    }

    //Sonido de pasos cuando el personaje se mueve

    private void HandleAudio()
    {
        if (pasos == null)
            return;

        bool estaMoviendose = canMove && currentInput.magnitude > 0.1f;

        if (estaMoviendose)
        {
            // Ajusta el pitch (tono) según si está corriendo o caminando
            pasos.pitch = isRunning ? runspeedmusic : walkspeedmusic;

            // Si el sonido no está sonando, lo reproduce
            if (!pasos.isPlaying)
            {
                pasos.Play();
            }
        }
        else
        {
            // Detiene el sonido al quedarse quieto
            if (pasos.isPlaying)
            {
                pasos.Stop();
            }
        }
    }
    

}