using UnityEngine;
using System;

/// <summary>
/// Centralized controller for all combat UI elements
/// Manages player health/stamina (screen-space) and enemy health (worldspace)
/// </summary>
public class BattleCombatUIController : MonoBehaviour
{
    [Header("Player UI References (Screen Space)")]
    [SerializeField] private HealthBarUI playerHealthBar;
    [SerializeField] private StaminaOrbsUI playerStaminaOrbs;

    [Header("Enemy UI (Worldspace)")]
    [SerializeField] private GameObject enemyHealthBarPrefab;
    [SerializeField] private Transform worldspaceUIParent;

    [Header("Settings")]
    [SerializeField] private float staminaPerOrb = 20f;

    // Runtime references
    private WorldspaceHealthBar currentEnemyHealthBar;
    private BattleCharacter currentPlayer;
    private BattleCharacter currentEnemy;
    private Camera battleCamera;
    private Transform currentPlayerTransform;
    private Transform currentEnemyTransform;

    private bool isInitialized = false;

    /// <summary>
    /// Initialize the UI controller with camera reference
    /// </summary>
    public void Initialize(Camera camera)
    {
        battleCamera = camera;
        isInitialized = true;
        
        Debug.Log("[BattleCombatUIController] Initialized");
    }

    /// <summary>
    /// Setup player UI elements
    /// </summary>
    public void SetupPlayerUI(BattleCharacter player, Transform playerTransform)
    {
        if (!isInitialized)
        {
            Debug.LogError("[BattleCombatUIController] Cannot setup player UI - controller not initialized!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("[BattleCombatUIController] Cannot setup player UI - player is null!");
            return;
        }

        // Unsubscribe from previous player if any
        if (currentPlayer != null)
        {
            currentPlayer.OnDamageTaken -= OnPlayerHealthChanged;
            if (currentPlayer.StaminaManager != null)
            {
                currentPlayer.StaminaManager.OnStaminaChanged -= OnPlayerStaminaChanged;
            }
        }

        currentPlayer = player;
        currentPlayerTransform = playerTransform;

        // Initialize player health bar
        if (playerHealthBar != null)
        {
            Debug.Log($"[BattleCombatUIController] Initializing player health bar. HP: {player.CurrentHealth}/{player.MaxHealth}");
            playerHealthBar.Initialize(player.MaxHealth);
            playerHealthBar.SetHealthImmediate(player.CurrentHealth, player.MaxHealth);
            playerHealthBar.SetShowNumericalValue(true); // ENABLE health text for player
            playerHealthBar.SetVisible(true);
            Debug.Log($"[BattleCombatUIController] Player health bar initialized. Active: {playerHealthBar.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[BattleCombatUIController] Player health bar not assigned!");
        }

        // Initialize player stamina orbs
        if (playerStaminaOrbs != null && player.StaminaManager != null)
        {
            Debug.Log($"[BattleCombatUIController] Initializing stamina orbs. Stamina: {player.StaminaManager.CurrentStamina}/{player.StaminaManager.MaxStamina}, Per Orb: {staminaPerOrb}");
            playerStaminaOrbs.Initialize(player.StaminaManager.MaxStamina, staminaPerOrb);
            playerStaminaOrbs.UpdateStamina(player.StaminaManager.CurrentStamina);
            playerStaminaOrbs.SetVisible(true);
            Debug.Log($"[BattleCombatUIController] Stamina orbs initialized. Active: {playerStaminaOrbs.gameObject.activeSelf}, Orb Count: {playerStaminaOrbs.OrbCount}");
        }
        else
        {
            if (playerStaminaOrbs == null)
                Debug.LogWarning("[BattleCombatUIController] Player stamina orbs not assigned!");
            if (player.StaminaManager == null)
                Debug.LogWarning("[BattleCombatUIController] Player StaminaManager is null!");
        }

        // Subscribe to player events
        currentPlayer.OnDamageTaken += OnPlayerHealthChanged;
        
        if (currentPlayer.StaminaManager != null)
        {
            currentPlayer.StaminaManager.OnStaminaChanged += OnPlayerStaminaChanged;
        }

        Debug.Log($"[BattleCombatUIController] Player UI setup complete for {player.name}");
    }

    /// <summary>
    /// Setup enemy worldspace health bar
    /// </summary>
    public void SetupEnemyUI(BattleCharacter enemy, Transform enemyTransform, Transform playerTransform)
    {
        if (!isInitialized)
        {
            Debug.LogError("[BattleCombatUIController] Cannot setup enemy UI - controller not initialized!");
            return;
        }

        if (enemy == null || enemyTransform == null)
        {
            Debug.LogError("[BattleCombatUIController] Cannot setup enemy UI - enemy or transform is null!");
            return;
        }

        // Cleanup previous enemy UI
        CleanupEnemyUI();

        currentEnemy = enemy;
        currentEnemyTransform = enemyTransform;

        // Spawn enemy health bar in worldspace
        if (enemyHealthBarPrefab != null)
        {
            Debug.Log($"[BattleCombatUIController] Spawning enemy health bar for {enemy.name} at position {enemyTransform.position}");
            
            Transform parent = worldspaceUIParent != null ? worldspaceUIParent : null;
            
            // Instantiate at enemy position (will be refined by Initialize)
            Vector3 spawnPosition = enemyTransform.position;
            Quaternion spawnRotation = Quaternion.identity;
            
            GameObject healthBarObject = Instantiate(enemyHealthBarPrefab, spawnPosition, spawnRotation, parent);
            
            Debug.Log($"[BattleCombatUIController] Health bar instantiated at {healthBarObject.transform.position}");
            
            // Look for Canvas component first (the WorldspaceHealthBar should be on the Canvas GameObject)
            Canvas worldspaceCanvas = healthBarObject.GetComponentInChildren<Canvas>();
            
            if (worldspaceCanvas != null)
            {
                Debug.Log($"[BattleCombatUIController] Canvas found on: {worldspaceCanvas.gameObject.name}");
                
                // Get WorldspaceHealthBar component from the Canvas GameObject
                currentEnemyHealthBar = worldspaceCanvas.GetComponent<WorldspaceHealthBar>();
                
                if (currentEnemyHealthBar != null)
                {
                    Debug.Log($"[BattleCombatUIController] WorldspaceHealthBar component found on Canvas: {currentEnemyHealthBar.gameObject.name}");
                    Debug.Log($"[BattleCombatUIController] Initializing WorldspaceHealthBar component...");
                    currentEnemyHealthBar.Initialize(
                        enemy, 
                        enemyTransform, 
                        playerTransform, 
                        battleCamera
                    );
                    currentEnemyHealthBar.Show();
                    Debug.Log($"[BattleCombatUIController] Enemy health bar setup complete. Position: {currentEnemyHealthBar.transform.position}");
                }
                else
                {
                    Debug.LogError($"[BattleCombatUIController] Canvas found but missing WorldspaceHealthBar component! Add WorldspaceHealthBar to {worldspaceCanvas.gameObject.name}");
                    Destroy(healthBarObject);
                }
            }
            else
            {
                Debug.LogError($"[BattleCombatUIController] No Canvas component found in prefab {healthBarObject.name}! The prefab must contain a Canvas with WorldspaceHealthBar component.");
                Destroy(healthBarObject);
            }
        }
        else
        {
            Debug.LogWarning("[BattleCombatUIController] Enemy health bar prefab not assigned!");
        }

        // Subscribe to enemy events
        currentEnemy.OnDamageTaken += OnEnemyHealthChanged;

        Debug.Log($"[BattleCombatUIController] Enemy UI setup complete for {enemy.name}");
    }

    /// <summary>
    /// Cleanup enemy UI (call when enemy dies or battle ends)
    /// </summary>
    public void CleanupEnemyUI()
    {
        // Unsubscribe from enemy events
        if (currentEnemy != null)
        {
            currentEnemy.OnDamageTaken -= OnEnemyHealthChanged;
        }

        // Destroy enemy health bar
        if (currentEnemyHealthBar != null)
        {
            Destroy(currentEnemyHealthBar.gameObject);
            currentEnemyHealthBar = null;
        }

        currentEnemy = null;
        currentEnemyTransform = null;

        Debug.Log("[BattleCombatUIController] Enemy UI cleaned up");
    }

    /// <summary>
    /// Show all combat UI
    /// </summary>
    public void ShowUI()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.SetVisible(true);
        }

        if (playerStaminaOrbs != null)
        {
            playerStaminaOrbs.SetVisible(true);
        }

        if (currentEnemyHealthBar != null)
        {
            currentEnemyHealthBar.Show();
        }

        Debug.Log("[BattleCombatUIController] Combat UI shown");
    }

    /// <summary>
    /// Hide all combat UI
    /// </summary>
    public void HideUI()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.SetVisible(false);
        }

        if (playerStaminaOrbs != null)
        {
            playerStaminaOrbs.SetVisible(false);
        }

        if (currentEnemyHealthBar != null)
        {
            currentEnemyHealthBar.Hide();
        }

        Debug.Log("[BattleCombatUIController] Combat UI hidden");
    }

    #region Event Handlers

    /// <summary>
    /// Handle player health change
    /// </summary>
    private void OnPlayerHealthChanged(float damage)
    {
        if (currentPlayer == null || playerHealthBar == null) return;

        playerHealthBar.UpdateHealth(currentPlayer.CurrentHealth, currentPlayer.MaxHealth);
        playerHealthBar.PlayDamageFlash();
    }

    /// <summary>
    /// Handle player stamina change
    /// </summary>
    private void OnPlayerStaminaChanged(float current, float max)
    {
        if (playerStaminaOrbs == null) return;

        playerStaminaOrbs.UpdateStamina(current);
    }

    /// <summary>
    /// Handle enemy health change
    /// </summary>
    private void OnEnemyHealthChanged(float damage)
    {
        if (currentEnemy == null || currentEnemyHealthBar == null) return;

        currentEnemyHealthBar.UpdateHealth(currentEnemy.CurrentHealth, currentEnemy.MaxHealth);
        
        // Trigger damage flash effect
        if (currentEnemyHealthBar.HealthBar != null)
        {
            currentEnemyHealthBar.HealthBar.PlayDamageFlash();
        }
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // Unsubscribe from all events
        if (currentPlayer != null)
        {
            currentPlayer.OnDamageTaken -= OnPlayerHealthChanged;
            
            if (currentPlayer.StaminaManager != null)
            {
                currentPlayer.StaminaManager.OnStaminaChanged -= OnPlayerStaminaChanged;
            }
        }

        CleanupEnemyUI();
    }

    #endregion

    #region Public Getters

    public bool IsInitialized => isInitialized;
    public HealthBarUI PlayerHealthBar => playerHealthBar;
    public StaminaOrbsUI PlayerStaminaOrbs => playerStaminaOrbs;
    public WorldspaceHealthBar EnemyHealthBar => currentEnemyHealthBar;

    #endregion
}
