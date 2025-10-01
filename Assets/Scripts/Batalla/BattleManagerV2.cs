using UnityEngine;
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
    
    private void Awake()
    {
        ValidateReferences();
    }
    
    private void Start()
    {
       // InitializeBattle();
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
        
        // Mostrar parry icon antes del ataque
        ShowParryIcon();
        
        // Simular tiempo de animación
        yield return new WaitForSeconds(enemyAttackDelay);
        
        Debug.Log("Enemy attack animation complete, executing damage");
        
        // Ejecutar el daño real
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

    [Header("Parry System")]
    [SerializeField] private GameObject parryIndicatorPrefab; // Prefab 3D del indicador
    private GameObject activeParryIndicator;

    /// <summary>
    /// Muestra el icono de parry cuando el enemigo va a atacar
    /// </summary>
    private void ShowParryIcon()
    {
        if (parryIndicatorPrefab == null || enemyController == null) 
        {
            Debug.LogWarning("ParryIndicatorPrefab or EnemyController not assigned!");
            return;
        }

        // Destruir indicador anterior si existe
        if (activeParryIndicator != null)
        {
            Destroy(activeParryIndicator);
        }

        // Crear indicador 3D encima del enemigo (pequeño inicialmente)
        Vector3 spawnPos = enemyController.transform.position + Vector3.up * 2f;
        Quaternion rot = parryIndicatorPrefab.transform.rotation * Quaternion.Euler(0, 180, 0);

        activeParryIndicator = Instantiate(parryIndicatorPrefab, spawnPos, rot, enemyController.transform);
        
        // Comenzar pequeño
        activeParryIndicator.transform.localScale = Vector3.one * 0.2f;

        // Agregar componente para seguir al enemigo
        activeParryIndicator.AddComponent<FollowTarget>().Init(enemyController.transform, Vector3.up * 2f);

        // Esperar hasta que se abra la ventana de parry para animar
        StartCoroutine(WaitForParryWindowAndAnimate());
        
        Debug.Log("3D Parry indicator shown for enemy attack");
    }

    /// <summary>
    /// Espera hasta que se abra la ventana de parry y entonces anima la estrella
    /// </summary>
    private IEnumerator WaitForParryWindowAndAnimate()
    {
        // Usar los mismos valores que el EnemyBattleController
        float parryWindowStartTime = 1.5f;  // Mismo valor que EnemyBattleController
        float damageApplicationTime = 1.7f; // Mismo valor que EnemyBattleController
        
        // Esperar hasta que se abra la ventana de parry
        yield return new WaitForSeconds(parryWindowStartTime);
        
        if (activeParryIndicator != null)
        {
            // Calcular la duración real de la ventana de parry
            float parryWindowDuration = damageApplicationTime - parryWindowStartTime; // = 0.2f
            StartCoroutine(AnimateParryIndicatorDuringWindow(activeParryIndicator.transform, parryWindowDuration));
        }
    }

    /// <summary>
    /// Anima el parry indicator SOLO durante la ventana de parry
    /// </summary>
    private IEnumerator AnimateParryIndicatorDuringWindow(Transform indicator, float duration)
    {
        if (indicator == null) yield break;
        
        float half = duration / 2f;
        float timer = 0f;

        Debug.Log($"Parry window opened! Animating indicator for {duration} seconds");

        while (timer < duration && indicator != null)
        {
            timer += Time.deltaTime;

            float scale = (timer < half)
                ? Mathf.Lerp(0.2f, 1.2f, timer / half)          // Primera mitad: crecer
                : Mathf.Lerp(1.2f, 0.2f, (timer - half) / half); // Segunda mitad: encoger (no a 0)

            indicator.localScale = Vector3.one * scale;

            yield return null;
        }

        // Mantener pequeño hasta que se destruya
        if (indicator != null)
        {
            indicator.localScale = Vector3.one * 0.2f;
        }
        
        // Esperar un poco más antes de destruir (para que el jugador vea el resultado)
        yield return new WaitForSeconds(0.5f);
        
        // Autodestruir
        if (indicator != null)
        {
            Destroy(indicator.gameObject);
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
}
