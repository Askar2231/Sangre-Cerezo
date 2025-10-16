using UnityEngine;
using TMPro;

/// <summary>
/// Example script showing how to use InputIconMapper with sprite name mappings
/// </summary>
public class InputIconMapperExample : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Example Settings")]
    [SerializeField] private bool showDynamicExample = true;
    [SerializeField] private InputAction exampleAction = InputAction.Run;

    private void Start()
    {
        // Example 1: Simple text replacement with placeholders
        SimpleTextReplacement();

        // Example 2: Get specific sprite name
        GetSpecificSpriteName();

        // Example 3: Dynamic text based on device
        if (showDynamicExample)
        {
            ShowDynamicExample();
        }

        // Example 4: Listen to device changes
        SubscribeToDeviceChanges();
    }

    /// <summary>
    /// Example 1: Replace placeholders in text with sprites
    /// </summary>
    private void SimpleTextReplacement()
    {
        string rawText = "Press {Move} to move and {Run} to sprint!";
        string processedText = InputIconMapper.Instance.ProcessTextPlaceholders(rawText);

        Debug.Log($"Raw: {rawText}");
        Debug.Log($"Processed: {processedText}");

        // If you have a TextMeshProUGUI component, assign it
        if (tutorialText != null)
        {
            tutorialText.text = processedText;
        }
    }

    /// <summary>
    /// Example 2: Get sprite name for specific action
    /// </summary>
    private void GetSpecificSpriteName()
    {
        string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(exampleAction);

        Debug.Log($"Sprite name for {exampleAction}: {spriteName}");

        // Use it in TMP manually
        string tmpTag = $"<sprite name=\"{spriteName}\">";
        Debug.Log($"TMP Tag: {tmpTag}");
    }

    /// <summary>
    /// Example 3: Show different text based on current device
    /// </summary>
    private void ShowDynamicExample()
    {
        InputDeviceType currentDevice = InputIconMapper.Instance.GetCurrentDeviceType();

        switch (currentDevice)
        {
            case InputDeviceType.Keyboard:
                Debug.Log("Keyboard detected! Showing keyboard icons.");
                break;
            case InputDeviceType.XboxController:
                Debug.Log("Xbox controller detected! Showing Xbox icons.");
                break;
            case InputDeviceType.PlayStationController:
                Debug.Log("PlayStation controller detected! Showing PlayStation icons.");
                break;
            case InputDeviceType.GenericGamepad:
                Debug.Log("Generic gamepad detected! Using Xbox icons as fallback.");
                break;
        }
    }

    /// <summary>
    /// Example 4: React to device changes
    /// </summary>
    private void SubscribeToDeviceChanges()
    {
        InputIconMapper.Instance.OnDeviceChanged += OnInputDeviceChanged;
    }

    private void OnInputDeviceChanged(InputDeviceType newDevice)
    {
        Debug.Log($"Input device changed to: {newDevice}");

        // Update your UI when device changes
        if (tutorialText != null)
        {
            string text = "Press {Run} to sprint!";
            tutorialText.text = InputIconMapper.Instance.ProcessTextPlaceholders(text);
        }
    }

    /// <summary>
    /// Example 5: Build custom text with multiple actions
    /// </summary>
    public string BuildCustomTutorialText()
    {
        // Method 1: Using placeholders (recommended)
        string text = "Press {Move} to move, {Run} to sprint, and {Interact} to interact with objects.";
        return InputIconMapper.Instance.ProcessTextPlaceholders(text);

        // Method 2: Manual construction
        /*
        string moveSprite = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Move);
        string runSprite = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Run);
        string interactSprite = InputIconMapper.Instance.GetSpriteNameForAction(InputAction.Interact);
        
        return $"Press <sprite name=\"{moveSprite}\"> to move, " +
               $"<sprite name=\"{runSprite}\"> to sprint, and " +
               $"<sprite name=\"{interactSprite}\"> to interact with objects.";
        */
    }

    /// <summary>
    /// Example 6: Get QTE button specifically
    /// </summary>
    public void ShowQTEPrompt()
    {
        string qteIcon = InputIconMapper.Instance.GetIconForQTE();

        if (tutorialText != null)
        {
            tutorialText.text = $"Quick! Press {qteIcon} now!";
        }
    }

    /// <summary>
    /// Example 7: Check if a sprite name is available
    /// </summary>
    public bool HasSpriteForAction(InputAction action)
    {
        string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(action);
        return !string.IsNullOrEmpty(spriteName);
    }

    /// <summary>
    /// Example 8: Get text fallback if sprite is not available
    /// </summary>
    public string GetActionDisplayText(InputAction action)
    {
        string spriteName = InputIconMapper.Instance.GetSpriteNameForAction(action);

        if (!string.IsNullOrEmpty(spriteName))
        {
            return $"<sprite name=\"{spriteName}\">";
        }
        else
        {
            // Fallback to text
            return InputIconMapper.Instance.GetTextForAction(action);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged -= OnInputDeviceChanged;
        }
    }
}
