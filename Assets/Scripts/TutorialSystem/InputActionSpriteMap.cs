using System;

/// <summary>
/// Maps an input action to its corresponding sprite names for different device types.
/// Used by InputIconMapper to display device-specific input icons.
/// </summary>
[Serializable]
public class InputActionSpriteMap
{
    /// <summary>
    /// The input action this mapping applies to (Move, Run, Attack, etc.)
    /// </summary>
    public InputAction action;
    
    /// <summary>
    /// Sprite name for keyboard/mouse input (e.g., "Shift", "WASD", "Mouse_Left")
    /// </summary>
    public string keyboardSpriteName = "";
    
    /// <summary>
    /// Sprite name for Xbox controller input (e.g., "Xbox_A", "Xbox_R3", "Xbox_LT")
    /// </summary>
    public string xboxSpriteName = "";
    
    /// <summary>
    /// Sprite name for PlayStation controller input (e.g., "PS_Cross", "PS_R3", "PS_L2")
    /// </summary>
    public string playStationSpriteName = "";
}
