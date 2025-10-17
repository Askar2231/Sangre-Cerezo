using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// DEPRECATED: This class has been replaced by BattleInputManager
/// Handles battle input and triggers appropriate battle actions
/// Reads direct input from keyboard/gamepad instead of relying on UI
/// 
/// MIGRATION NOTE:
/// - Use BattleInputManager instead for centralized input handling
/// - BattleInputManager supports InputActionReferences (better Inspector integration)
/// - BattleInputManager handles parry/QTE input coordination
/// - BattleInputManager provides state-based input validation
/// </summary>
[System.Obsolete("This class is deprecated. Use BattleInputManager for centralized input handling.")]
public class BattleInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset battleInputActions;
    
    [Header("Battle References")]
    [SerializeField] private PlayerBattleController playerController;
    [SerializeField] private BattleManagerV2 battleManager;
    
    [Header("Attack Data - Configure in Inspector")]
    [SerializeField] private AttackAnimationData lightAttackData;
    [SerializeField] private AttackAnimationData heavyAttackData;
    
    [Header("Skill Data - Configure in Inspector")]
    [SerializeField] private SkillData skill1Data;
    [SerializeField] private SkillData skill2Data;
    
    [Header("Settings")]
    [SerializeField] private bool debugMode = false;
    
    private InputActionMap battleMap;
    private BattleCharacter targetEnemy;
    private bool isInputEnabled = false;
    
    // Events for UI to respond to input
    public event Action<ActionType> OnActionRequested;
    public event Action<bool> OnInputEnabledChanged; // bool = enabled state
    
    private void Awake()
    {
        if (battleInputActions != null)
        {
            battleMap = battleInputActions.FindActionMap("Battle");
            if (battleMap == null)
            {
                Debug.LogError("Battle action map not found in BattleInputActions!");
            }
        }
        else
        {
            Debug.LogError("BattleInputActions not assigned to BattleInputHandler!");
        }
    }
    
    private void OnEnable()
    {
        if (battleMap != null)
        {
            // Subscribe to input events
            var lightAttack = battleMap.FindAction("LightAttack");
            var heavyAttack = battleMap.FindAction("HeavyAttack");
            var skill1 = battleMap.FindAction("Skill1");
            var skill2 = battleMap.FindAction("Skill2");
            var endTurn = battleMap.FindAction("EndTurn");
            var cancel = battleMap.FindAction("Cancel");
            
            if (lightAttack != null) lightAttack.performed += OnLightAttack;
            if (heavyAttack != null) heavyAttack.performed += OnHeavyAttack;
            if (skill1 != null) skill1.performed += OnSkill1;
            if (skill2 != null) skill2.performed += OnSkill2;
            if (endTurn != null) endTurn.performed += OnEndTurn;
            if (cancel != null) cancel.performed += OnCancel;
        }
    }
    
    private void OnDisable()
    {
        if (battleMap != null)
        {
            // Unsubscribe from input events
            var lightAttack = battleMap.FindAction("LightAttack");
            var heavyAttack = battleMap.FindAction("HeavyAttack");
            var skill1 = battleMap.FindAction("Skill1");
            var skill2 = battleMap.FindAction("Skill2");
            var endTurn = battleMap.FindAction("EndTurn");
            var cancel = battleMap.FindAction("Cancel");
            
            if (lightAttack != null) lightAttack.performed -= OnLightAttack;
            if (heavyAttack != null) heavyAttack.performed -= OnHeavyAttack;
            if (skill1 != null) skill1.performed -= OnSkill1;
            if (skill2 != null) skill2.performed -= OnSkill2;
            if (endTurn != null) endTurn.performed -= OnEndTurn;
            if (cancel != null) cancel.performed -= OnCancel;
        }
    }
    
    /// <summary>
    /// Enable input for player's turn
    /// </summary>
    public void EnableInput(BattleCharacter target)
    {
        isInputEnabled = true;
        targetEnemy = target;
        battleMap?.Enable();
        
        OnInputEnabledChanged?.Invoke(true);
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Input ENABLED - Player turn started");
        }
    }
    
    /// <summary>
    /// Disable input (during enemy turn or animations)
    /// </summary>
    public void DisableInput()
    {
        isInputEnabled = false;
        targetEnemy = null;
        battleMap?.Disable();
        
        OnInputEnabledChanged?.Invoke(false);
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Input DISABLED");
        }
    }
    
    /// <summary>
    /// Check if input is currently enabled
    /// </summary>
    public bool IsInputEnabled => isInputEnabled;
    
    #region Input Callbacks
    
    private void OnLightAttack(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!ValidateInput("Light Attack")) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Light Attack pressed");
        }
        
        OnActionRequested?.Invoke(ActionType.LightAttack);
        
        // Trigger light attack on player controller
        if (playerController != null && targetEnemy != null)
        {
            playerController.ExecuteLightAttack(targetEnemy);
        }
    }
    
    private void OnHeavyAttack(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!ValidateInput("Heavy Attack")) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Heavy Attack pressed");
        }
        
        OnActionRequested?.Invoke(ActionType.HeavyAttack);
        
        // Trigger heavy attack on player controller
        if (playerController != null && targetEnemy != null)
        {
            // Check if heavy attack is implemented
            if (playerController.HasMethod("ExecuteHeavyAttack"))
            {
                playerController.ExecuteHeavyAttack(targetEnemy);
            }
            else
            {
                Debug.LogWarning("[BattleInput] ExecuteHeavyAttack not implemented yet!");
            }
        }
    }
    
    private void OnSkill1(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!ValidateInput("Skill 1")) return;
        
        if (skill1Data == null)
        {
            Debug.LogWarning("[BattleInput] Skill 1 data not assigned!");
            return;
        }
        
        if (debugMode)
        {
            Debug.Log($"[BattleInput] Skill 1 pressed: {skill1Data.skillName}");
        }
        
        OnActionRequested?.Invoke(ActionType.Skill);
        
        // Trigger skill on player controller
        if (playerController != null && targetEnemy != null)
        {
            playerController.ExecuteSkill(0, targetEnemy);
        }
    }
    
    private void OnSkill2(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!ValidateInput("Skill 2")) return;
        
        if (skill2Data == null)
        {
            Debug.LogWarning("[BattleInput] Skill 2 data not assigned!");
            return;
        }
        
        if (debugMode)
        {
            Debug.Log($"[BattleInput] Skill 2 pressed: {skill2Data.skillName}");
        }
        
        OnActionRequested?.Invoke(ActionType.Skill);
        
        // Trigger skill on player controller
        if (playerController != null && targetEnemy != null)
        {
            playerController.ExecuteSkill(1, targetEnemy);
        }
    }
    
    private void OnEndTurn(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] End Turn pressed");
        }
        
        OnActionRequested?.Invoke(ActionType.EndTurn);
        
        // Signal battle manager to end turn
        if (battleManager != null)
        {
            // BattleManager should have a public method to end player turn
            // For now, disable input - manager will handle turn end
            DisableInput();
        }
    }
    
    private void OnCancel(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Cancel pressed");
        }
        
        // Could be used to cancel action selection, open menu, etc.
        // For now, just log
    }
    
    #endregion
    
    /// <summary>
    /// Validate that input can be processed
    /// </summary>
    private bool ValidateInput(string actionName)
    {
        if (!isInputEnabled)
        {
            if (debugMode)
            {
                Debug.Log($"[BattleInput] {actionName} ignored - input disabled");
            }
            return false;
        }
        
        if (targetEnemy == null)
        {
            Debug.LogWarning($"[BattleInput] {actionName} ignored - no target enemy!");
            return false;
        }
        
        if (playerController == null)
        {
            Debug.LogError($"[BattleInput] {actionName} ignored - no player controller!");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Set references programmatically
    /// </summary>
    public void Initialize(PlayerBattleController player, BattleManagerV2 manager)
    {
        playerController = player;
        battleManager = manager;
        
        if (debugMode)
        {
            Debug.Log("[BattleInput] Initialized with player and battle manager");
        }
    }
}

/// <summary>
/// Extension method to check if method exists (for compatibility)
/// </summary>
public static class BattleInputExtensions
{
    public static bool HasMethod(this PlayerBattleController controller, string methodName)
    {
        var method = controller.GetType().GetMethod(methodName);
        return method != null;
    }
}
