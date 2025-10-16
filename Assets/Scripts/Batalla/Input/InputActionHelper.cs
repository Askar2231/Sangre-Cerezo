using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Helper to get input sprites and text from InputActionReference
/// Works with InputIconMapper to get device-specific icons
/// </summary>
public static class InputActionHelper
{
    /// <summary>
    /// Get the sprite for an InputActionReference based on current device
    /// Returns TextMeshPro sprite tag or text fallback
    /// </summary>
    public static string GetInputDisplayText(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null)
        {
            return "[?]";
        }
        
        // Get the action name
        string actionName = actionRef.action.name;
        
        // Map to InputIconMapper's InputAction enum
        InputAction mappedAction = MapToInputAction(actionName);
        
        // Get the sprite/text from InputIconMapper
        if (InputIconMapper.Instance != null)
        {
            return InputIconMapper.Instance.ProcessTextPlaceholders($"{{{mappedAction}}}");
        }
        
        // Fallback: Use binding display string
        return actionRef.action.GetBindingDisplayString();
    }
    
    /// <summary>
    /// Get just the sprite tag (for use in TextMeshPro text)
    /// </summary>
    public static string GetInputSpriteTag(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null)
        {
            return "<sprite index=0>"; // Default sprite
        }
        
        string actionName = actionRef.action.name;
        InputAction mappedAction = MapToInputAction(actionName);
        
        if (InputIconMapper.Instance != null)
        {
            Sprite sprite = InputIconMapper.Instance.GetIconForAction(mappedAction);
            if (sprite != null)
            {
                return $"<sprite name=\"{sprite.name}\">";
            }
        }
        
        return actionRef.action.GetBindingDisplayString();
    }
    
    /// <summary>
    /// Get the Sprite object for an InputActionReference
    /// </summary>
    public static Sprite GetInputSprite(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null)
        {
            return null;
        }
        
        string actionName = actionRef.action.name;
        InputAction mappedAction = MapToInputAction(actionName);
        
        if (InputIconMapper.Instance != null)
        {
            return InputIconMapper.Instance.GetIconForAction(mappedAction);
        }
        
        return null;
    }
    
    /// <summary>
    /// Map InputActionReference name to InputIconMapper's InputAction enum
    /// Add more mappings as needed
    /// </summary>
    private static InputAction MapToInputAction(string actionName)
    {
        switch (actionName.ToLower())
        {
            case "lightattack":
            case "attack":
                return InputAction.LightAttack;
                
            case "heavyattack":
                return InputAction.HeavyAttack;
                
            case "skill1":
            case "skill":
                return InputAction.Skill1;
                
            case "skill2":
                return InputAction.Skill2;
                
            case "endturn":
                return InputAction.EndTurn;
                
            case "cancel":
                return InputAction.Cancel;
                
            case "confirm":
                return InputAction.Confirm;
                
            case "parry":
                return InputAction.Parry;
                
            case "qte":
                return InputAction.QTE;
                
            default:
                Debug.LogWarning($"Unknown action name: {actionName}, defaulting to LightAttack");
                return InputAction.LightAttack;
        }
    }
    
    /// <summary>
    /// Check if the InputActionReference is currently pressed
    /// </summary>
    public static bool IsPressed(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null)
            return false;
            
        return actionRef.action.IsPressed();
    }
    
    /// <summary>
    /// Check if the InputActionReference was pressed this frame
    /// </summary>
    public static bool WasPressedThisFrame(InputActionReference actionRef)
    {
        if (actionRef == null || actionRef.action == null)
            return false;
            
        return actionRef.action.WasPressedThisFrame();
    }
    
    /// <summary>
    /// Enable an InputActionReference
    /// </summary>
    public static void Enable(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Enable();
        }
    }
    
    /// <summary>
    /// Disable an InputActionReference
    /// </summary>
    public static void Disable(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null)
        {
            actionRef.action.Disable();
        }
    }
}
