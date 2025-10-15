using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonJRPGCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.2f, -2.5f);

    [Header("Rotación Orbital")]
    public float mouseXSens = 180f;
    public float mouseYSens = 120f;
    public float controlXSens = 2000f;
    public float controlYSens = 2000f;
    public float minPitch = -10f;
    public float maxPitch = 60f;

    [Header("Sensibilidad del mando")]
    [Tooltip("Multiplicador adicional para ajustar la sensación del joystick")]
    [Range(0.1f, 5f)] public float controllerSensitivityMultiplier = 0.5f;
    [Range(0f, 1f)] public float controllerSmoothing = 0.1f; // suavizado del movimiento del joystick

    [Header("Auto Follow")]
    public float autoFollowDelay = 2f;
    public float autoFollowSpeed = 5f;

    [Header("Camera Settings")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float followHeight = 1f;
    [SerializeField] private float combatDistance = 1.5f;
    [SerializeField] private float combatHeight = 0.8f;

    [Header("Combat Mode")]
    [SerializeField] private Vector3 combatOffset = new Vector3(0.3f, 0.8f, -1.5f);
    public float combatPitch = 15f;
    public float transitionSpeed = 3f;

    [Header("Battle System")]
    public BattleManagerV2 battleManager;
    public Transform enemy;

    [Header("Input System")]
    public InputActionReference lookAction;

    float yaw;
    float pitch;
    float lastInputTime;
    private bool inCombatMode = false;

    // variables internas para suavizado
    private Vector2 smoothLookInput;

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
        if (inCombat != inCombatMode)
        {
            inCombatMode = inCombat;
            if (inCombatMode)
                EnterCombatMode();
        }

        UpdateCursorState();

        if (inCombatMode)
        {
            HandleCombatCamera();
            return;
        }

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude > 0.0001f)
        {
            // Detectar tipo de dispositivo
            InputDevice device = lookAction.action.activeControl?.device;
            bool isController = device is Gamepad;

            float sensX = isController ? controlXSens * controllerSensitivityMultiplier : mouseXSens;
            float sensY = isController ? controlYSens * controllerSensitivityMultiplier : mouseYSens;

            // Suavizado (solo para mando)
            if (isController && controllerSmoothing > 0)
                smoothLookInput = Vector2.Lerp(smoothLookInput, lookInput, 1 - Mathf.Exp(-controllerSmoothing * 50f * Time.deltaTime));
            else
                smoothLookInput = lookInput;

            yaw += smoothLookInput.x * sensX * Time.deltaTime;
            pitch -= smoothLookInput.y * sensY * Time.deltaTime;
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

    private void EnterCombatMode()
    {
        Debug.Log("Camera entering combat mode - behind shoulder position");
        if (enemy != null)
        {
            Vector3 directionToEnemy = (enemy.position - target.position).normalized;
            yaw = Mathf.Atan2(directionToEnemy.x, directionToEnemy.z) * Mathf.Rad2Deg;
        }
        pitch = combatPitch;
    }

    private void HandleCombatCamera()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position + rotation * combatOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, transitionSpeed * Time.deltaTime);
    }

    private bool IsInCombat()
    {
        if (battleManager == null)
            return false;

        BattleState currentState = battleManager.CurrentState;
        bool isActive = battleManager.IsBattleActive;
        bool inCombat = isActive && (currentState == BattleState.PlayerTurn || currentState == BattleState.EnemyTurn);

        if (Time.frameCount % 60 == 0)
            Debug.Log($"Camera Combat Check - State: {currentState}, Active: {isActive}, InCombat: {inCombat}");

        return inCombat;
    }

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
