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
    [SerializeField] private EnemyBattleController enemyController; // Optional: can be set dynamically via StartBattleWithEnemy()

    [Header("Systems")]
    [SerializeField] private AnimationSequencer playerAnimationSequencer;
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private ParrySystem parrySystem;
    [SerializeField] private BattleInputManager inputManager; // NEW: Centralized input handling
    [SerializeField] private BattleNotificationSystem notificationSystem;

    [Header("Player Movement")]
    [SerializeField] private MovimientoV2 playerMovement;

    [Header("UI")]
    [SerializeField] private BattleUIButtonController uiButtonController;
    [SerializeField] private GameObject actionSelectionUI; // Optional legacy UI
    [SerializeField] private GameObject generalUICanvas; // UI shown outside of combat (Karma, etc.)
    [SerializeField] private GameObject battleUICanvas; // UI shown during combat (optional, if you have a separate battle canvas)

    [Header("Camera")]
    [SerializeField] private ThirdPersonJRPGCamera battleCamera;
    
    [Header("Combat Positioning (Optional)")]
    [Tooltip("Optional: Set this to use specific combat positions. Can be set dynamically by BattleTrigger.")]
    [SerializeField] private BattlePositionSetup combatPositionSetup;
    
    [Header("Camera Fade Settings")]
    [Tooltip("Fade duration when transitioning to combat positions")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeHoldDuration = 0.1f;

    // Core Systems
    private TurnManager turnManager;
    private BattleResult battleResult = BattleResult.None;
    
    // Position tracking
    private Vector3 playerOriginalPosition;
    private Quaternion playerOriginalRotation;
    private bool isTransitioningPositions = false;
    
    // Damage tracking for notifications
    private bool lastAttackHadQTE = false;
    private bool lastQTEWasSuccessful = false;

    // Events for UI
    public event Action<BattleState> OnBattleStateChanged;
    public event Action<BattleResult> OnBattleEnded;

    // Properties
    private BattleState currentBattleState = BattleState.BattleStart;
    public BattleState CurrentState => currentBattleState;
    public bool IsBattleActive => battleResult == BattleResult.None;

    [Header("Parry System")]
    [SerializeField] private ParryIcon parryIconSpawner; // Reference to the ParryIconSpawner object
    
    [Header("Parry Rewards")]
    [SerializeField] private float counterAttackDamage = 25f;
    [SerializeField] private float parryStaminaReward = 30f;
    [SerializeField] private string counterAttackAnimationState = "CounterAttack";

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

    // MODIFICAR el método Start() para conectar ParrySystem al InputManager:
    private void Start()
    {
        Debug.Log("<color=cyan>[BattleManager]</color> 🎮 Start() - Setting up event subscriptions...");
        
        // Subscribe to BattleInputManager events
        if (inputManager != null)
        {
            Debug.Log("<color=lime>[BattleManager]</color> ✅ Subscribing to BattleInputManager events");
            inputManager.OnLightAttackRequested += HandleInputLightAttack;
            inputManager.OnHeavyAttackRequested += HandleInputHeavyAttack;
            inputManager.OnSkill1Requested += HandleInputSkill1;
            inputManager.OnSkill2Requested += HandleInputSkill2;
            inputManager.OnEndTurnRequested += HandleInputEndTurn;
            
            // CONECTAR ParrySystem al InputManager
            if (parrySystem != null && inputManager != null)
            {
                // Asegurarse de que el InputManager tenga la referencia al ParrySystem
                // Si el InputManager no tiene el ParrySystem asignado, asignarlo por código
                var parrySystemField = typeof(BattleInputManager).GetField("parrySystem", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (parrySystemField != null && parrySystemField.GetValue(inputManager) == null)
                {
                    parrySystemField.SetValue(inputManager, parrySystem);
                    Debug.Log("<color=lime>[BattleManager]</color> ✅ ParrySystem assigned to InputManager via code");
                }
            }
            
            Debug.Log("<color=lime>[BattleManager]</color> ✅ BattleInputManager event subscriptions complete");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BattleManager]</color> ⚠️ BattleInputManager is NULL! Input will not work!");
        }
        
        // Subscribe to ParrySystem events
        if (parrySystem != null)
        {
            Debug.Log("<color=lime>[BattleManager]</color> ✅ Subscribing to ParrySystem events");
            parrySystem.OnParryWindowActive += HandleParryWindowStateChanged;
            parrySystem.OnParrySuccess += HandleParrySuccess;
            parrySystem.OnParrySuccessWithTiming += HandleParrySuccessWithTiming;
            parrySystem.OnParryFail += HandleParryFail;
            Debug.Log("<color=lime>[BattleManager]</color> ✅ ParrySystem event subscriptions complete");
            
            // VERIFY subscription worked
            Debug.Log($"<color=cyan>[BattleManager]</color> 🔍 Subscription verified - HandleParrySuccess is now listening for OnParrySuccess events");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BattleManager]</color> ⚠️ ParrySystem is NULL! Cannot subscribe to events!");
        }
        
        // Subscribe to QTEManager events
        if (qteManager != null)
        {
            Debug.Log("<color=lime>[BattleManager]</color> ✅ Subscribing to QTEManager events");
            qteManager.OnQTEWindowStart += HandleQTEWindowStateChanged;
            qteManager.OnQTEWindowEnd += HandleQTEWindowEnd;
            qteManager.OnQTESuccess += HandleQTESuccess;
            qteManager.OnQTEFail += HandleQTEFail;
            
            // ASEGURAR que QTEManager esté conectado al InputManager también
            var qteManagerField = typeof(BattleInputManager).GetField("qteManager", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (qteManagerField != null && qteManagerField.GetValue(inputManager) == null)
            {
                qteManagerField.SetValue(inputManager, qteManager);
                Debug.Log("<color=lime>[BattleManager]</color> ✅ QTEManager assigned to InputManager via code");
            }
            
            Debug.Log("<color=lime>[BattleManager]</color> ✅ QTEManager event subscriptions complete");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BattleManager]</color> ⚠️ QTEManager is NULL! Cannot subscribe to events!");
        }
        
        // Subscribe to UI button events
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
            uiButtonController.SetButtonsVisible(false);
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

        // Hide general UI and show battle UI
        SetGeneralUIVisible(false);
        SetBattleUIVisible(true);

        // Disable player movement during battle
        DisablePlayerMovement();
        
        // Move combatants to combat positions if setup is provided
        if (combatPositionSetup != null && combatPositionSetup.IsValid())
        {
            // Use camera fade with instant positioning during fade
            StartCoroutine(TransitionWithFade());
        }
        else
        {
            Debug.LogWarning("No BattlePositionSetup assigned or invalid - using current positions");
        }

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
        playerController.Initialize(playerAnimationSequencer, qteManager, notificationSystem);
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
    /// This is the RECOMMENDED way to start battles with dynamically spawned enemies
    /// </summary>
    public void StartBattleWithEnemy(EnemyBattleController specificEnemy)
    {
        // VERIFICAR estado actual
        if (battleResult != BattleResult.None && turnManager != null)
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
        Debug.Log($"<color=cyan>Battle started with specific enemy: {enemyController.name}</color>");

        // FORZAR reset del resultado de batalla
        battleResult = BattleResult.None;

        InitializeBattle();
    }
    
    /// <summary>
    /// Start battle with specific enemy and combat positioning
    /// </summary>
    public void StartBattleWithEnemy(EnemyBattleController specificEnemy, BattlePositionSetup positionSetup)
    {
        // Set the position setup before initializing
        combatPositionSetup = positionSetup;
        
        // Call the standard method
        StartBattleWithEnemy(specificEnemy);
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
        playerController.Character.OnDamageTaken += HandlePlayerDamageTaken;

        // Enemy events
        enemyController.OnThinkingComplete += HandleEnemyThinkingComplete;
        enemyController.OnAttackComplete += HandleEnemyAttackComplete;
        enemyController.Character.OnDeath += HandleEnemyDeath;
        enemyController.Character.OnDamageTaken += HandleEnemyDamageTaken;
        
        // ParrySystem events (CRITICAL: Re-subscribe after UnsubscribeFromEvents)
        if (parrySystem != null)
        {
            Debug.Log("<color=lime>[BattleManager]</color> 🔄 Re-subscribing to ParrySystem events in SubscribeToEvents()");
            parrySystem.OnParryWindowActive += HandleParryWindowStateChanged;
            parrySystem.OnParrySuccess += HandleParrySuccess;
            parrySystem.OnParrySuccessWithTiming += HandleParrySuccessWithTiming;
            parrySystem.OnParryFail += HandleParryFail;
            Debug.Log("<color=lime>[BattleManager]</color> ✅ ParrySystem re-subscribed successfully!");
        }
    }

    #region State Change Handlers

    private void HandleBattleStateChanged(BattleState newState)
    {
        Debug.Log($">>> Battle State Changed: {newState}");
        currentBattleState = newState; // ACTUALIZAR estado local
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
                currentBattleState = BattleState.BattleEnd; // ACTUALIZAR
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
                // Enable player turn input
                if (inputManager != null)
                {
                    inputManager.SetInputState(BattleInputState.PlayerTurn);
                }
                break;

            case PlayerTurnState.ExecutingAttack:
            case PlayerTurnState.ExecutingSkill:
                HideActionSelectionUI();
                // Input state already set by input handlers
                break;
        }
    }

    private void HandleEnemyTurnStateChanged(EnemyTurnState newState)
    {
        // Handle enemy turn state changes if needed
    }

    #endregion

    #region Player Turn

    // CORREGIR StartPlayerTurn() para habilitar input correctamente:
    private void StartPlayerTurn()
    {
        Debug.Log("=== PLAYER TURN START ===");
        currentBattleState = BattleState.PlayerTurn; // AGREGAR esta línea
        
        playerController.Character.StaminaManager.RestoreToMax();
        UpdateTurnDisplayUI("PLAYER TURN");
        
        // Enable UI buttons for player input
        if (uiButtonController != null)
        {
            uiButtonController.EnableInput();
            uiButtonController.SetButtonsVisible(true);
        }
        
        // HABILITAR INPUT MANAGER CORRECTAMENTE
        if (inputManager != null)
        {
            Debug.Log($"Enabling input. Current state: {inputManager.CurrentInputState}");
            inputManager.EnablePlayerTurnInput();
            Debug.Log($"Input enabled. New state: {inputManager.CurrentInputState}");
        }
        else
        {
            Debug.LogError("InputManager is NULL in StartPlayerTurn!");
        }
        
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
        Debug.Log("=== PLAYER ACTION COMPLETE ===");
        
        // Actualizar UI de vida después de la acción del jugador
        UpdateHealthUI();
        
        // Check if enemy is dead
        if (!enemyController.Character.IsAlive)
        {
            Debug.Log("Enemy died - not restoring input");
            return; // Death handler will end battle
        }
        
        // RESTAURAR INPUT DEL JUGADOR para permitir más acciones
        if (inputManager != null)
        {
            Debug.Log($"Restoring input from: {inputManager.CurrentInputState}");
            inputManager.EnablePlayerTurnInput();
            Debug.Log($"Input restored to: {inputManager.CurrentInputState}");
        }
        
        // Enable UI buttons again
        if (uiButtonController != null)
        {
            uiButtonController.EnableInput();
        }
        
        // Return to action selection
        turnManager.ChangePlayerTurnState(PlayerTurnState.SelectingAction);
    }

    #region Input Manager Event Handlers (NEW - Centralized Input)
    
    /// <summary>
    /// Handle light attack input from BattleInputManager
    /// </summary>
    private void HandleInputLightAttack()
    {
        if (currentBattleState != BattleState.PlayerTurn) return;
        
        if (!playerController.CanPerformAction(ActionType.LightAttack))
        {
            if (notificationSystem != null)
                notificationSystem.ShowInsufficientResources("ataque ligero");
            return;
        }
        
        Debug.Log("[Input] Light Attack requested");
        
        // Update input state
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.ExecutingAction);
        }
        
        // Disable UI while executing
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingAttack);
        playerController.ExecuteLightAttack(enemyController.Character);
    }
    
    /// <summary>
    /// Handle heavy attack input from BattleInputManager
    /// </summary>
    private void HandleInputHeavyAttack()
    {
        if (currentBattleState != BattleState.PlayerTurn) return;
        
        if (!playerController.CanPerformAction(ActionType.HeavyAttack))
        {
            if (notificationSystem != null)
                notificationSystem.ShowInsufficientResources("ataque pesado");
            return;
        }
        
        Debug.Log("[Input] Heavy Attack requested");
        
        // Update input state
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.ExecutingAction);
        }
        
        // Disable UI while executing
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingAttack);
        playerController.ExecuteHeavyAttack(enemyController.Character);
    }
    
    /// <summary>
    /// Handle skill 1 input from BattleInputManager
    /// </summary>
    private void HandleInputSkill1()
    {
        if (currentBattleState != BattleState.PlayerTurn) return;
        
        if (!playerController.CanPerformAction(ActionType.Skill))
        {
            if (notificationSystem != null)
                notificationSystem.ShowInsufficientResources("habilidad 1");
            return;
        }
        
        Debug.Log("[Input] Skill 1 requested");
        
        // Update input state
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.ExecutingAction);
        }
        
        // Disable UI while executing
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingSkill);
        playerController.ExecuteSkill(0, enemyController.Character);
    }
    
    /// <summary>
    /// Handle skill 2 input from BattleInputManager
    /// </summary>
    private void HandleInputSkill2()
    {
        if (currentBattleState != BattleState.PlayerTurn) return;
        
        if (!playerController.CanPerformAction(ActionType.Skill))
        {
            if (notificationSystem != null)
                notificationSystem.ShowInsufficientResources("habilidad 2");
            return;
        }
        
        Debug.Log("[Input] Skill 2 requested");
        
        // Update input state
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.ExecutingAction);
        }
        
        // Disable UI while executing
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
        }
        
        turnManager.ChangePlayerTurnState(PlayerTurnState.ExecutingSkill);
        playerController.ExecuteSkill(1, enemyController.Character);
    }
    
    /// <summary>
    /// Handle end turn input from BattleInputManager
    /// </summary>
    private void HandleInputEndTurn()
    {
        if (currentBattleState != BattleState.PlayerTurn) return;
        
        Debug.Log("[Input] End Turn requested");
        
        // Disable input
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.Disabled);
        }
        
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
        }
        
        turnManager.EndPlayerTurn();
    }
    
    #endregion

    #endregion

    #region Enemy Turn

    private void StartEnemyTurn()
    {
        Debug.Log("=== ENEMY TURN ===");
        currentBattleState = BattleState.EnemyTurn; // ACTUALIZAR estado
        
        enemyController.Character.StaminaManager.RestoreToMax();
        UpdateTurnDisplayUI("ENEMY TURN");
        
        // DESHABILITAR INPUT
        if (inputManager != null)
        {
            inputManager.SetInputState(BattleInputState.Disabled);
            Debug.Log("Input disabled for enemy turn");
        }
        
        if (uiButtonController != null)
        {
            uiButtonController.DisableInput();
            uiButtonController.SetButtonsVisible(false);
        }
        
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

        // NUEVO: NO destruir el enemigo automáticamente
        // Dejar que el QuestManager maneje la destrucción después de la decisión del jugador
        
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
        
        // Show general UI and hide battle UI
        SetGeneralUIVisible(true);
        SetBattleUIVisible(false);
        
        // Return player to original position if we moved them
        if (combatPositionSetup != null)
        {
            StartCoroutine(ReturnFromCombatPositionsAndCleanup());
        }
        else
        {
            // Re-enable player movement after battle
            EnablePlayerMovement();

            // LIMPIAR TODO EL ESTADO COMPLETAMENTE
            CleanupBattleState();
        }
    }
    
    /// <summary>
    /// Return from combat positions then cleanup (used when transitioning positions)
    /// </summary>
    private IEnumerator ReturnFromCombatPositionsAndCleanup()
    {
        // Use camera fade for return
        if (CameraFadeController.Instance != null)
        {
            yield return CameraFadeController.Instance.FadeOutAndIn(
                actionDuringFade: () => ReturnPlayerToOriginalPositionInstantly(),
                fadeOutDuration: fadeOutDuration * 0.8f, // Slightly faster on exit
                fadeInDuration: fadeInDuration,
                holdDuration: fadeHoldDuration * 0.5f
            );
        }
        else
        {
            ReturnPlayerToOriginalPositionInstantly();
            yield return null;
        }
        
        // Re-enable player movement after battle
        EnablePlayerMovement();

        // LIMPIAR TODO EL ESTADO COMPLETAMENTE
        CleanupBattleState();
    }
    
    /// <summary>
    /// Instantly return player to original position (called during fade)
    /// </summary>
    private void ReturnPlayerToOriginalPositionInstantly()
    {
        if (playerController?.Character?.gameObject != null)
        {
            Transform playerTransform = playerController.Character.transform;
            playerTransform.position = playerOriginalPosition;
            playerTransform.rotation = playerOriginalRotation;
            
            Debug.Log("Player returned to original position (instant)");
        }
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
    
    /// <summary>
    /// Show or hide the general UI (shown outside of combat)
    /// </summary>
    private void SetGeneralUIVisible(bool visible)
    {
        if (generalUICanvas != null)
        {
            generalUICanvas.SetActive(visible);
            Debug.Log($"<color=cyan>[BattleManager]</color> General UI set to: {(visible ? "VISIBLE" : "HIDDEN")}");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[BattleManager]</color> GeneralUICanvas reference is not assigned!");
        }
    }
    
    /// <summary>
    /// Show or hide the battle UI (shown during combat)
    /// </summary>
    private void SetBattleUIVisible(bool visible)
    {
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(visible);
            Debug.Log($"<color=cyan>[BattleManager]</color> Battle UI set to: {(visible ? "VISIBLE" : "HIDDEN")}");
        }
        // Note: If you don't have a separate battle UI canvas, you can leave this empty
        // The action selection UI is already handled separately
    }

    #endregion

    #region Cleanup

    private void UnsubscribeFromEvents()
    {
        if (playerController != null)
        {
            playerController.OnActionComplete -= HandlePlayerActionComplete;
            if (playerController.Character != null)
            {
                playerController.Character.OnDeath -= HandlePlayerDeath;
                playerController.Character.OnDamageTaken -= HandlePlayerDamageTaken;
            }
        }

        if (enemyController != null)
        {
            enemyController.OnThinkingComplete -= HandleEnemyThinkingComplete;
            enemyController.OnAttackComplete -= HandleEnemyAttackComplete;
            if (enemyController.Character != null)
            {
                enemyController.Character.OnDeath -= HandleEnemyDeath;
                enemyController.Character.OnDamageTaken -= HandleEnemyDamageTaken;
            }
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
        
        // Unsubscribe from QTE events
        if (qteManager != null)
        {
            qteManager.OnQTEWindowStart -= HandleQTEWindowStateChanged;
            qteManager.OnQTEWindowEnd -= HandleQTEWindowEnd;
            qteManager.OnQTESuccess -= HandleQTESuccess;
            qteManager.OnQTEFail -= HandleQTEFail;
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
    
    #region Combat Positioning
    
    /// <summary>
    /// P with camera fade - instant positioning during black screen
    /// </summary>
    private IEnumerator TransitionWithFade()
    {
        if (combatPositionSetup == null || !combatPositionSetup.IsValid())
        {
            yield break;
        }
        
        isTransitioningPositions = true;
        
        // Save original positions
        if (playerController?.Character?.gameObject != null)
        {
            playerOriginalPosition = playerController.Character.transform.position;
            playerOriginalRotation = playerController.Character.transform.rotation;
        }
        
        Debug.Log("<color=cyan>Starting camera fade transition to combat positions</color>");
        
        // Use camera fade system
        if (CameraFadeController.Instance != null)
        {
            yield return CameraFadeController.Instance.FadeOutAndIn(
                actionDuringFade: () => PositionCharactersInstantly(),
                fadeOutDuration: fadeOutDuration,
                fadeInDuration: fadeInDuration,
                holdDuration: fadeHoldDuration
            );
        }
        else
        {
            // Fallback: instant positioning without fade
            Debug.LogWarning("CameraFadeController not found! Positioning instantly without fade.");
            PositionCharactersInstantly();
            yield return null;
        }
        
        isTransitioningPositions = false;
        Debug.Log("<color=green>Combat positioning with fade complete!</color>");
    }
    
    /// <summary>
    /// Instantly position all characters (called during fade)
    /// </summary>
    private void PositionCharactersInstantly()
    {
        if (combatPositionSetup == null || !combatPositionSetup.IsValid())
        {
            return;
        }
        
        Transform playerTarget = combatPositionSetup.PlayerCombatPosition;
        Transform enemyTarget = combatPositionSetup.EnemyCombatPosition;
        Transform cameraTarget = combatPositionSetup.CameraCombatPosition;
        
        // Position player
        if (playerController?.Character?.gameObject != null && playerTarget != null)
        {
            Transform playerTransform = playerController.Character.transform;
            playerTransform.position = playerTarget.position;
            
            if (combatPositionSetup.AutoRotateToFaceOpponent && enemyTarget != null)
            {
                Vector3 lookDir = (enemyTarget.position - playerTransform.position).normalized;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                {
                    playerTransform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
            else
            {
                playerTransform.rotation = playerTarget.rotation;
            }
            
            Debug.Log($"Player positioned at: {playerTarget.position}");
        }
        
        // Position enemy
        if (enemyController?.Character?.gameObject != null && enemyTarget != null)
        {
            Transform enemyTransform = enemyController.Character.transform;
            enemyTransform.position = enemyTarget.position;
            
            if (combatPositionSetup.AutoRotateToFaceOpponent && playerTarget != null)
            {
                Vector3 lookDir = (playerTarget.position - enemyTransform.position).normalized;
                lookDir.y = 0f;
                if (lookDir != Vector3.zero)
                {
                    enemyTransform.rotation = Quaternion.LookRotation(lookDir);
                }
            }
            else
            {
                enemyTransform.rotation = enemyTarget.rotation;
            }
            
            Debug.Log($"Enemy positioned at: {enemyTarget.position}");
        }
        
        // Position camera
        if (battleCamera != null && cameraTarget != null)
        {
            Transform cameraTransform = battleCamera.transform;
            cameraTransform.position = cameraTarget.position;
            cameraTransform.rotation = cameraTarget.rotation;
            
            Debug.Log($"Camera positioned at: {cameraTarget.position}");
        }
    }
    
    /// <summary>
    /// Set combat position setup dynamically (called by BattleTrigger)
    /// </summary>
    public void SetCombatPositionSetup(BattlePositionSetup setup)
    {
        combatPositionSetup = setup;
    }
    
    #endregion

    #region Parry System

    /// <summary>
    /// Handle the parry window state changes from ParrySystem
    /// </summary>
    private void HandleParryWindowStateChanged(bool isActive)
    {
        Debug.Log($"<color=cyan>[BattleManager]</color> 🛡️ HandleParryWindowStateChanged({isActive}) called from ParrySystem");
        
        // Notify BattleInputManager about parry window state
        if (inputManager != null)
        {
            Debug.Log($"<color=cyan>[BattleManager]</color> 📤 Calling BattleInputManager.SetParryWindowActive({isActive})");
            inputManager.SetParryWindowActive(isActive);
            
            // VERIFICAR que se aplicó correctamente
            Debug.Log($"<color=cyan>[BattleManager]</color> 🔍 After SetParryWindowActive: InputManager.IsParryWindowActive = {inputManager.IsParryWindowActive}");
            Debug.Log($"<color=cyan>[BattleManager]</color> 🔍 After SetParryWindowActive: InputManager.CurrentInputState = {inputManager.CurrentInputState}");
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[BattleManager]</color> ⚠️ InputManager is NULL! Cannot notify parry window state!");
        }
        
        // Visual feedback
        if (isActive)
        {
            CreateParryIndicator();
            Debug.Log($"<color=lime>[BattleManager]</color> ✅ Parry window OPENED - Players should be able to parry now!");
        }
        else
        {
            DestroyParryIndicator();
            Debug.Log($"<color=red>[BattleManager]</color> ❌ Parry window CLOSED");
        }
    }
    
    /// <summary>
    /// Handle QTE window state changes from QTEManager (NEW)
    /// </summary>
    private void HandleQTEWindowStateChanged(bool isActive)
    {
        Debug.Log($"<color=cyan>[BattleManager]</color> ⚡ HandleQTEWindowStateChanged({isActive}) called from QTEManager");
        
        // Notify BattleInputManager about QTE window state
        if (inputManager != null)
        {
            Debug.Log($"<color=cyan>[BattleManager]</color> 📤 Notifying BattleInputManager.SetQTEWindowActive({isActive})");
            inputManager.SetQTEWindowActive(isActive);
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[BattleManager]</color> ⚠️ InputManager is NULL! Cannot notify QTE window state!");
        }
        
        // Track that this attack has a QTE
        if (isActive)
        {
            lastAttackHadQTE = true;
            lastQTEWasSuccessful = false; // Reset until we know the result
        }
        
        Debug.Log($"<color=cyan>[BattleManager]</color> QTE Window: {(isActive ? "<color=lime>OPEN</color>" : "<color=red>CLOSED</color>")}");
    }
    
    /// <summary>
    /// Handle QTE window ending (CORREGIDO)
    /// </summary>
    private void HandleQTEWindowEnd()
    {
        Debug.Log("QTE Window ended - restoring player input");
        
        // RESTAURAR input a PlayerTurn si estamos en turno del jugador
        if (inputManager != null && currentBattleState == BattleState.PlayerTurn)
        {
            inputManager.EnablePlayerTurnInput();
            Debug.Log($"Input restored after QTE: {inputManager.CurrentInputState}");
        }
    }
    
    /// <summary>
    /// Handle QTE success - Show notification with damage dealt
    /// </summary>
    private void HandleQTESuccess()
    {
        Debug.Log("QTE Success! Perfect attack!");
        lastQTEWasSuccessful = true;
        // Notification will be shown when damage is actually dealt (HandleEnemyDamageTaken)
    }
    
    /// <summary>
    /// Handle QTE fail - Show notification for normal attack
    /// </summary>
    private void HandleQTEFail()
    {
        Debug.Log("QTE Failed! Normal attack.");
        lastQTEWasSuccessful = false;
        // Notification will be shown when damage is actually dealt (HandleEnemyDamageTaken)
    }
    
    /// <summary>
    /// Handle damage dealt to enemy - Show appropriate notification
    /// </summary>
    private void HandleEnemyDamageTaken(float damage)
    {
        Debug.Log($"Enemy took {damage} damage");
        
        if (notificationSystem == null) return;
        
        // Check if this damage was from a QTE attack
        if (lastAttackHadQTE)
        {
            if (lastQTEWasSuccessful)
            {
                notificationSystem.ShowQTESuccess(damage);
            }
            else
            {
                notificationSystem.ShowQTEFailed(damage);
            }
            
            // Reset QTE tracking
            lastAttackHadQTE = false;
            lastQTEWasSuccessful = false;
        }
        else
        {
            // Normal damage (no QTE, or from counter attack, etc.)
            notificationSystem.ShowDamageDealt(damage);
        }
    }
    
    /// <summary>
    /// Handle damage dealt to player - Show notification
    /// </summary>
    private void HandlePlayerDamageTaken(float damage)
    {
        Debug.Log($"Player took {damage} damage");
        
        // Only show notification if it wasn't parried
        // (Parry notifications are shown via HandleParrySuccessWithTiming)
        if (notificationSystem != null && parrySystem != null)
        {
            // If parry window wasn't active, this is normal damage
            if (!parrySystem.IsParryWindowActive)
            {
                notificationSystem.ShowDamageReceived(damage);
            }
        }
    }

    /// <summary>
    /// Handle parry success event
    /// </summary>
    private void HandleParrySuccess()
    {
        Debug.Log("==========================================================");
        Debug.Log("🎉🎉🎉 HANDLEPARRYSUCCESS CALLED! 🎉🎉🎉");
        Debug.Log("==========================================================");
        
        DestroyParryIndicator();
        Debug.Log("Parry successful! Executing counter-attack!");
        if (GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnParry();
        }

        // Award stamina immediately
        if (playerController != null && playerController.Character != null)
        {
            float staminaBefore = playerController.Character.StaminaManager.CurrentStamina;
            playerController.Character.StaminaManager.AddStamina(parryStaminaReward);
            float staminaAfter = playerController.Character.StaminaManager.CurrentStamina;
            
            Debug.Log($"<color=cyan>Parry Reward: +{parryStaminaReward} stamina! ({staminaBefore:F0} → {staminaAfter:F0})</color>");
            
            // Update UI immediately to show stamina reward
            UpdatePlayerStaminaUI();
        }

        // Ejecutar contrataque automático del jugador
        Debug.Log($"<color=yellow>[BattleManager]</color> 🚀 Starting ExecuteCounterAttack coroutine...");
        StartCoroutine(ExecuteCounterAttack());
    }

    private IEnumerator ExecuteCounterAttack()
    {
        // IMPORTANT: Wait for parry animation to play before counter-attack
        // Parry animation needs to be visible for player feedback
        // Default: 0.5s (adjust based on your parry animation length)
        Debug.Log($"<color=cyan>[BattleManager]</color> ⏱️ Waiting for parry animation to complete...");
        yield return new WaitForSeconds(0.5f);

        Debug.Log($"<color=yellow>Counter-attack hitting enemy for {counterAttackDamage} damage!</color>");

        // El jugador ejecuta el contrataque
        if (enemyController != null && enemyController.Character != null && enemyController.Character.IsAlive)
        {
            // Play counter attack animation
            if (playerController?.Character?.Animator != null)
            {
                playerController.Character.Animator.Play(counterAttackAnimationState, 0, 0f);
                Debug.Log($"Playing counter attack animation: {counterAttackAnimationState}");
            }

            // Wait a bit for animation to reach hit frame (adjust timing as needed)
            yield return new WaitForSeconds(0.4f);

            // Deal counter attack damage to enemy
            float enemyHealthBefore = enemyController.Character.CurrentHealth;
            enemyController.Character.TakeDamage(counterAttackDamage);
            float enemyHealthAfter = enemyController.Character.CurrentHealth;
            
            Debug.Log($"<color=red>Enemy took {counterAttackDamage} counter damage! ({enemyHealthBefore:F0} → {enemyHealthAfter:F0})</color>");

            // Actualizar UI
            UpdateHealthUI();

            // Play enemy hit reaction
            if (enemyController.Character.Animator != null)
            {
                enemyController.Character.Animator.Play("Hit");
            }
        }

        // Pequeño delay antes de continuar
        yield return new WaitForSeconds(0.5f);
        
        // Return player to idle
        if (playerController?.Character?.Animator != null)
        {
            playerController.Character.Animator.SetFloat("Speed", 0f);
            Debug.Log("Counter attack complete, returning to idle");
        }
    }

    /// <summary>
    /// Handle parry success with timing information
    /// </summary>
    private void HandleParrySuccessWithTiming(bool wasPerfect)
    {
        Debug.Log($"<color=cyan>[BattleManager]</color> 🛡️ HandleParrySuccessWithTiming({wasPerfect}) called!");
        Debug.Log($"<color=yellow>⚔️ {(wasPerfect ? "PERFECT PARRY!" : "PARRY SUCCESS!")} ⚔️</color>");
        
        // Show notification based on perfect timing
        if (notificationSystem != null)
        {
            Debug.Log($"<color=cyan>[BattleManager]</color> 📢 Displaying parry notification...");
            if (wasPerfect)
            {
                notificationSystem.ShowPerfectParry(parryStaminaReward, counterAttackDamage);
                Debug.Log($"<color=yellow>[BattleManager]</color> ⭐ PERFECT PARRY notification shown! (+{parryStaminaReward} stamina, {counterAttackDamage} counter dmg)");
            }
            else
            {
                notificationSystem.ShowParrySuccess(parryStaminaReward, counterAttackDamage);
                Debug.Log($"<color=lime>[BattleManager]</color> ✅ PARRY SUCCESS notification shown! (+{parryStaminaReward} stamina, {counterAttackDamage} counter dmg)");
            }
        }
        else
        {
            Debug.LogWarning("<color=red>[BattleManager]</color> ⚠️ BattleNotificationSystem is NULL! Cannot show parry notification!");
        }
        
        // Delegate to PlayerBattleController to play parry animation
        Debug.Log($"<color=cyan>[BattleManager]</color> 🎬 Triggering player parry animation...");
        if (playerController != null)
        {
            playerController.PlayParryAnimation(wasPerfect);
            Debug.Log($"<color=lime>[BattleManager]</color> ✅ Player parry animation triggered via PlayerBattleController!");
        }
        else
        {
            Debug.LogError("<color=red>[BattleManager]</color> ❌ PlayerController is NULL! Cannot play parry animation!");
        }

        // Play enemy stagger animation
        if (enemyController?.Character?.Animator != null)
        {
            enemyController.Character.Animator.Play("Staggered");
            Debug.Log("<color=lime>[BattleManager]</color> 🎭 Playing enemy stagger animation");
        }
    }

    /// <summary>
    /// Handle parry fail event
    /// </summary>
    private void HandleParryFail()
    {
        Debug.Log("Parry failed! Player will take damage.");
        // Note: Damage notification will be shown when damage is actually applied
        // via EnemyBattleController's damage application
    }

    /// <summary>
    /// Creates the parry indicator when window opens
    /// </summary>
    private void CreateParryIndicator()
    {
        if (parryIconSpawner == null)
        {
            Debug.LogWarning("<color=yellow>[BattleManager]</color> ParryIconSpawner not assigned! Parry indicator will not show.");
            return;
        }

        Debug.Log("<color=cyan>[BattleManager]</color> Parry window opened! Showing parry indicator");
        parryIconSpawner.Show();
    }

    /// <summary>
    /// Destroys the parry indicator
    /// </summary>
    private void DestroyParryIndicator()
    {
        if (parryIconSpawner != null)
        {
            parryIconSpawner.Hide();
            Debug.Log("<color=cyan>[BattleManager]</color> Parry indicator hidden");
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
        
        if (notificationSystem == null)
            Debug.LogWarning("BattleNotificationSystem not assigned! Notifications will not be shown.");

        if (playerMovement == null)
            Debug.LogWarning("PlayerMovement (MovimientoV2) not assigned! Movement will not be disabled during battle.");

        if (parryIconSpawner == null)
            Debug.LogWarning("ParryIconSpawner not assigned! Parry indicators will not show.");
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