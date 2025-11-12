using UnityEngine;

/// <summary>
/// Manages a worldspace health bar that follows an enemy character
/// Implements billboard rotation to face between player and camera
/// </summary>
[RequireComponent(typeof(Canvas))]
public class WorldspaceHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private Canvas worldCanvas;

    [Header("Positioning")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float followSmoothSpeed = 10f;

    [Header("Billboard Settings")]
    [Tooltip("0 = Face player only, 1 = Face camera only, 0.5 = Face between both")]
    [SerializeField] private float billboardBlend = 0.7f;
    [SerializeField] private bool lockYRotation = true;

    // Runtime references
    private Transform enemyTransform;
    private Transform playerTransform;
    private Camera mainCamera;
    private bool isInitialized = false;

    private void Awake()
    {
        ValidateReferences();
        SetupCanvas();
    }

    /// <summary>
    /// Initialize the worldspace health bar
    /// </summary>
    public void Initialize(BattleCharacter enemy, Transform enemyTransformRef, Transform playerTransformRef, Camera camera)
    {
        if (enemy == null || enemyTransformRef == null || playerTransformRef == null || camera == null)
        {
            Debug.LogError("[WorldspaceHealthBar] Cannot initialize - null references provided!");
            return;
        }

        enemyTransform = enemyTransformRef;
        playerTransform = playerTransformRef;
        mainCamera = camera;
        isInitialized = true;

        Debug.Log($"[WorldspaceHealthBar] Initializing for {enemy.name}");
        Debug.Log($"[WorldspaceHealthBar] Enemy position: {enemyTransformRef.position}");
        Debug.Log($"[WorldspaceHealthBar] Offset: {offset}");

        // Initialize health bar UI
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(enemy.MaxHealth);
            healthBarUI.SetHealthImmediate(enemy.CurrentHealth, enemy.MaxHealth);
        }

        // Note: Event subscription is handled by BattleCombatUIController
        // to avoid double subscription

        // Position immediately (non-lerped for instant positioning)
        Vector3 initialPosition = enemyTransform.position + offset;
        transform.position = initialPosition;
        Debug.Log($"[WorldspaceHealthBar] Initial position set to: {transform.position}");

        // Update billboard immediately
        UpdateBillboard();

        Debug.Log($"[WorldspaceHealthBar] Initialized successfully. Final position: {transform.position}");
    }

    private void LateUpdate()
    {
        if (!isInitialized)
        {
            // Debug: Check why not initialized
            if (enemyTransform != null)
            {
                Debug.LogWarning($"[WorldspaceHealthBar] LateUpdate called but not initialized! Has enemy transform but isInitialized=false");
            }
            return;
        }

        UpdatePosition();
        UpdateBillboard();
    }

    /// <summary>
    /// Update position to follow enemy
    /// </summary>
    private void UpdatePosition()
    {
        if (enemyTransform == null)
        {
            Debug.LogWarning("[WorldspaceHealthBar] UpdatePosition called but enemyTransform is null!");
            return;
        }

        // Target position is enemy position + offset
        Vector3 targetPosition = enemyTransform.position + offset;

        // Smooth follow
        if (followSmoothSpeed > 0)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                targetPosition, 
                Time.deltaTime * followSmoothSpeed
            );
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    /// <summary>
    /// Update billboard rotation to face camera/player
    /// </summary>
    private void UpdateBillboard()
    {
        if (mainCamera == null || playerTransform == null) return;

        // Calculate blend position between camera and player
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = Vector3.Lerp(playerPos, cameraPos, billboardBlend);

        // Calculate look direction
        Vector3 lookDirection = targetPos - transform.position;

        // Lock Y rotation if needed (keep health bar horizontal)
        if (lockYRotation)
        {
            lookDirection.y = 0f;
        }

        // Apply rotation
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// Update health display
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Show the health bar
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hide the health bar
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Setup canvas for worldspace rendering
    /// </summary>
    private void SetupCanvas()
    {
        if (worldCanvas == null)
        {
            worldCanvas = GetComponent<Canvas>();
        }

        if (worldCanvas != null)
        {
            worldCanvas.renderMode = RenderMode.WorldSpace;
            
            // Set canvas size
            RectTransform rectTransform = worldCanvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(2f, 0.3f); // 2 units wide, 0.3 units tall
            }

            // Ensure CanvasScaler is set up properly
            UnityEngine.UI.CanvasScaler scaler = worldCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null)
            {
                scaler = worldCanvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            }
            
            scaler.dynamicPixelsPerUnit = 100;
        }
    }

    /// <summary>
    /// Validate required references
    /// </summary>
    private void ValidateReferences()
    {
        if (healthBarUI == null)
        {
            healthBarUI = GetComponentInChildren<HealthBarUI>();
            
            if (healthBarUI == null)
            {
                Debug.LogError($"[WorldspaceHealthBar] HealthBarUI component not found on {gameObject.name}!", this);
            }
        }

        if (worldCanvas == null)
        {
            worldCanvas = GetComponent<Canvas>();
            
            if (worldCanvas == null)
            {
                Debug.LogError($"[WorldspaceHealthBar] Canvas component not found on {gameObject.name}!", this);
            }
        }
    }

    /// <summary>
    /// Cleanup when destroyed
    /// </summary>
    private void OnDestroy()
    {
        // Note: We don't unsubscribe from events here because the enemy 
        // BattleCharacter reference is not stored. The UI controller 
        // should handle cleanup before destroying this object.
        isInitialized = false;
    }

    #region Public Getters

    public bool IsInitialized => isInitialized;
    public HealthBarUI HealthBar => healthBarUI;

    #endregion
}
