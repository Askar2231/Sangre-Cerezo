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
    [SerializeField] private AnimationSequencer playerAnimationSequencer;
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private ParrySystem parrySystem;

    [Header("Player Movement")]
    [SerializeField] private MovimientoV2 playerMovement;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject actionSelectionUI;

    [Header("Camera")]
    [SerializeField] private ThirdPersonJRPGCamera battleCamera;

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
    [SerializeField] private GameObject parryIndicatorPrefab;
    [SerializeField] private float parryIndicatorHeight = 1.5f; // Nueva variable configurable
    [SerializeField] private float parryIndicatorScale = 0.8f;  // Nueva variable configurable
    [SerializeField] private float counterAttackDamage = 25f;
    [SerializeField] private float parryStaminaReward = 30f;
    private GameObject activeParryIndicator;

    [Header("UI Health Display")]
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI enemyHealthText;
    [SerializeField] private TextMeshProUGUI playerStaminaText;

    [Header("UI Turn Display")]
    [SerializeField] private TextMeshProUGUI turnDisplayText; // AGREGAR ESTA LÍNEA

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
            parrySystem.OnParrySuccessWithTiming += HandleParrySuccessWithTiming;
            parrySystem.OnParryFail += HandleParryFail;
        }
    }

    /// <summary>
    /// Initialize battle systems
    /// </summary>
    private void InitializeBattle()
    {
        Debug.Log("=== BATTLE START ===");

        // VERIFICAR que tengamos referencias válidas
        if (playerController == null)
        {
            Debug.LogError("PlayerBattleController is null! Cannot start battle.");
            return;
        }

        if (enemyController == null)
        {
            Debug.LogError("EnemyBattleController is null! Cannot start battle.");
            return;
        }

        // Disable player movement during battle
        DisablePlayerMovement();

        // Initialize AnimationSequencer with player animator
        if (playerAnimationSequencer != null && playerController?.Character?.Animator != null)
        {
            playerAnimationSequencer.Initialize(playerController.Character.Animator, qteManager);
            Debug.Log("AnimationSequencer initialized with player animator");
        }

        // Initialize turn manager con referencias actualizadas
        turnManager = new TurnManager(
            playerController.Character,
            enemyController.Character
        );

        // Initialize controllers with proper AnimationSequencer reference
        playerController.Initialize(playerAnimationSequencer, qteManager);
        enemyController.Initialize(parrySystem);

        // Reset characters
        playerController.ResetForBattle();
        enemyController.ResetForBattle();

        // Subscribe to events (IMPORTANTE: limpiar eventos anteriores)
        UnsubscribeFromEvents(); // Limpiar primero
        SubscribeToEvents();     // Suscribir con nuevo enemigo

        // Subscribe to state changes
        turnManager.OnBattleStateChanged += HandleBattleStateChanged;
        turnManager.OnPlayerTurnStateChanged += HandlePlayerTurnStateChanged;
        turnManager.OnEnemyTurnStateChanged += HandleEnemyTurnStateChanged;

        // Actualizar UI inicial de vida
        UpdateHealthUI();

        // Mostrar que la batalla está comenzando
        UpdateTurnDisplayUI("BATTLE START");

        // RESETEAR resultado de batalla
        battleResult = BattleResult.None;

        // Start battle
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

        // Solo buscar automáticamente si no se pasó un enemigo específico
        if (enemyController == null)
        {
            FindAndAssignNewEnemy();
        }

        Debug.Log("Starting battle from external trigger...");
        InitializeBattle();
    }

    // AGREGAR nuevo método público que recibe el enemigo específico:
    /// <summary>
    /// Inicia la batalla con un enemigo específico
    /// </summary>
    public void StartBattleWithEnemy(EnemyBattleController specificEnemy)
    {
        // VERIFICAR estado actual
        if (battleResult != BattleResult.None)
        {
            Debug.LogWarning($"Battle already in progress or finished! Current result: {battleResult}");
            return;
        }

        // VALIDAR enemigo
        if (specificEnemy == null || specificEnemy.gameObject == null)
        {
            Debug.LogError("Specific enemy is null or destroyed! Cannot start battle.");
            return;
        }

        // ASIGNAR el enemigo específico que activó el trigger
        enemyController = specificEnemy;
        Debug.Log($"Battle started with specific enemy: {enemyController.name}");

        // FORZAR reset del resultado de batalla
        battleResult = BattleResult.None;

        InitializeBattle();
    }

    // El método FindAndAssignNewEnemy() se mantiene como respaldo:
    /// <summary>
    /// Busca y asigna un nuevo enemigo si el actual está destruido o es null
    /// </summary>
    private void FindAndAssignNewEnemy()
    {
        Debug.Log("Searching for enemy automatically...");

        // Buscar EnemyBattleController en la escena (usando el nuevo método de Unity)
        EnemyBattleController[] enemies = FindObjectsByType<EnemyBattleController>(FindObjectsSortMode.None);

        if (enemies.Length > 0)
        {
            // Tomar el primer enemigo encontrado
            enemyController = enemies[0];
            Debug.Log($"Enemy auto-assigned: {enemyController.name}");
        }
        else
        {
            Debug.LogError("No EnemyBattleController found in scene! Cannot start battle.");
        }
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

        // Actualizar display de turno
        UpdateTurnDisplayUI("PLAYER TURN");

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

        // Execute attack (animation handled by PlayerBattleController)
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

        // Execute skill (animation handled by PlayerBattleController)
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

        // Actualizar display de turno
        UpdateTurnDisplayUI("ENEMY TURN");

        turnManager.ChangeEnemyTurnState(EnemyTurnState.Thinking);
        enemyController.ExecuteThinking();
    }

    private void HandleEnemyThinkingComplete()
    {
        Debug.Log("Enemy thinking complete");
        turnManager.ChangeEnemyTurnState(EnemyTurnState.Attacking);

        // Execute enemy attack (animation handled by EnemyBattleController)
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
        
        // Play victory animation for player
        if (playerController?.Character != null)
        {
            playerController.Character.PlayVictoryAnimation();
        }

        // NO destruir aquí, dejar que el BattleTrigger lo maneje
        // StartCoroutine(DestroyEnemyAfterDelay(1.5f)); // COMENTAR ESTA LÍNEA

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
            Debug.Log($"Destroying defeated enemy: {enemyController.name}");
            Destroy(enemyController.gameObject);

            // LIMPIAR referencias para permitir nueva batalla
            enemyController = null;
            turnManager = null;

            Debug.Log("Enemy references cleaned for next battle");
        }
    }

    private void EndBattle()
    {
        Debug.Log($"=== BATTLE END: {battleResult} ===");

        // Mostrar resultado de la batalla
        string resultText = battleResult == BattleResult.PlayerVictory ? "VICTORY!" : "DEFEAT!";
        UpdateTurnDisplayUI(resultText);

        // INVOCAR EVENTO ANTES DE LIMPIAR
        OnBattleEnded?.Invoke(battleResult);

        // Re-enable player movement after battle
        EnablePlayerMovement();

        // LIMPIAR TODO EL ESTADO COMPLETAMENTE
        CleanupBattleState();
    }

    // AGREGAR método para limpiar completamente el estado:
    /// <summary>
    /// Limpia completamente el estado de batalla para permitir nuevas batallas
    /// </summary>
    private void CleanupBattleState()
    {
        // Cleanup events
        UnsubscribeFromEvents();

        // Limpiar referencias de batalla
        enemyController = null;
        turnManager = null;
        battleResult = BattleResult.None;

        // Limpiar parry indicator
        DestroyParryIndicator();

        // Limpiar UI
        HideActionSelectionUI();

        Debug.Log("Battle state completely cleaned for next battle");
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
    /// Updates turn display text
    /// </summary>
    private void UpdateTurnDisplayUI(string turnText)
    {
        if (turnDisplayText != null)
        {
            turnDisplayText.text = turnText;
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
            parrySystem.OnParrySuccessWithTiming -= HandleParrySuccessWithTiming;
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
        Debug.Log("Parry successful! Executing counter-attack!");

        // Ejecutar contrataque automático del jugador
        StartCoroutine(ExecuteCounterAttack());
    }

    private IEnumerator ExecuteCounterAttack()
    {
        // Pequeño delay para la animación de parry
        yield return new WaitForSeconds(0.2f);

        Debug.Log("Counter-attack hitting enemy!");

        // El jugador ejecuta el contrataque
        if (enemyController != null && enemyController.Character != null && enemyController.Character.IsAlive)
        {
            // Ejecutar contrataque con animación
            playerController.ExecuteCounterAttackOnEnemy(enemyController.Character);

            // Restaurar stamina del jugador
            if (playerController != null && playerController.Character != null)
            {
                playerController.Character.StaminaManager.RestoreToMax();
            }

            // Actualizar UI
            UpdateHealthUI();
        }

        // Pequeño delay antes de continuar
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// Handle parry success with timing information
    /// </summary>
    private void HandleParrySuccessWithTiming(bool wasPerfect)
    {
        // Delegate to PlayerBattleController to play parry animation
        if (playerController != null)
        {
            playerController.PlayParryAnimation(wasPerfect);
        }

        // Play enemy stagger animation
        if (enemyController?.Character?.Animator != null)
        {
            enemyController.Character.Animator.Play("Staggered");
            Debug.Log("Playing enemy stagger animation");
        }
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

        // Usar la altura configurable desde el Inspector
        Vector3 spawnPos = enemyController.transform.position + Vector3.up * parryIndicatorHeight;
        Quaternion rot = parryIndicatorPrefab.transform.rotation * Quaternion.Euler(0, 180, 0);

        activeParryIndicator = Instantiate(parryIndicatorPrefab, spawnPos, rot, enemyController.transform);

        // Usar la escala configurable desde el Inspector
        activeParryIndicator.transform.localScale = Vector3.one * parryIndicatorScale;

        // Usar la altura configurable para el FollowTarget también
        activeParryIndicator.AddComponent<FollowTarget>().Init(enemyController.transform, Vector3.up * parryIndicatorHeight);
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

        if (playerAnimationSequencer == null)
            Debug.LogError("AnimationSequencer not assigned!");

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

    /// <summary>
    /// Resetea completamente el BattleManager (llamado desde BattleTrigger)
    /// </summary>
    public void ForceReset()
    {
        CleanupBattleState();
        Debug.Log("BattleManager force reset completed");
    }
    
    /// <summary>
    /// Get the current enemy character (for dynamic UI)
    /// </summary>
    public BattleCharacter GetCurrentEnemy()
    {
        return enemyController?.Character;
    }

    #endregion

    #endregion
}