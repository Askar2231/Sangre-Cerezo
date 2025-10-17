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
    [SerializeField] private ParrySystem parrySystem;
    [SerializeField] private QTEManager qteManager;
    
    [Header("Settings")]
    [SerializeField] private bool showInputIcons = true;
    [SerializeField] private bool debugMode = false;
    
    // Events that the BattleManager can subscribe to
    public event Action OnLightAttackPressed;
    public event Action OnHeavyAttackPressed;
    public event Action OnSkill1Pressed;
    public event Action OnSkill2Pressed;
    public event Action OnParryPressed;
    public event Action OnQTEPressed;
    
    private bool isInputEnabled = false;
    private bool isParryWindowActive = false;
    private bool isQTEWindowActive = false;
    
    private void OnEnable()
    {
        // Subscribe to action button clicks
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
        
        // Subscribe to Input System actions
        EnableInputActions();
        
        // Subscribe to timing window events
        SubscribeToTimingWindows();
        
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
        
        // Unsubscribe from Input System actions
        DisableInputActions();
        
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
    
    #region Input Action Handlers
    
    private void EnableInputActions()
    {
        if (lightAttackAction != null && lightAttackAction.action != null)
        {
            lightAttackAction.action.performed += OnLightAttackInput;
            lightAttackAction.action.Enable();
        }
        
        if (heavyAttackAction != null && heavyAttackAction.action != null)
        {
            heavyAttackAction.action.performed += OnHeavyAttackInput;
            heavyAttackAction.action.Enable();
        }
        
        if (skill1Action != null && skill1Action.action != null)
        {
            skill1Action.action.performed += OnSkill1Input;
            skill1Action.action.Enable();
        }
        
        if (skill2Action != null && skill2Action.action != null)
        {
            skill2Action.action.performed += OnSkill2Input;
            skill2Action.action.Enable();
        }
        
        if (parryAction != null && parryAction.action != null)
        {
            parryAction.action.performed += OnParryInput;
            parryAction.action.Enable();
        }
        
        if (qteAction != null && qteAction.action != null)
        {
            qteAction.action.performed += OnQTEInput;
            qteAction.action.Enable();
        }
    }
    
    private void DisableInputActions()
    {
        if (lightAttackAction != null && lightAttackAction.action != null)
        {
            lightAttackAction.action.performed -= OnLightAttackInput;
            lightAttackAction.action.Disable();
        }
        
        if (heavyAttackAction != null && heavyAttackAction.action != null)
        {
            heavyAttackAction.action.performed -= OnHeavyAttackInput;
            heavyAttackAction.action.Disable();
        }
        
        if (skill1Action != null && skill1Action.action != null)
        {
            skill1Action.action.performed -= OnSkill1Input;
            skill1Action.action.Disable();
        }
        
        if (skill2Action != null && skill2Action.action != null)
        {
            skill2Action.action.performed -= OnSkill2Input;
            skill2Action.action.Disable();
        }
        
        if (parryAction != null && parryAction.action != null)
        {
            parryAction.action.performed -= OnParryInput;
            parryAction.action.Disable();
        }
        
        if (qteAction != null && qteAction.action != null)
        {
            qteAction.action.performed -= OnQTEInput;
            qteAction.action.Disable();
        }
    }
    
    // Input System callbacks - trigger the same actions as button clicks
    private void OnLightAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isInputEnabled)
        {
            TriggerLightAttack();
            AnimateButtonPress(lightAttackButton);
        }
    }
    
    private void OnHeavyAttackInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isInputEnabled)
        {
            TriggerHeavyAttack();
            AnimateButtonPress(heavyAttackButton);
        }
    }
    
    private void OnSkill1Input(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isInputEnabled)
        {
            TriggerSkill1();
            AnimateButtonPress(skill1Button);
        }
    }
    
    private void OnSkill2Input(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isInputEnabled)
        {
            TriggerSkill2();
            AnimateButtonPress(skill2Button);
        }
    }
    
    private void OnParryInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isParryWindowActive)
        {
            TriggerParry();
            AnimateButtonPress(parryButton);
        }
    }
    
    private void OnQTEInput(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (isQTEWindowActive)
        {
            TriggerQTE();
            AnimateButtonPress(qteButton);
        }
    }
    
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
            Debug.Log("[BattleUIButtons] Light Attack triggered");
        }
        
        OnLightAttackPressed?.Invoke();
    }
    
    private void TriggerHeavyAttack()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Heavy Attack triggered");
        }
        
        OnHeavyAttackPressed?.Invoke();
    }
    
    private void TriggerSkill1()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Skill 1 triggered");
        }
        
        OnSkill1Pressed?.Invoke();
    }
    
    private void TriggerSkill2()
    {
        if (!isInputEnabled) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Skill 2 triggered");
        }
        
        OnSkill2Pressed?.Invoke();
    }
    
    private void TriggerParry()
    {
        if (!isParryWindowActive) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] Parry triggered");
        }
        
        OnParryPressed?.Invoke();
    }
    
    private void TriggerQTE()
    {
        if (!isQTEWindowActive) return;
        
        if (debugMode)
        {
            Debug.Log("[BattleUIButtons] QTE triggered");
        }
        
        OnQTEPressed?.Invoke();
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
