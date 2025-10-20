using UnityEngine;

/// <summary>
/// Handles battle initiation when player contacts enemy
/// </summary>
public class BattleTrigger : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] private BattleManagerV2 battleManager; // Optional: Auto-finds in scene if not assigned
    [SerializeField] private bool autoStartBattle = true; // Start battle on trigger enter
    [SerializeField] private bool destroyOnBattleEnd = true; // Destroy enemy on victory
    
    [Header("Combat Positioning (NEW)")]
    [Tooltip("Optional: Use a BattlePositionSetup to define exact combat positions")]
    [SerializeField] private BattlePositionSetup combatPositionSetup;
    [SerializeField] private bool useLegacyPositioning = false; // Use old 30-unit positioning method
    
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Optional: Auto-finds player
    [SerializeField] private Transform enemyTransform; // Optional: Uses this GameObject if not assigned
    
    [Header("UI")]
    [SerializeField] private GameObject battleUICanvas; // Optional: Battle UI canvas
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private bool battleTriggered = false;
    private bool isSubscribedToBattle = false;
    private EnemyBattleController thisEnemyController; // Controller de ESTE enemigo
    
    private void Awake()
    {
        if (debugMode) Debug.Log($"[BattleTrigger] Awake on: {gameObject.name}");
        
        // Auto-find player if not assigned
        if (playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerBattleController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                if (debugMode) Debug.Log($"[BattleTrigger] Auto-found player: {playerTransform.name}");
            }
        }
        
        // Find EnemyBattleController on this GameObject or parent
        thisEnemyController = GetComponent<EnemyBattleController>();
        if (thisEnemyController == null)
        {
            thisEnemyController = GetComponentInParent<EnemyBattleController>();
        }
        
        // Use this transform as enemy transform if not assigned
        if (enemyTransform == null)
            enemyTransform = transform;
        
        // Auto-find BattleManager if not assigned
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManagerV2>();
            if (battleManager != null && debugMode)
            {
                Debug.Log($"[BattleTrigger] Auto-found battle manager: {battleManager.name}");
            }
        }
        
        // Ensure battle UI is hidden initially
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
        }
        
        // Validate critical references
        if (thisEnemyController == null)
        {
            Debug.LogError($"[BattleTrigger] No EnemyBattleController found on {gameObject.name} or its parent! This trigger won't work.");
        }
        else if (debugMode)
        {
            Debug.Log($"[BattleTrigger] Initialized successfully on {gameObject.name}\n" +
                     $"  Enemy Controller: {thisEnemyController.name}\n" +
                     $"  BattleManager: {battleManager?.name ?? "NULL"}\n" +
                     $"  Auto-start: {autoStartBattle}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if battle already triggered
        if (battleTriggered)
        {
            if (debugMode) Debug.Log($"[BattleTrigger] Battle already triggered for {gameObject.name}");
            return;
        }
        
        // Check if player entered
        if (other.CompareTag("Player") || other.GetComponent<PlayerBattleController>() != null)
        {
            if (debugMode) Debug.Log($"<color=cyan>[BattleTrigger] Player contacted {gameObject.name}! Starting battle...</color>");
            
            if (autoStartBattle)
            {
                StartBattleSequence();
            }
        }
    }
    
    /// <summary>
    /// Inicia la secuencia de batalla con posicionamiento
    /// </summary>
    private void StartBattleSequence()
    {
        battleTriggered = true;
        
        // Activar UI de combate inmediatamente
        ActivateBattleUI();
        
        // Posicionar jugador y enemigo para el combate (solo si se usa posicionamiento legacy)
        if (useLegacyPositioning && combatPositionSetup == null)
        {
            PositionCharactersForBattle();
            Invoke(nameof(InitiateBattle), 0.5f);
        }
        else
        {
            // Iniciar batalla inmediatamente - el BattleManager manejará el posicionamiento
            InitiateBattle();
        }
    }
    
    /// <summary>
    /// Manually start battle (can be called by quest system)
    /// </summary>
    public void StartBattle()
    {
        if (battleTriggered)
        {
            Debug.LogWarning($"[BattleTrigger] Battle already triggered for {gameObject.name}");
            return;
        }
        
        if (debugMode) Debug.Log($"<color=green>[BattleTrigger] Manually starting battle with {gameObject.name}</color>");
        StartBattleSequence();
    }
    
    /// <summary>
    /// Posiciona al jugador y enemigo para el combate
    /// </summary>
    private void PositionCharactersForBattle()
    {
        if (playerTransform == null || enemyTransform == null)
        {
            Debug.LogWarning("Player or Enemy transform not assigned!");
            return;
        }
        
        // Obtener la dirección hacia donde está mirando el enemigo
        Vector3 enemyForward = enemyTransform.forward;
        
        // Solo usar componentes horizontales (X, Z) - ignorar Y
        enemyForward.y = 0f;
        enemyForward = enemyForward.normalized;
        
        // Posicionar al jugador 30 unidades EN LA DIRECCIÓN que mira el enemigo
        Vector3 playerBattlePosition = enemyTransform.position + (enemyForward * 30f);
        
        // Mantener la Y original del jugador
        playerBattlePosition.y = playerTransform.position.y;
        
        // Aplicar nueva posición
        playerTransform.position = playerBattlePosition;
        
        // Hacer que ambos se miren mutuamente
        Vector3 lookTarget = new Vector3(enemyTransform.position.x, playerTransform.position.y, enemyTransform.position.z);
        playerTransform.LookAt(lookTarget);
        
        Vector3 enemyLookTarget = new Vector3(playerTransform.position.x, enemyTransform.position.y, playerTransform.position.z);
        enemyTransform.LookAt(enemyLookTarget);
        
        Debug.Log($"Player positioned 30 units in enemy's forward direction. New position: {playerTransform.position}");
        Debug.Log($"Enemy position: {enemyTransform.position}");
        Debug.Log($"Enemy forward direction: {enemyForward}");
    }
    
    /// <summary>
    /// Activa la UI de combate
    /// </summary>
    private void ActivateBattleUI()
    {
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(true);
            Debug.Log("Battle UI Canvas activated");
        }
        else
        {
            Debug.LogWarning("Battle UI Canvas not assigned!");
        }
    }
    
    /// <summary>
    /// Desactiva la UI de combate
    /// </summary>
    private void DeactivateBattleUI()
    {
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
            Debug.Log("Battle UI Canvas deactivated");
        }
    }
    
    /// <summary>
    /// Inicia la batalla en el BattleManager
    /// </summary>
    private void InitiateBattle()
    {
        if (battleManager != null && thisEnemyController != null)
        {
            // Subscribe to battle end event
            if (!isSubscribedToBattle)
            {
                battleManager.OnBattleEnded += HandleBattleEnded;
                isSubscribedToBattle = true;
            }
            
            // Pass combat position setup to BattleManager if available
            if (combatPositionSetup != null)
            {
                battleManager.StartBattleWithEnemy(thisEnemyController, combatPositionSetup);
                if (debugMode) Debug.Log($"<color=cyan>[BattleTrigger] Battle initiated with custom positioning</color>");
            }
            else
            {
                // Use default positioning
                battleManager.StartBattleWithEnemy(thisEnemyController);
                if (debugMode) Debug.Log($"<color=cyan>[BattleTrigger] Battle initiated with default positioning</color>");
            }
            
            if (debugMode) Debug.Log($"<color=cyan>[BattleTrigger] Battle started with {thisEnemyController.name}</color>");
        }
        else
        {
            Debug.LogError("[BattleTrigger] BattleManager or EnemyBattleController not found! Cannot start battle.");
        }
    }
    
    /// <summary>
    /// Maneja el fin de la batalla
    /// </summary>
    private void HandleBattleEnded(BattleResult result)
    {
        if (debugMode) Debug.Log($"<color=yellow>[BattleTrigger] Battle ended with result: {result}</color>");
        
        // Desactivar UI de combate
        DeactivateBattleUI();
        
        // Unsubscribe from event
        if (battleManager != null && isSubscribedToBattle)
        {
            battleManager.OnBattleEnded -= HandleBattleEnded;
            isSubscribedToBattle = false;
        }
        
        // Handle victory/defeat
        if (result == BattleResult.PlayerVictory)
        {
            if (debugMode) Debug.Log("[BattleTrigger] Player victory! Cleaning up...");
            
            // Force reset BattleManager
            if (battleManager != null)
            {
                battleManager.ForceReset();
            }
            
            // Destroy enemy if configured to do so
            if (destroyOnBattleEnd)
            {
                Invoke(nameof(DestroyAfterVictory), 1.5f);
            }
        }
        else
        {
            // Player lost - reset for retry
            if (battleManager != null)
            {
                battleManager.ForceReset();
            }
            
            Invoke(nameof(ResetTrigger), 2f);
        }
    }

    /// <summary>
    /// Destruye el enemigo y trigger después de victoria
    /// </summary>
    private void DestroyAfterVictory()
    {
        if (debugMode) Debug.Log($"<color=red>[BattleTrigger] Destroying defeated enemy: {gameObject.name}</color>");
        Destroy(gameObject); // Destruye todo el enemigo + trigger
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (battleManager != null && isSubscribedToBattle)
        {
            battleManager.OnBattleEnded -= HandleBattleEnded;
            isSubscribedToBattle = false;
        }
    }
    
    /// <summary>
    /// Resetea el trigger para permitir nuevas batallas
    /// </summary>
    public void ResetTrigger()
    {
        battleTriggered = false;
        if (debugMode) Debug.Log($"[BattleTrigger] Trigger reset on: {gameObject.name}");
    }
    
    /// <summary>
    /// Enable/disable this trigger (useful for quest control)
    /// </summary>
    public void SetTriggerActive(bool active)
    {
        enabled = active;
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = active;
        }
        
        if (debugMode) Debug.Log($"[BattleTrigger] Trigger set to {(active ? "ACTIVE" : "INACTIVE")} on {gameObject.name}");
    }
    
    /// <summary>
    /// Check if battle has been triggered
    /// </summary>
    public bool IsBattleTriggered => battleTriggered;
    
    /// <summary>
    /// Get the enemy controller for this trigger
    /// </summary>
    public EnemyBattleController GetEnemy() => thisEnemyController;
}
