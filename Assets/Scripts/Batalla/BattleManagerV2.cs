using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Main battle manager that coordinates all battle systems
/// Implements turn-based combat with real-time action execution
/// </summary>
public class BattleManagerV2 : MonoBehaviour
{
    [Header("Battle Participants")]
    [SerializeField] private PlayerBattleController playerController;
    [SerializeField] private EnemyBattleController enemyController;
    
    [Header("Systems")]
    // [SerializeField] private AnimationSequencer playerAnimationSequencer; // COMENTADO - Usando delays temporales
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private ParrySystem parrySystem;
    
    [Header("Player Movement")]
    [SerializeField] private MovimientoV2 playerMovement;
    
    [Header("UI (Optional)")]
    [SerializeField] private GameObject actionSelectionUI;
    
    [Header("Camera")]
    [SerializeField] private ThirdPersonJRPGCamera battleCamera;
    
    [Header("Temporary Animation Settings")]
    [SerializeField] private float playerAttackDelay = 1.0f; // Tiempo simulado de animación de ataque
    [SerializeField] private float playerSkillDelay = 1.2f;  // Tiempo simulado de animación de skill
    [SerializeField] private float enemyAttackDelay = 1.0f;  // Tiempo simulado de animación enemiga
    
    // Core Systems
    private TurnManager turnManager;
    private BattleResult battleResult = BattleResult.None;
    
    // Events for UI
    public event Action<BattleState> OnBattleStateChanged;
    public event Action<BattleResult> OnBattleEnded;
    
    // Properties
    public BattleState CurrentState => turnManager?.CurrentBattleState ?? BattleState.BattleStart;
    public bool IsBattleActive => battleResult == BattleResult.None;
    
    [Header("Parry System")]
    [SerializeField] private GameObject parryIndicatorPrefab; // Prefab 3D del indicador
    private GameObject activeParryIndicator;

    [Header("UI Health Display")]
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private TextMeshProUGUI playerStaminaText; // AGREGAR ESTA LÍNEA
    
    private void Awake()
    {
        ValidateReferences();
    }
    
    // MODIFICAR el método Start() para que NO inicie automáticamente:
    private void Start()
    {
        // Solo suscribirse a eventos del ParrySystem, NO inicializar batalla
        if (parrySystem != null)
        {
            parrySystem.OnParryWindowActive += HandleParryWindowStateChanged;
            parrySystem.OnParrySuccess += HandleParrySuccess;
            parrySystem.OnParryFail += HandleParryFail;
        }
    }
    
    /// <summary>
    /// Initialize battle systems
    /// </summary>
    private void InitializeBattle()
    {
        Debug.Log("=== BATTLE START ===");
        
        // Disable player movement during battle
        DisablePlayerMovement();
        
        // Initialize turn manager
        turnManager = new TurnManager(
            playerController.Character,
            enemyController.Character
        );
        
        // Initialize controllers - pasar null para animationSequencer temporalmente
        playerController.Initialize(null, qteManager);
        enemyController.Initialize(parrySystem);
        
        // Reset characters
        playerController.ResetForBattle();
        enemyController.ResetForBattle();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Subscribe to state changes
        turnManager.OnBattleStateChanged += HandleBattleStateChanged;
        turnManager.OnPlayerTurnStateChanged += HandlePlayerTurnStateChanged;
        turnManager.OnEnemyTurnStateChanged += HandleEnemyTurnStateChanged;
        
        // Actualizar UI inicial de vida
        UpdateHealthUI();
        
        // Start battle
        battleResult = BattleResult.None;
        turnManager.ChangeBattleState(BattleState.PlayerTurn);
    }

    // AGREGAR método público para iniciar batalla desde afuera:
    /// <summary>
    /// Inicia la batalla desde un trigger externo
    /// </summary>
    public void StartBattle()
    {
        if (battleResult != BattleResult.None)
        {
            Debug.LogWarning("Battle already in progress or finished!");
            return;
        }
        
        Debug.Log("Starting battle from external trigger...");
        InitializeBattle();
    }
    
    /// <summary>
    /// Subscribe to all necessary events
    /// </summary>
    private void SubscribeToEvents()
    {
        // Player events
        playerController.OnActionComplete += HandlePlayerActionComplete;
        playerController.Character.OnDeath += HandlePlayerDeath;
        
        // Enemy events
        enemyController.OnThinkingComplete += HandleEnemyThinkingComplete;
        enemyController.OnAttackComplete += HandleEnemyAttackComplete;
        enemyController.Character.OnDeath += HandleEnemyDeath;
    }
    
    #region State Change Handlers
    
    private void HandleBattleStateChanged(BattleState newState)
    {
        Debug.Log($">>> Battle State Changed: {newState}");
        OnBattleStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case BattleState.PlayerTurn:
                StartPlayerTurn();
                break;
            
            case BattleState.EnemyTurn:
                StartEnemyTurn();
                break;
            
            case BattleState.BattleEnd:
                EndBattle();
                break;
        }
    }
    
    private void HandlePlayerTurnStateChanged(PlayerTurnState newState)
    {
        switch (newState)
        {
            case PlayerTurnState.SelectingAction:
                ShowActionSelectionUI();
                break;
            
            case PlayerTurnState.ExecutingAttack:
            case PlayerTurnState.ExecutingSkill:
                HideActionSelectionUI();
                break;
        }
    }
    
    private void HandleEnemyTurnStateChanged(EnemyTurnState newState)
    {
        // Handle enemy turn state changes if needed
    }
    
    #endregion
    
    #region Player Turn
    
    private void StartPlayerTurn()
    {
        Debug.Log("=== PLAYER TURN ===");
        playerController.Character.StaminaManager.RestoreToMax();
        turnManager.ChangePlayerTurnState(PlayerTurnState.SelectingAction);
    }
    
    /// <summary>
    /// Player chooses to attack
    /// </summary>
    public void PlayerChooseAttack()
    {
        if (CurrentState != BattleState.PlayerTurn) return;
        if (!playerController.CanPerformAction(ActionType.LightAttack))
        {
            Debug.LogWarning("Not enough stamina for attack!");
            return;
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingAttack);
        
        // Ejecutar ataque con delay temporal (simulando animación)
        StartCoroutine(ExecutePlayerAttackWithDelay());
    }
    
    /// <summary>
    /// Player chooses to use a skill
    /// </summary>
    public void PlayerChooseSkill(int skillIndex)
    {
        if (CurrentState != BattleState.PlayerTurn) return;
        if (!playerController.CanPerformAction(ActionType.Skill))
        {
            Debug.LogWarning("Not enough stamina for skill!");
            return;
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingSkill);
        
        // Ejecutar skill con delay temporal (simulando animación)
        StartCoroutine(ExecutePlayerSkillWithDelay(skillIndex));
    }
    
    /// <summary>
    /// Simula la animación de ataque del jugador con delay
    /// </summary>
    private IEnumerator ExecutePlayerAttackWithDelay()
    {
        Debug.Log("Player attack animation started (simulated)");
        
        // Simular tiempo de animación
        yield return new WaitForSeconds(playerAttackDelay);
        
        Debug.Log("Player attack animation complete, executing damage");
        
        // Ejecutar el daño real
        playerController.ExecuteLightAttack(enemyController.Character);
    }
    
    /// <summary>
    /// Simula la animación de skill del jugador con delay
    /// </summary>
    private IEnumerator ExecutePlayerSkillWithDelay(int skillIndex)
    {
        Debug.Log($"Player skill {skillIndex} animation started (simulated)");
        
        // Simular tiempo de animación
        yield return new WaitForSeconds(playerSkillDelay);
        
        Debug.Log($"Player skill {skillIndex} animation complete, executing damage");
        
        // Ejecutar el daño real
        playerController.ExecuteSkill(skillIndex, enemyController.Character);
    }
    
    /// <summary>
    /// Player chooses to end turn
    /// </summary>
    public void PlayerEndTurn()
    {
        if (CurrentState != BattleState.PlayerTurn) return;
        
        Debug.Log("Player ends turn");
        turnManager.EndPlayerTurn();
    }
    
    private void HandlePlayerActionComplete()
    {
        Debug.Log("Player action complete");
        
        // Actualizar UI de vida después de la acción del jugador
        UpdateHealthUI();
        
        // Check if enemy is dead
        if (!enemyController.Character.IsAlive)
        {
            return; // Death handler will end battle
        }
        
        // Return to action selection or wait for player to end turn
        turnManager.ChangePlayerTurnState(PlayerTurnState.SelectingAction);
    }
    
    #endregion
    
    #region Enemy Turn
    
    private void StartEnemyTurn()
    {
        Debug.Log("=== ENEMY TURN ===");
        enemyController.Character.StaminaManager.RestoreToMax();
        turnManager.ChangeEnemyTurnState(EnemyTurnState.Thinking);
        enemyController.ExecuteThinking();
    }
    
    private void HandleEnemyThinkingComplete()
    {
        Debug.Log("Enemy thinking complete");
        turnManager.ChangeEnemyTurnState(EnemyTurnState.Attacking);
        
        // Ejecutar ataque enemigo con delay temporal
        StartCoroutine(ExecuteEnemyAttackWithDelay());
    }
    
    /// <summary>
    /// Simula la animación de ataque del enemigo con delay
    /// </summary>
    private IEnumerator ExecuteEnemyAttackWithDelay()
    {
        Debug.Log("Enemy attack animation started (simulated)");
        
        // Simular tiempo de animación
        yield return new WaitForSeconds(enemyAttackDelay);
        
        Debug.Log("Enemy attack animation complete, executing damage");
        
        // Ejecutar el daño real
        enemyController.ExecuteAttack(playerController.Character);
    }
    
    private void HandleEnemyAttackComplete()
    {
        Debug.Log("Enemy attack complete");
        
        // Actualizar UI de vida después del ataque enemigo
        UpdateHealthUI();
        
        // Check if player is dead
        if (!playerController.Character.IsAlive)
        {
            return; // Death handler will end battle
        }
        
        // Delay antes de terminar el turno enemigo
        StartCoroutine(EndEnemyTurnWithDelay(0.8f)); // 800ms delay
    }
    
    /// <summary>
    /// Termina el turno enemigo después de un delay
    /// </summary>
    private IEnumerator EndEnemyTurnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Debug.Log("Enemy turn ending after delay");
        turnManager.EndEnemyTurn();
    }
    
    #endregion
    
    #region Battle End
    
    private void HandlePlayerDeath()
    {
        Debug.Log("Player has been defeated!");
        battleResult = BattleResult.PlayerDefeated;
        turnManager.ChangeBattleState(BattleState.BattleEnd);
    }
    
    // MODIFICAR el método HandleEnemyDeath():
    private void HandleEnemyDeath()
    {
        Debug.Log("Enemy has been defeated!");
        battleResult = BattleResult.PlayerVictory;
        
        // Destruir el GameObject del enemigo después de un breve delay
        StartCoroutine(DestroyEnemyAfterDelay(1.5f));
        
        turnManager.ChangeBattleState(BattleState.BattleEnd);
    }
    
    // AGREGAR nueva corrutina para destruir enemigo:
    /// <summary>
    /// Destruye el enemigo después de un delay para permitir efectos visuales
    /// </summary>
    private IEnumerator DestroyEnemyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (enemyController != null && enemyController.gameObject != null)
        {
            Debug.Log("Destroying defeated enemy...");
            Destroy(enemyController.gameObject);
            enemyController = null; // Limpiar referencia
        }
    }
    
    private void EndBattle()
    {
        Debug.Log($"=== BATTLE END: {battleResult} ===");
        OnBattleEnded?.Invoke(battleResult);
        
        // Re-enable player movement after battle
        EnablePlayerMovement();
        
        // Cleanup
        UnsubscribeFromEvents();
    }
    
    #endregion
    
    #region UI Helpers
    
    private void ShowActionSelectionUI()
    {
        if (actionSelectionUI != null)
        {
            actionSelectionUI.SetActive(true);
        }
    }
    
    private void HideActionSelectionUI()
    {
        if (actionSelectionUI != null)
        {
            actionSelectionUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Updates player health display
    /// </summary>
    private void UpdatePlayerHealthUI()
    {
        if (playerHealthText != null && playerController?.Character != null)
        {
            float currentHP = playerController.Character.CurrentHealth;
            float maxHP = playerController.Character.MaxHealth;
            playerHealthText.text = $"Player HP: {currentHP:F0}/{maxHP:F0}";
        }
    }

    /// <summary>
    /// Updates enemy health display
    /// </summary>
    private void UpdateEnemyHealthUI()
    {
        if (enemyHealthText != null && enemyController?.Character != null)
        {
            float currentHP = enemyController.Character.CurrentHealth;
            float maxHP = enemyController.Character.MaxHealth;
            enemyHealthText.text = $"Enemy HP: {currentHP:F0}/{maxHP:F0}";
        }
    }

    /// <summary>
    /// Updates player stamina display
    /// </summary>
    private void UpdatePlayerStaminaUI()
    {
        if (playerStaminaText != null && playerController?.Character != null)
        {
            float currentStamina = playerController.Character.StaminaManager.CurrentStamina;
            float maxStamina = playerController.Character.StaminaManager.MaxStamina;
            playerStaminaText.text = $"Player Stamina: {currentStamina:F0}/{maxStamina:F0}";
        }
    }

    /// <summary>
    /// Updates both health UI elements
    /// </summary>
    private void UpdateHealthUI()
    {
        UpdatePlayerHealthUI();
        UpdateEnemyHealthUI();
        UpdatePlayerStaminaUI(); // AGREGAR ESTA LÍNEA
    }
    
    #endregion
    
    #region Cleanup
    
    private void UnsubscribeFromEvents()
    {
        if (playerController != null)
        {
            playerController.OnActionComplete -= HandlePlayerActionComplete;
            if (playerController.Character != null)
                playerController.Character.OnDeath -= HandlePlayerDeath;
        }
        
        if (enemyController != null)
        {
            enemyController.OnThinkingComplete -= HandleEnemyThinkingComplete;
            enemyController.OnAttackComplete -= HandleEnemyAttackComplete;
            if (enemyController.Character != null)
                enemyController.Character.OnDeath -= HandleEnemyDeath;
        }
        
        if (turnManager != null)
        {
            turnManager.OnBattleStateChanged -= HandleBattleStateChanged;
            turnManager.OnPlayerTurnStateChanged -= HandlePlayerTurnStateChanged;
            turnManager.OnEnemyTurnStateChanged -= HandleEnemyTurnStateChanged;
        }
        
        // Desuscribirse de eventos del ParrySystem
        if (parrySystem != null)
        {
            parrySystem.OnParryWindowActive -= HandleParryWindowStateChanged;
            parrySystem.OnParrySuccess -= HandleParrySuccess;
            parrySystem.OnParryFail -= HandleParryFail;
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    #region Movement Control
    
    /// <summary>
    /// Disable player movement when entering battle
    /// </summary>
    private void DisablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
            Debug.Log("Player movement disabled for battle");
        }
    }
    
    /// <summary>
    /// Re-enable player movement when exiting battle
    /// </summary>
    private void EnablePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
            Debug.Log("Player movement re-enabled after battle");
        }
    }
    
    #endregion
    
    #region Parry System

    /// <summary>
    /// Handle the parry window state changes from ParrySystem
    /// </summary>
    private void HandleParryWindowStateChanged(bool isActive)
    {
        if (isActive)
        {
            CreateParryIndicator();
        }
        else
        {
            DestroyParryIndicator();
        }
    }

    /// <summary>
    /// Handle parry success event
    /// </summary>
    private void HandleParrySuccess()
    {
        DestroyParryIndicator();
        Debug.Log("Parry successful!");
    }

    /// <summary>
    /// Handle parry fail event
    /// </summary>
    private void HandleParryFail()
    {
        Debug.Log("Parry failed!");
    }

    /// <summary>
    /// Creates the parry indicator when window opens
    /// </summary>
    private void CreateParryIndicator()
    {
        if (parryIndicatorPrefab == null || enemyController == null || enemyController.gameObject == null) 
            return;
        
        if (activeParryIndicator != null)
        {
            Destroy(activeParryIndicator);
        }
        
        Debug.Log("Parry window opened! Creating indicator");
        
        // Ajustar altura del indicador para jugador pequeño
        Vector3 spawnPos = enemyController.transform.position + Vector3.up * 1.5f; // REDUCIR de 2f a 1.5f
        Quaternion rot = parryIndicatorPrefab.transform.rotation * Quaternion.Euler(0, 180, 0);

        activeParryIndicator = Instantiate(parryIndicatorPrefab, spawnPos, rot, enemyController.transform);
        activeParryIndicator.transform.localScale = Vector3.one * 0.8f; // REDUCIR de 1.0f a 0.8f
        activeParryIndicator.AddComponent<FollowTarget>().Init(enemyController.transform, Vector3.up * 1.5f);
    }

    /// <summary>
    /// Destroys the parry indicator
    /// </summary>
    private void DestroyParryIndicator()
    {
        if (activeParryIndicator != null)
        {
            Destroy(activeParryIndicator);
            activeParryIndicator = null;
            Debug.Log("Parry indicator destroyed");
        }
    }

    #endregion
    
    #region Validation
    
    private void ValidateReferences()
    {
        if (playerController == null)
            Debug.LogError("PlayerBattleController not assigned!");
        
        if (enemyController == null)
            Debug.LogError("EnemyBattleController not assigned!");
        
        // AnimationSequencer comentado temporalmente
        // if (playerAnimationSequencer == null)
        //     Debug.LogError("AnimationSequencer not assigned!");
        
        if (qteManager == null)
            Debug.LogError("QTEManager not assigned!");
        
        if (parrySystem == null)
            Debug.LogError("ParrySystem not assigned!");
        
        if (playerMovement == null)
            Debug.LogWarning("PlayerMovement (MovimientoV2) not assigned! Movement will not be disabled during battle.");
        
        if (parryIndicatorPrefab == null)
            Debug.LogWarning("ParryIndicatorPrefab not assigned! Parry indicators will not show.");
    }
    
    #endregion
    
    #region Public API for Testing
    
    /// <summary>
    /// For debugging/testing - manually trigger state changes
    /// </summary>
    public void DebugStartBattle()
    {
        InitializeBattle();
    }
    
    /// <summary>
    /// Get current stamina info for UI
    /// </summary>
    public (float current, float max) GetPlayerStamina()
    {
        return (
            playerController.Character.StaminaManager.CurrentStamina,
            playerController.Character.StaminaManager.MaxStamina
        );
    }
    
    #endregion
    
    #endregion
}