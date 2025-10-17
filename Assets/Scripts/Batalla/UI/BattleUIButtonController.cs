using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Controls battle UI buttons including action buttons and timing-based buttons (Parry/QTE)
/// Action buttons are visible during player turn
/// Parry/QTE buttons dynamically appear during their timing windows
/// </summary>
public class BattleUIButtonController : MonoBehaviour
{
    [Header("Action Button References")]
    [SerializeField] private Button lightAttackButton;
    [SerializeField] private Button heavyAttackButton;
    [SerializeField] private Button skill1Button;
    [SerializeField] private Button skill2Button;
    
    [Header("Timing Button References (Dynamic)")]
    [SerializeField] private Button parryButton;
    [SerializeField] private Button qteButton;
    
    [Header("Input Actions - Link to InputSystem_Actions")]
    // NOTE: These are now just for icon display purposes
    // Actual input listening is handled by BattleInputManager
    [SerializeField] private InputActionReference lightAttackAction;
    [SerializeField] private InputActionReference heavyAttackAction;
    [SerializeField] private InputActionReference skill1Action;
    [SerializeField] private InputActionReference skill2Action;
    [SerializeField] private InputActionReference parryAction;
    [SerializeField] private InputActionReference qteAction;
    
    [Header("Button Text (TMP with inline sprites)")]
    [SerializeField] private TextMeshProUGUI lightAttackText;
    [SerializeField] private TextMeshProUGUI heavyAttackText;
    [SerializeField] private TextMeshProUGUI skill1Text;
    [SerializeField] private TextMeshProUGUI skill2Text;
    [SerializeField] private TextMeshProUGUI parryText;
    [SerializeField] private TextMeshProUGUI qteText;
    
    [Header("System References")]
    [SerializeField] private BattleInputManager inputManager; // NEW: Route clicks through input manager
    [SerializeField] private ParrySystem parrySystem;
    [SerializeField] private QTEManager qteManager;
    
    [Header("Settings")]
    [SerializeField] private bool showInputIcons = true;
    [SerializeField] private bool debugMode = false;
    
    // Events DEPRECATED - now handled by BattleInputManager
    // Keeping for backward compatibility during transition
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnLightAttackPressed;
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnHeavyAttackPressed;
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnSkill1Pressed;
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnSkill2Pressed;
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnParryPressed;
    [Obsolete("Use BattleInputManager events instead")]
    public event Action OnQTEPressed;
    
    private bool isInputEnabled = false;
    private bool isParryWindowActive = false;
    private bool isQTEWindowActive = false;
    
    private void OnEnable()
    {
        // Subscribe to action button clicks - now route through BattleInputManager
        if (lightAttackButton != null)
            lightAttackButton.onClick.AddListener(() => TriggerLightAttack());
        
        if (heavyAttackButton != null)
            heavyAttackButton.onClick.AddListener(() => TriggerHeavyAttack());
        
        if (skill1Button != null)
            skill1Button.onClick.AddListener(() => TriggerSkill1());
        
        if (skill2Button != null)
            skill2Button.onClick.AddListener(() => TriggerSkill2());
        
        // Subscribe to timing button clicks
        if (parryButton != null)
            parryButton.onClick.AddListener(() => TriggerParry());
        
        if (qteButton != null)
            qteButton.onClick.AddListener(() => TriggerQTE());
        
        // NOTE: Input action listeners removed - now handled by BattleInputManager
        // We only keep InputActionReferences for icon display purposes
        
        // Subscribe to timing window events
        SubscribeToTimingWindows();
        
        // Subscribe to BattleInputManager state changes (NEW)
        if (inputManager != null)
        {
            inputManager.OnInputStateChanged += HandleInputStateChanged;
        }
        
        // Subscribe to device changes for icon updates
        if (showInputIcons && InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged += OnDeviceChanged;
            UpdateButtonIcons();
        }
        
        // Initially hide timing buttons
        HideParryButton();
        HideQTEButton();
    }
    
    private void OnDisable()
    {
        // Unsubscribe from action button clicks
        if (lightAttackButton != null)
            lightAttackButton.onClick.RemoveAllListeners();
        
        if (heavyAttackButton != null)
            heavyAttackButton.onClick.RemoveAllListeners();
        
        if (skill1Button != null)
            skill1Button.onClick.RemoveAllListeners();
        
        if (skill2Button != null)
            skill2Button.onClick.RemoveAllListeners();
        
        // Unsubscribe from timing button clicks
        if (parryButton != null)
            parryButton.onClick.RemoveAllListeners();
        
        if (qteButton != null)
            qteButton.onClick.RemoveAllListeners();
        
        // NOTE: Input action unsubscription removed - handled by BattleInputManager
        
        // Unsubscribe from BattleInputManager
        if (inputManager != null)
        {
            inputManager.OnInputStateChanged -= HandleInputStateChanged;
        }
        
        // Unsubscribe from timing window events
        UnsubscribeFromTimingWindows();
        
        // Unsubscribe from device changes
        if (InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged -= OnDeviceChanged;
        }
    }
    
    /// <summary>
    /// Enable input listening for player's turn
    /// </summary>
    public void EnableInput()
    {
        isInputEnabled = true;
        
        // Enable buttons
        SetButtonInteractable(lightAttackButton, true);
        SetButtonInteractable(heavyAttackButton, true);
        SetButtonInteractable(skill1Button, true);
        SetButtonInteractable(skill2Button, true);
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Input ENABLED");
        }
    }
    
    /// <summary>
    /// Disable input during enemy turn or animations
    /// </summary>
    public void DisableInput()
    {
        isInputEnabled = false;
        
        // Disable buttons
        SetButtonInteractable(lightAttackButton, false);
        SetButtonInteractable(heavyAttackButton, false);
        SetButtonInteractable(skill1Button, false);
        SetButtonInteractable(skill2Button, false);
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Input DISABLED");
        }
    }
    
    /// <summary>
    /// Show/hide all buttons
    /// </summary>
    public void SetButtonsVisible(bool visible)
    {
        SetButtonVisible(lightAttackButton, visible);
        SetButtonVisible(heavyAttackButton, visible);
        SetButtonVisible(skill1Button, visible);
        SetButtonVisible(skill2Button, visible);
    }
    
    /// <summary>
    /// Handle input state changes from BattleInputManager (NEW)
    /// </summary>
    private void HandleInputStateChanged(BattleInputState newState)
    {
        switch (newState)
        {
            case BattleInputState.PlayerTurn:
                EnableInput();
                break;
                
            case BattleInputState.Disabled:
            case BattleInputState.ExecutingAction:
                DisableInput();
                break;
                
            case BattleInputState.ParryWindow:
            case BattleInputState.QTEWindow:
                // Timing windows have their own button visibility
                DisableInput();
                break;
        }
        
        if (debugMode)
        {
            Debug.Log($"[BattleUIButtons] Input state changed to: {newState}");
        }
    }
    
    #region Input Action Handlers (REMOVED - Now handled by BattleInputManager)
    
    // These methods have been removed as input is now centralized in BattleInputManager
    // InputActionReferences are kept only for icon display purposes
    
    #endregion
    
    #region Timing Window Subscriptions
    
    /// <summary>
    /// Subscribe to ParrySystem and QTEManager timing window events
    /// </summary>
    private void SubscribeToTimingWindows()
    {
        if (parrySystem != null)
        {
            parrySystem.OnParryWindowActive += HandleParryWindowChanged;
        }
        
        if (qteManager != null)
        {
            qteManager.OnQTEWindowStart += HandleQTEWindowChanged;
        }
    }
    
    /// <summary>
    /// Unsubscribe from timing window events
    /// </summary>
    private void UnsubscribeFromTimingWindows()
    {
        if (parrySystem != null)
        {
            parrySystem.OnParryWindowActive -= HandleParryWindowChanged;
        }
        
        if (qteManager != null)
        {
            qteManager.OnQTEWindowStart -= HandleQTEWindowChanged;
        }
    }
    
    /// <summary>
    /// Handle parry window state changes
    /// </summary>
    private void HandleParryWindowChanged(bool isActive)
    {
        isParryWindowActive = isActive;
        
        if (isActive)
        {
            ShowParryButton();
        }
        else
        {
            HideParryButton();
        }
        
        if (debugMode)
        {
            Debug.Log($"[BattleUIButtons] Parry window {(isActive ? "OPENED" : "CLOSED")}");
        }
    }
    
    /// <summary>
    /// Handle QTE window state changes
    /// </summary>
    private void HandleQTEWindowChanged(bool isActive)
    {
        isQTEWindowActive = isActive;
        
        if (isActive)
        {
            ShowQTEButton();
        }
        else
        {
            HideQTEButton();
        }
        
        if (debugMode)
        {
            Debug.Log($"[BattleUIButtons] QTE window {(isActive ? "OPENED" : "CLOSED")}");
        }
    }
    
    #endregion
    
    #region Action Triggers
    
    private void TriggerLightAttack()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Light Attack button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerLightAttack();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnLightAttackPressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    private void TriggerHeavyAttack()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Heavy Attack button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerHeavyAttack();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnHeavyAttackPressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    private void TriggerSkill1()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Skill 1 button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerSkill1();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnSkill1Pressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    private void TriggerSkill2()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Skill 2 button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerSkill2();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnSkill2Pressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    private void TriggerParry()
    {
        if (!isParryWindowActive) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Parry button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerParry();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnParryPressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    private void TriggerQTE()
    {
        if (!isQTEWindowActive) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] QTE button clicked");
        }
        
        // Route through BattleInputManager (NEW)
        if (inputManager != null)
        {
            inputManager.TriggerQTE();
        }
        
        // Legacy event for backward compatibility (will be removed)
        #pragma warning disable CS0618 // Type or member is obsolete
        OnQTEPressed?.Invoke();
        #pragma warning restore CS0618
    }
    
    #endregion
    
    #region Visual Feedback
    
    /// <summary>
    /// Animate button press when triggered by controller/keyboard
    /// Simulates the button being pressed and released
    /// </summary>
    private void AnimateButtonPress(Button button)
    {
        if (button == null) return;
        
        // Start coroutine to animate press and release
        StartCoroutine(AnimateButtonPressCoroutine(button));
    }
    
    /// <summary>
    /// Coroutine that handles button press animation
    /// </summary>
    private System.Collections.IEnumerator AnimateButtonPressCoroutine(Button button)
    {
        if (button == null) yield break;
        
        // Simulate press down
        button.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        
        // Optional: Force selection to trigger highlighted state
        button.Select();
        
        // Wait for visual feedback duration
        yield return new WaitForSeconds(0.15f);
        
        // Simulate release
        button.OnPointerUp(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current));
        
        if (debugMode)
        {
            Debug.Log($"[BattleUIButtons] Button {button.name} animated");
        }
    }
    
    /// <summary>
    /// Update button text with input icons based on current device (Spanish labels)
    /// </summary>
    private void UpdateButtonIcons()
    {
        if (!showInputIcons || InputIconMapper.Instance == null) return;
        
        // Update action buttons
        if (lightAttackText != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.LightAttack);
            lightAttackText.text = $"Ataque Ligero {iconText}";
        }
        
        if (heavyAttackText != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.HeavyAttack);
            heavyAttackText.text = $"Ataque Pesado {iconText}";
        }
        
        if (skill1Text != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Skill1);
            skill1Text.text = $"Habilidad 1 {iconText}";
        }
        
        if (skill2Text != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Skill2);
            skill2Text.text = $"Habilidad 2 {iconText}";
        }
        
        // Update timing buttons
        if (parryText != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Parry);
            parryText.text = $"¡PARRY! {iconText}";
        }
        
        if (qteText != null)
        {
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.QTE);
            qteText.text = $"¡PRESIONA! {iconText}";
        }
    }
    
    private void OnDeviceChanged(InputDeviceType newDevice)
    {
        UpdateButtonIcons();
    }
    
    #endregion
    
    #region Timing Button Visibility
    
    /// <summary>
    /// Show parry button when window opens
    /// </summary>
    private void ShowParryButton()
    {
        if (parryButton != null)
        {
            parryButton.gameObject.SetActive(true);
            parryButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Hide parry button when window closes
    /// </summary>
    private void HideParryButton()
    {
        if (parryButton != null)
        {
            parryButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show QTE button when window opens
    /// </summary>
    private void ShowQTEButton()
    {
        if (qteButton != null)
        {
            qteButton.gameObject.SetActive(true);
            qteButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Hide QTE button when window closes
    /// </summary>
    private void HideQTEButton()
    {
        if (qteButton != null)
        {
            qteButton.gameObject.SetActive(false);
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }
    
    private void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }
    
    #endregion
}
