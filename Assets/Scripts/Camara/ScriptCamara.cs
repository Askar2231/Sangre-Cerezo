using UnityEngine;

public class ThirdPersonJRPGCamera : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // Player
    public Vector3 offset = new Vector3(0, 2f, -4f);

    [Header("Rotación Orbital")]
    public float mouseXSens = 180f;
    public float mouseYSens = 120f;
    public float minPitch = -10f;
    public float maxPitch = 60f;

    [Header("Auto Follow")]
    public float autoFollowDelay = 2f;   
    public float autoFollowSpeed = 5f;   

    [Header("Modo Combate")]
    public Vector3 combatOffset = new Vector3(1.5f, 2.5f, -3f); // Detrás del hombro derecho
    public float combatPitch = 10f; // Ángulo hacia abajo para ver al enemigo
    public float transitionSpeed = 3f;
    
    [Header("Battle System")]
    public BattleManagerV2 battleManager;
    public Transform enemy; // Referencia al enemigo para enfocar

    float yaw;
    float pitch;
    float lastInputTime;
    private bool inCombatMode = false;

    void Start()
    {
        if (!target) return;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        
        UpdateCursorState();
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
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            yaw += mouseX * mouseXSens * Time.deltaTime;
            pitch -= mouseY * mouseYSens * Time.deltaTime;
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
    }

    /// <summary>
    /// Maneja la cámara durante el combate - posición fija detrás del hombro
    /// </summary>
    private void HandleCombatCamera()
    {
        // Posición fija detrás del hombro derecho
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position + rotation * combatOffset;
        
        // Suave transición a la posición de combate
        transform.position = Vector3.Lerp(transform.position, targetPosition, transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, transitionSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Verifica si estamos en combate activo (no solo inicializado)
    /// </summary>
    private bool IsInCombat()
    {
        if (battleManager == null) return false;
        
        // Solo activar modo combate durante estados activos de batalla
        BattleState currentState = battleManager.CurrentState;
        
        return currentState == BattleState.PlayerTurn || 
               currentState == BattleState.EnemyTurn;
        // NO incluir BattleState.BattleStart ni BattleState.BattleEnd
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


