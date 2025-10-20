using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;

/// <summary>
/// Represents a single battle action button with dynamic input sprite display
/// </summary>
[RequireComponent(typeof(Button))]
public class BattleActionButton : MonoBehaviour
{
    [Header("UI Components (Auto-found if not assigned)")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI inputHintText;
    [SerializeField] private Image inputIconImage;
    
    [Header("Display Settings")]
    [SerializeField] private bool showInputIcon = true;
    [SerializeField] private bool showInputText = true;
    [SerializeField] private string inputTextFormat = "Press {0}"; // {0} = input sprite/text
    
    private InputActionReference actionReference;
    private Action onClickCallback;
    private string actionName;
    private string actionDescription;
    
    private void Awake()
    {
        // Auto-find components if not assigned
        if (button == null)
            button = GetComponent<Button>();
            
        if (nameText == null)
            nameText = transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            
        if (descriptionText == null)
            descriptionText = transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            
        if (inputHintText == null)
            inputHintText = transform.Find("InputHintText")?.GetComponent<TextMeshProUGUI>();
            
        if (inputIconImage == null)
            inputIconImage = transform.Find("InputIcon")?.GetComponent<Image>();
    }
    
    /// <summary>
    /// Initialize the button with action data
    /// </summary>
    public void Initialize(string name, string description, InputActionReference input, Action onClick)
    {
        actionName = name;
        actionDescription = description;
        actionReference = input;
        onClickCallback = onClick;
        
        // Add click listener
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }
        
        // Update display
        UpdateDisplay();
    }
    
    /// <summary>
    /// Update all visual elements
    /// </summary>
    public void UpdateDisplay()
    {
        UpdateNameText();
        UpdateDescriptionText();
        UpdateInputDisplay();
    }
    
    /// <summary>
    /// Update only the input display (useful when device changes)
    /// </summary>
    public void UpdateInputDisplay()
    {
        if (!showInputIcon && !showInputText)
            return;
        
        if (actionReference == null || actionReference.action == null)
        {
            // No input assigned - hide input elements
            if (inputHintText != null)
                inputHintText.gameObject.SetActive(false);
            if (inputIconImage != null)
                inputIconImage.gameObject.SetActive(false);
            return;
        }
        
        // Note: inputIconImage is not used with new TMP sprite system
        // Icons are displayed inline within TextMeshPro text
        if (inputIconImage != null)
        {
            inputIconImage.gameObject.SetActive(false);
        }
        
        if (showInputText && inputHintText != null)
        {
            // Get display text (sprite tag or text)
            string displayText = InputActionHelper.GetInputDisplayText(actionReference);
            
            // Format it
            string formattedText = string.Format(inputTextFormat, displayText);
            inputHintText.text = formattedText;
            inputHintText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Update name text
    /// </summary>
    private void UpdateNameText()
    {
        if (nameText != null)
        {
            nameText.text = actionName;
        }
    }
    
    /// <summary>
    /// Update description text
    /// </summary>
    private void UpdateDescriptionText()
    {
        if (descriptionText != null)
        {
            descriptionText.text = actionDescription;
            
            // Hide if no description
            if (string.IsNullOrEmpty(actionDescription))
            {
                descriptionText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Handle button click
    /// </summary>
    private void OnButtonClick()
    {
        onClickCallback?.Invoke();
    }
    
    /// <summary>
    /// Enable/disable the button
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }
    
    /// <summary>
    /// Check if this button's input was pressed this frame
    /// </summary>
    public bool WasPressedThisFrame()
    {
        return InputActionHelper.WasPressedThisFrame(actionReference);
    }
    
    /// <summary>
    /// Get the action name
    /// </summary>
    public string ActionName => actionName;
    
    /// <summary>
    /// Get the input action reference
    /// </summary>
    public InputActionReference InputAction => actionReference;
}
