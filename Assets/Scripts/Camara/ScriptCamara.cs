using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonJRPGCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // Player
    public Vector3 offset = new Vector3(0, 1.2f, -2.5f); // REDUCIR de (0, 2f, -4f)

    [Header("Rotación Orbital - Mouse")]
    public float mouseXSens = 180f;
    public float mouseYSens = 120f;
    
    [Header("Rotación Orbital - Joystick")]
    public float joystickXSens = 100f;
    public float joystickYSens = 80f;
    
    [Header("Pitch Limits")]
    public float minPitch = -10f;
    public float maxPitch = 60f;

    [Header("Auto Follow")]
    public float autoFollowDelay = 2f;   
    public float autoFollowSpeed = 5f;   

    [Header("Camera Settings")]
    [SerializeField] private float followDistance = 2f;    // REDUCIR de 3f
    [SerializeField] private float followHeight = 1f;      // REDUCIR de 1.5f  
    [SerializeField] private float combatDistance = 1.5f;  // REDUCIR de 2f
    [SerializeField] private float combatHeight = 0.8f;    // REDUCIR de 1f

    [Header("Combat Mode")]
    [SerializeField] private Vector3 combatOffset = new Vector3(0.3f, 0.8f, -1.5f); // AJUSTAR para jugador pequeño
    public float combatPitch = 15f; // Aumentar ligeramente de 10f para ver mejor
    public float transitionSpeed = 3f; // Mantener
    
    [Header("Battle System")]
    public BattleManagerV2 battleManager;
    public Transform enemy; // Referencia al enemigo para enfocar

    [Header("Input System")] // <-- AÑADE ESTA LÍNEA
    public InputActionReference lookAction;

    float yaw;
    float pitch;
    float lastInputTime;
    private bool inCombatMode = false;
    private Transform battleCameraPosition; // Position from BattlePositionSetup (if provided)

    void Start()
    {
        if (!target) return;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        
        UpdateCursorState();
    }

    void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
    }

    void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();
    }

    void LateUpdate()
    {
        if (!target) return;

        bool inCombat = IsInCombat();
        
        // Activar modo combate si cambia el estado
        if (inCombat != inCombatMode)
        {
            inCombatMode = inCombat;
            if (inCombatMode)
            {
                EnterCombatMode();
            }
        }
        
        UpdateCursorState();

        if (inCombatMode)
        {
            HandleCombatCamera();
            return;
        }

        // Entrada mouse (solo fuera de combate)
        //float mouseX = Input.GetAxis("Mouse X");
        //float mouseY = Input.GetAxis("Mouse Y");

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // Detectar si es mouse o joystick basado en la magnitud y el dispositivo
        bool isMouseInput = false;
        float sensitivityX = joystickXSens;
        float sensitivityY = joystickYSens;
        
        // Detectar tipo de input
        if (lookAction.action.activeControl != null)
        {
            string controlPath = lookAction.action.activeControl.path.ToLower();
            isMouseInput = controlPath.Contains("mouse");
        }
        
        // Usar sensibilidad apropiada
        if (isMouseInput)
        {
            sensitivityX = mouseXSens;
            sensitivityY = mouseYSens;
        }

        // Asignamos los componentes X e Y a las variables que el script ya usa
        float mouseX = lookInput.x;
        float mouseY = lookInput.y;

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            yaw += mouseX * sensitivityX * Time.deltaTime;
            pitch -= mouseY * sensitivityY * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            lastInputTime = Time.time;
        }
        else if (Time.time - lastInputTime > autoFollowDelay)
        {
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, autoFollowSpeed * Time.deltaTime);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target.position + rotation * offset;
        transform.rotation = rotation;
    }

    /// <summary>
    /// Configura la cámara para modo combate detrás del hombro
    /// </summary>
    private void EnterCombatMode()
    {
        Debug.Log("Camera entering combat mode - behind shoulder position");
        
        // Si hay enemigo, enfocar hacia él
        if (enemy != null)
        {
            Vector3 directionToEnemy = (enemy.position - target.position).normalized;
            yaw = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
        }
        
        // Ángulo fijo para ver al enemigo
        pitch = combatPitch;
        
        // Clear any previous battle camera position reference
        battleCameraPosition = null;
    }
    
    /// <summary>
    /// Set battle camera position from BattlePositionSetup
    /// Called by BattleManager when transitioning to combat positions
    /// </summary>
    public void SetBattleCameraPosition(Transform cameraPosition)
    {
        battleCameraPosition = cameraPosition;
        
        if (cameraPosition != null)
        {
            Debug.Log($"Camera using BattlePositionSetup at {cameraPosition.position}");
            
            // Extract yaw and pitch from the provided position's rotation
            Vector3 eulerAngles = cameraPosition.rotation.eulerAngles;
            yaw = eulerAngles.y;
            pitch = eulerAngles.x;
            
            // Normalize pitch to -180 to 180 range
            if (pitch > 180f) pitch -= 360f;
        }
    }
    
    /// <summary>
    /// Clear battle camera position (called when exiting combat)
    /// </summary>
    public void ClearBattleCameraPosition()
    {
        battleCameraPosition = null;
        Debug.Log("Camera cleared BattlePositionSetup, returning to normal combat mode");
    }

    /// <summary>
    /// Maneja la cámara durante el combate - posición fija detrás del hombro
    /// </summary>
    private void HandleCombatCamera()
    {
        // If BattlePositionSetup provided a camera position, use it
        if (battleCameraPosition != null)
        {
            // Use the exact position and rotation from BattlePositionSetup
            Vector3 setupPosition = battleCameraPosition.position;
            Quaternion setupRotation = battleCameraPosition.rotation;
            
            // Smooth transition to the setup position
            transform.position = Vector3.Lerp(transform.position, setupPosition, transitionSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, setupRotation, transitionSpeed * Time.deltaTime);
            
            return; // Don't use the offset-based positioning
        }
        
        // Fallback: Posición fija detrás del hombro derecho (legacy behavior)
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position + rotation * combatOffset;
        
        // Suave transición a la posición de combate
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, transitionSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Verifica si estamos en combate activo
    /// </summary>
    private bool IsInCombat()
    {
        if (battleManager == null) 
        {
            return false;
        }
        
        // AÑADIR debugging temporal
        BattleState currentState = battleManager.CurrentState;
        bool isActive = battleManager.IsBattleActive;
        
        bool inCombat = isActive && (currentState == BattleState.PlayerTurn || currentState == BattleState.EnemyTurn);
        
        // DEBUG TEMPORAL - quitar después
        if (Time.frameCount % 60 == 0) // cada 60 frames
        {
//            Debug.Log($"Camera Combat Check - State: {currentState}, Active: {isActive}, InCombat: {inCombat}");
        }
        
        return inCombat;
    }

    /// <summary>
    /// Actualiza el estado del cursor según el modo de combate
    /// </summary>
    private void UpdateCursorState()
    {
        bool inCombat = IsInCombat();
        
        if (inCombat)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Debug.Log("Combat mode ON - Cursor unlocked");
            }
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Debug.Log("Combat mode OFF - Cursor locked");
            }
        }
    }
}


