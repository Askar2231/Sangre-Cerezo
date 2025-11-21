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
    [Tooltip("Additional height offset for tall enemies")]
    [SerializeField] private float additionalHeightOffset = 5f;

    [Header("Billboard Settings")]
    [Tooltip("0 = Face player only, 1 = Face camera only, 0.5 = Face between both")]
    [SerializeField] private float billboardBlend = 0.7f;
    [SerializeField] private bool lockYRotation = true;

    // Runtime references
    private Transform enemyTransform;
    private Transform playerTransform;
    private Camera mainCamera;
    private bool isInitialized = false;
    private Vector3 calculatedOffset; // Stores the final calculated offset including enemy height

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
        // Detailed null checking with specific error messages
        if (enemy == null)
        {
            Debug.LogError("[WorldspaceHealthBar] Cannot initialize - enemy is null!");
            return;
        }
        if (enemyTransformRef == null)
        {
            Debug.LogError("[WorldspaceHealthBar] Cannot initialize - enemyTransformRef is null!");
            return;
        }
        if (playerTransformRef == null)
        {
            Debug.LogError("[WorldspaceHealthBar] Cannot initialize - playerTransformRef is null!");
            return;
        }
        if (camera == null)
        {
            Debug.LogError("[WorldspaceHealthBar] Cannot initialize - camera is null!");
            return;
        }

        enemyTransform = enemyTransformRef;
        playerTransform = playerTransformRef;
        mainCamera = camera;
        isInitialized = true;

        Debug.Log($"[WorldspaceHealthBar] Initializing for {enemy.name}");
        Debug.Log($"[WorldspaceHealthBar] Enemy position: {enemyTransformRef.position}");

        // Initialize health bar UI
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(enemy.MaxHealth);
            healthBarUI.SetHealthImmediate(enemy.CurrentHealth, enemy.MaxHealth);
        }
        else
        {
            Debug.LogWarning("[WorldspaceHealthBar] healthBarUI is null! Health display won't work.");
        }

        // Note: Event subscription is handled by BattleCombatUIController
        // to avoid double subscription

        // Calculate enemy height and position health bar above head
        float enemyHeight = CalculateEnemyHeight(enemyTransformRef);
        Debug.Log($"[WorldspaceHealthBar] Calculated enemy height: {enemyHeight}");
        Debug.Log($"[WorldspaceHealthBar] Base offset: {offset}, Additional offset: {additionalHeightOffset}");
        
        calculatedOffset = offset;
        calculatedOffset.y += enemyHeight + additionalHeightOffset;
        
        // Position immediately (non-lerped for instant positioning)
        Vector3 initialPosition = enemyTransform.position + calculatedOffset;
        transform.position = initialPosition;
        Debug.Log($"[WorldspaceHealthBar] Initial position set to: {transform.position} (enemy height: {enemyHeight}, final offset: {calculatedOffset})");

        // Update billboard immediately
        UpdateBillboard();

        Debug.Log($"[WorldspaceHealthBar] Initialized successfully. Final position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
    }
    
    /// <summary>
    /// Calculate enemy height from collider or renderer bounds
    /// </summary>
    private float CalculateEnemyHeight(Transform enemyTransform)
    {
        // Try CharacterController first
        CharacterController charController = enemyTransform.GetComponent<CharacterController>();
        if (charController != null)
        {
            Debug.Log($"[WorldspaceHealthBar] Using CharacterController height: {charController.height}");
            return charController.height * 0.5f; // Half height to get to top
        }
        
        // Try Collider bounds
        Collider collider = enemyTransform.GetComponent<Collider>();
        if (collider != null)
        {
            float height = collider.bounds.size.y;
            Debug.Log($"[WorldspaceHealthBar] Using Collider bounds height: {height}");
            return height * 0.5f;
        }
        
        // Try Renderer bounds as fallback
        Renderer renderer = enemyTransform.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            float height = renderer.bounds.size.y;
            Debug.Log($"[WorldspaceHealthBar] Using Renderer bounds height: {height}");
            return height * 0.5f;
        }
        
        Debug.LogWarning("[WorldspaceHealthBar] Could not determine enemy height, using default 1.0");
        return 1.0f; // Default fallback
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
        Vector3 targetPosition = enemyTransform.position + calculatedOffset;

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

        // Calculate look direction (REVERSED to face towards camera/player instead of away)
        Vector3 lookDirection = transform.position - targetPos;

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
            
            // Set canvas size and scale
            RectTransform rectTransform = worldCanvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(200f, 30f); // Reasonable size for worldspace
                rectTransform.localScale = Vector3.one * 0.01f; // Scale down for worldspace (1% of original size)
                
                Debug.Log($"[WorldspaceHealthBar] Canvas setup - Size: {rectTransform.sizeDelta}, Scale: {rectTransform.localScale}");
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
