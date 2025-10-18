using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Centralized battle input manager - Single source of truth for all battle inputs
/// Handles keyboard, gamepad, and UI button inputs with state-based validation
/// Routes validated inputs to appropriate battle systems via events
/// </summary>
public class BattleInputManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Player Turn Actions - Assign from InputSystem_Actions")]
    [Tooltip("Input action for light attack (e.g., J key, gamepad X)")]
    [SerializeField] private InputActionReference lightAttackInput;
    
    [Tooltip("Input action for heavy attack (e.g., K key, gamepad Y)")]
    [SerializeField] private InputActionReference heavyAttackInput;
    
    [Tooltip("Input action for skill 1 (e.g., U key, gamepad LB)")]
    [SerializeField] private InputActionReference skill1Input;
    
    [Tooltip("Input action for skill 2 (e.g., I key, gamepad RB)")]
    [SerializeField] private InputActionReference skill2Input;
    
    [Tooltip("Input action to end turn (e.g., Enter, gamepad Start)")]
    [SerializeField] private InputActionReference endTurnInput;
    
    [Header("Timing-Critical Actions")]
    [Tooltip("Input action for parry (e.g., Space, gamepad B)")]
    [SerializeField] private InputActionReference parryInput;
    
    [Tooltip("Input action for QTE (e.g., E key, gamepad A)")]
    [SerializeField] private InputActionReference qteInput;
    
    [Header("Settings")]
    [Tooltip("Enable detailed logging of all input events")]
    [SerializeField] private bool debugMode = true;
    
    [Tooltip("Allow input during action animations (for queuing)")]
    [SerializeField] private bool allowInputDuringAnimations = false;
    
    [Tooltip("Show on-screen debug HUD with current input state")]
    [SerializeField] private bool showDebugHUD = false;
    
    [Header("Debug UI (Optional)")]
    [SerializeField] private TMPro.TextMeshProUGUI debugText;
    
    [Header("System References (Optional - for direct routing)")]
    [SerializeField] private ParrySystem parrySystem;
    [SerializeField] private QTEManager qteManager;
    
    #endregion
    
    #region Events
    
    // Player action events
    public event Action OnLightAttackRequested;
    public event Action OnHeavyAttackRequested;
    public event Action OnSkill1Requested;
    public event Action OnSkill2Requested;
    public event Action OnEndTurnRequested;
    
    // Timing action events
    public event Action OnParryPressed;
    public event Action OnQTEPressed;
    
    // State change event (for UI feedback)
    public event Action<BattleInputState> OnInputStateChanged;
    
    #endregion
    
    #region Private State
    
    private BattleInputState currentInputState = BattleInputState.Disabled;
    private bool parryWindowActive = false;
    private bool qteWindowActive = false;
    
    // Debug tracking
    private string lastInputName = "None";
    private float lastInputTime = 0f;
    
    #endregion
    
    #region Properties
    
    public BattleInputState CurrentInputState => currentInputState;
    public bool IsInputEnabled => currentInputState != BattleInputState.Disabled;
    public bool IsParryWindowActive => parryWindowActive;
    public bool IsQTEWindowActive => qteWindowActive;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnEnable()
    {
        SubscribeToInputActions();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromInputActions();
    }
    
    private void Update()
    {
        if (showDebugHUD)
        {
            UpdateDebugHUD();
        }
    }
    
    private void Start()
    {
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> Starting with state: <color=yellow>{currentInputState}</color>");
            Debug.Log($"<color=cyan>[BattleInput]</color> Waiting for battle initialization...");
        }
    }
    
    #endregion
    
    #region Input Action Subscription
    
    private void SubscribeToInputActions()
    {
        // Player turn actions
        if (lightAttackInput != null && lightAttackInput.action != null)
        {
            lightAttackInput.action.performed += OnLightAttackInput;
            lightAttackInput.action.Enable();
        }
        
        if (heavyAttackInput != null && heavyAttackInput.action != null)
        {
            heavyAttackInput.action.performed += OnHeavyAttackInput;
            heavyAttackInput.action.Enable();
        }
        
        if (skill1Input != null && skill1Input.action != null)
        {
            skill1Input.action.performed += OnSkill1Input;
            skill1Input.action.Enable();
        }
        
        if (skill2Input != null && skill2Input.action != null)
        {
            skill2Input.action.performed += OnSkill2Input;
            skill2Input.action.Enable();
        }
        
        if (endTurnInput != null && endTurnInput.action != null)
        {
            endTurnInput.action.performed += OnEndTurnInput;
            endTurnInput.action.Enable();
        }
        
        // Timing actions
        if (parryInput != null && parryInput.action != null)
        {
            parryInput.action.performed += OnParryInput;
            parryInput.action.Enable();
        }
        
        if (qteInput != null && qteInput.action != null)
        {
            qteInput.action.performed += OnQTEInput;
            qteInput.action.Enable();
        }
    }
    
    private void UnsubscribeFromInputActions()
    {
        // Player turn actions
        if (lightAttackInput != null && lightAttackInput.action != null)
        {
            lightAttackInput.action.performed -= OnLightAttackInput;
            lightAttackInput.action.Disable();
        }
        
        if (heavyAttackInput != null && heavyAttackInput.action != null)
        {
            heavyAttackInput.action.performed -= OnHeavyAttackInput;
            heavyAttackInput.action.Disable();
        }
        
        if (skill1Input != null && skill1Input.action != null)
        {
            skill1Input.action.performed -= OnSkill1Input;
            skill1Input.action.Disable();
        }
        
        if (skill2Input != null && skill2Input.action != null)
        {
            skill2Input.action.performed -= OnSkill2Input;
            skill2Input.action.Disable();
        }
        
        if (endTurnInput != null && endTurnInput.action != null)
        {
            endTurnInput.action.performed -= OnEndTurnInput;
            endTurnInput.action.Disable();
        }
        
        // Timing actions
        if (parryInput != null && parryInput.action != null)
        {
            parryInput.action.performed -= OnParryInput;
            parryInput.action.Disable();
        }
        
        if (qteInput != null && qteInput.action != null)
        {
            qteInput.action.performed -= OnQTEInput;
            qteInput.action.Disable();
        }
    }
    
    #endregion
    
    #region Input Callbacks (from InputSystem)
    
    private void OnLightAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessLightAttack();
    }
    
    private void OnHeavyAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessHeavyAttack();
    }
    
    private void OnSkill1Input(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessSkill1();
    }
    
    private void OnSkill2Input(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessSkill2();
    }
    
    private void OnEndTurnInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessEndTurn();
    }
    
    private void OnParryInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessParry();
    }
    
    private void OnQTEInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        ProcessQTE();
    }
    
    #endregion
    
    #region Public Methods (for UI Buttons)
    
    /// <summary>
    /// Trigger light attack from UI button click
    /// </summary>
    public void TriggerLightAttack()
    {
        ProcessLightAttack();
    }
    
    /// <summary>
    /// Trigger heavy attack from UI button click
    /// </summary>
    public void TriggerHeavyAttack()
    {
        ProcessHeavyAttack();
    }
    
    /// <summary>
    /// Trigger skill 1 from UI button click
    /// </summary>
    public void TriggerSkill1()
    {
        ProcessSkill1();
    }
    
    /// <summary>
    /// Trigger skill 2 from UI button click
    /// </summary>
    public void TriggerSkill2()
    {
        ProcessSkill2();
    }
    
    /// <summary>
    /// Trigger end turn from UI button click
    /// </summary>
    public void TriggerEndTurn()
    {
        ProcessEndTurn();
    }
    
    /// <summary>
    /// Trigger parry from UI button click
    /// </summary>
    public void TriggerParry()
    {
        ProcessParry();
    }
    
    /// <summary>
    /// Trigger QTE from UI button click
    /// </summary>
    public void TriggerQTE()
    {
        ProcessQTE();
    }
    
    #endregion
    
    #region Input Processing (Validation + Event Firing)
    
    private void ProcessLightAttack()
    {
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> ProcessLightAttack() called! State: {currentInputState}");
        }
        
        if (!ValidatePlayerAction("Light Attack")) return;
        
        LogInput("Light Attack", true);
        
        if (debugMode)
        {
            Debug.Log($"<color=lime>[BattleInput] LIGHT ATTACK EVENT FIRED!</color>");
        }
        
        OnLightAttackRequested?.Invoke();
    }
    
    private void ProcessHeavyAttack()
    {
        if (!ValidatePlayerAction("Heavy Attack")) return;
        
        LogInput("Heavy Attack", true);
        OnHeavyAttackRequested?.Invoke();
    }
    
    private void ProcessSkill1()
    {
        if (!ValidatePlayerAction("Skill 1")) return;
        
        LogInput("Skill 1", true);
        OnSkill1Requested?.Invoke();
    }
    
    private void ProcessSkill2()
    {
        if (!ValidatePlayerAction("Skill 2")) return;
        
        LogInput("Skill 2", true);
        OnSkill2Requested?.Invoke();
    }
    
    private void ProcessEndTurn()
    {
        if (!ValidatePlayerAction("End Turn")) return;
        
        LogInput("End Turn", true);
        OnEndTurnRequested?.Invoke();
    }
    
    private void ProcessParry()
    {
        // Parry only valid during parry window
        if (!parryWindowActive)
        {
            LogInput("Parry", false, "No parry window active");
            return;
        }
        
        if (currentInputState != BattleInputState.ParryWindow)
        {
            LogInput("Parry", false, $"Wrong state: {currentInputState}");
            return;
        }
        
        LogInput("Parry", true);
        
        // Route to ParrySystem if directly wired, otherwise fire event
        if (parrySystem != null)
        {
            parrySystem.ProcessParryInput();
        }
        
        OnParryPressed?.Invoke();
    }
    
    private void ProcessQTE()
    {
        // QTE only valid during QTE window
        if (!qteWindowActive)
        {
            LogInput("QTE", false, "No QTE window active");
            return;
        }
        
        if (currentInputState != BattleInputState.QTEWindow)
        {
            LogInput("QTE", false, $"Wrong state: {currentInputState}");
            return;
        }
        
        LogInput("QTE", true);
        
        // Route to QTEManager if directly wired, otherwise fire event
        if (qteManager != null)
        {
            qteManager.ProcessQTEInput();
        }
        
        OnQTEPressed?.Invoke();
    }
    
    #endregion
    
    #region Input Validation
    
    /// <summary>
    /// Validate if player actions are allowed right now
    /// </summary>
    private bool ValidatePlayerAction(string actionName)
    {
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> Validating {actionName} | State: {currentInputState}");
        }
        
        // Check if input is globally disabled
        if (currentInputState == BattleInputState.Disabled)
        {
            LogInput(actionName, false, "Input disabled");
            return false;
        }
        
        // Check if it's player turn
        if (currentInputState != BattleInputState.PlayerTurn)
        {
            // Exception: Allow input during animations if enabled
            if (currentInputState == BattleInputState.ExecutingAction && allowInputDuringAnimations)
            {
                LogInput(actionName, true, "Queued during animation");
                return true;
            }
            
            LogInput(actionName, false, $"Wrong state: {currentInputState}");
            return false;
        }
        
        if (debugMode)
        {
            Debug.Log($"<color=lime>[BattleInput] {actionName} VALIDATION PASSED!</color>");
        }
        
        return true;
    }
    
    #endregion
    
    #region State Management
    
    /// <summary>
    /// Set the current input state (called by BattleManager based on battle flow)
    /// </summary>
    public void SetInputState(BattleInputState newState)
    {
        if (currentInputState == newState) 
        {
            if (debugMode)
            {
                Debug.Log($"<color=cyan>[BattleInput]</color> State unchanged: <color=yellow>{newState}</color>");
            }
            return;
        }
        
        BattleInputState previousState = currentInputState;
        currentInputState = newState;
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> State: <color=yellow>{previousState}</color> → <color=lime>{newState}</color>");
            
            // Extra logging para PlayerTurn
            if (newState == BattleInputState.PlayerTurn)
            {
                Debug.Log($"<color=lime>[BattleInput] PLAYER TURN ENABLED! Actions now allowed.</color>");
            }
        }
        
        OnInputStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// Notify that parry window is active (called by ParrySystem)
    /// </summary>
    public void SetParryWindowActive(bool isActive)
    {
        parryWindowActive = isActive;
        
        if (isActive)
        {
            SetInputState(BattleInputState.ParryWindow);
        }
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> Parry Window: {(isActive ? "<color=lime>OPEN</color>" : "<color=red>CLOSED</color>")}");
        }
    }
    
    /// <summary>
    /// Notify that QTE window is active (called by QTEManager)
    /// </summary>
    public void SetQTEWindowActive(bool isActive)
    {
        qteWindowActive = isActive;
        
        if (isActive)
        {
            SetInputState(BattleInputState.QTEWindow);
        }
        
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> QTE Window: {(isActive ? "<color=lime>OPEN</color>" : "<color=red>CLOSED</color>")}");
        }
    }
    
    /// <summary>
    /// Completely disable all input (e.g., during cutscenes)
    /// </summary>
    public void DisableAllInput()
    {
        SetInputState(BattleInputState.Disabled);
        parryWindowActive = false;
        qteWindowActive = false;
    }
    
    /// <summary>
    /// Enable player turn input
    /// </summary>
    public void EnablePlayerTurnInput()
    {
        if (debugMode)
        {
            Debug.Log($"<color=cyan>[BattleInput]</color> EnablePlayerTurnInput() called! Current: {currentInputState} → PlayerTurn");
        }
        
        SetInputState(BattleInputState.PlayerTurn);
    }
    
    #endregion
    
    #region Debug Logging
    
    private void LogInput(string actionName, bool allowed, string reason = "")
    {
        lastInputName = actionName;
        lastInputTime = Time.time;
        
        if (!debugMode) return;
        
        string statusIcon = allowed ? "✅" : "❌";
        string statusColor = allowed ? "lime" : "red";
        string reasonText = string.IsNullOrEmpty(reason) ? "" : $" | <color=orange>{reason}</color>";
        
        Debug.Log($"<color=cyan>[BattleInput]</color> {statusIcon} <b>{actionName}</b> | " +
                  $"State: <color=yellow>{currentInputState}</color> | " +
                  $"Result: <color={statusColor}>{(allowed ? "ALLOWED" : "BLOCKED")}</color>" +
                  $"{reasonText}");
    }
    
    private void UpdateDebugHUD()
    {
        if (debugText == null) return;
        
        float timeSinceLastInput = Time.time - lastInputTime;
        string deviceName = "Unknown";
        
        // Detect current device
        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            deviceName = $"Gamepad ({UnityEngine.InputSystem.Gamepad.current.displayName})";
        }
        else if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            deviceName = "Keyboard + Mouse";
        }
        
        debugText.text = $@"<b>BATTLE INPUT DEBUG</b>
━━━━━━━━━━━━━━━━━━━━━━━━━━━
<b>Input State:</b> {currentInputState}
<b>Parry Window:</b> {(parryWindowActive ? "<color=lime>OPEN ✓</color>" : "<color=red>CLOSED ✗</color>")}
<b>QTE Window:</b> {(qteWindowActive ? "<color=lime>OPEN ✓</color>" : "<color=red>CLOSED ✗</color>")}
<b>Last Input:</b> {lastInputName} ({timeSinceLastInput:F2}s ago)
<b>Device:</b> {deviceName}

<b>Allowed Actions:</b>
{(currentInputState == BattleInputState.PlayerTurn ? "✅" : "❌")} Light/Heavy Attack
{(currentInputState == BattleInputState.PlayerTurn ? "✅" : "❌")} Skills
{(parryWindowActive ? "✅" : "❌")} Parry
{(qteWindowActive ? "✅" : "❌")} QTE";
    }
    
    #endregion
    
    #region Public Query Methods
    
    /// <summary>
    /// Check if player actions are currently allowed
    /// </summary>
    public bool CanUsePlayerActions()
    {
        return currentInputState == BattleInputState.PlayerTurn ||
               (currentInputState == BattleInputState.ExecutingAction && allowInputDuringAnimations);
    }
    
    /// <summary>
    /// Check if parry input is currently valid
    /// </summary>
    public bool CanParry()
    {
        return parryWindowActive && currentInputState == BattleInputState.ParryWindow;
    }
    
    /// <summary>
    /// Check if QTE input is currently valid
    /// </summary>
    public bool CanUseQTE()
    {
        return qteWindowActive && currentInputState == BattleInputState.QTEWindow;
    }
    
    #endregion
}

/// <summary>
/// Enum defining all possible input states during battle
/// </summary>
public enum BattleInputState
{
    Disabled,           // No input allowed (cutscenes, battle end, etc.)
    PlayerTurn,         // Normal player actions allowed (attacks, skills)
    ParryWindow,        // Only parry input allowed (during enemy attack)
    QTEWindow,          // Only QTE input allowed (during player attack)
    ExecutingAction     // Action animation playing (optionally block new inputs)
}
