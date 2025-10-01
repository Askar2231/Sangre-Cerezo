using UnityEngine;
using System;

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
    [SerializeField] private AnimationSequencer playerAnimationSequencer;
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private ParrySystem parrySystem;
    
    [Header("Player Movement")]
    [SerializeField] private MovimientoV2 playerMovement;
    
    [Header("UI (Optional)")]
    [SerializeField] private GameObject actionSelectionUI;
    
    // Core Systems
    private TurnManager turnManager;
    private BattleResult battleResult = BattleResult.None;
    
    // Events for UI
    public event Action<BattleState> OnBattleStateChanged;
    public event Action<BattleResult> OnBattleEnded;
    
    // Properties
    public BattleState CurrentState => turnManager?.CurrentBattleState ?? BattleState.BattleStart;
    public bool IsBattleActive => battleResult == BattleResult.None;
    
    private void Awake()
    {
        ValidateReferences();
    }
    
    private void Start()
    {
        InitializeBattle();
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
        
        // Initialize controllers
        playerController.Initialize(playerAnimationSequencer, qteManager);
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
        
        // Start battle
        battleResult = BattleResult.None;
        turnManager.ChangeBattleState(BattleState.PlayerTurn);
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
        playerController.ExecuteLightAttack(enemyController.Character);
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
        enemyController.ExecuteAttack(playerController.Character);
    }
    
    private void HandleEnemyAttackComplete()
    {
        Debug.Log("Enemy attack complete");
        
        // Check if player is dead
        if (!playerController.Character.IsAlive)
        {
            return; // Death handler will end battle
        }
        
        // End enemy turn
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
    
    private void HandleEnemyDeath()
    {
        Debug.Log("Enemy has been defeated!");
        battleResult = BattleResult.PlayerVictory;
        turnManager.ChangeBattleState(BattleState.BattleEnd);
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
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    #endregion
    
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
        else
        {
            Debug.LogWarning("PlayerMovement reference not assigned! Movement will not be disabled.");
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
    
    #region Validation
    
    private void ValidateReferences()
    {
        if (playerController == null)
            Debug.LogError("PlayerBattleController not assigned!");
        
        if (enemyController == null)
            Debug.LogError("EnemyBattleController not assigned!");
        
        if (playerAnimationSequencer == null)
            Debug.LogError("AnimationSequencer not assigned!");
        
        if (qteManager == null)
            Debug.LogError("QTEManager not assigned!");
        
        if (parrySystem == null)
            Debug.LogError("ParrySystem not assigned!");
        
        if (playerMovement == null)
            Debug.LogWarning("PlayerMovement (MovimientoV2) not assigned! Movement will not be disabled during battle.");
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
}
