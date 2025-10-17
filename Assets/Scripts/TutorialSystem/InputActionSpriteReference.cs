using System;
using UnityEngine;

/// <summary>
/// Maps an input action to actual Sprite references for different device types.
/// Used by InputIconMapper to display sprite icons in UI Image components.
/// </summary>
[Serializable]
public class InputActionSpriteReference
{
    /// <summary>
    /// The input action this mapping applies to (Move, Run, Attack, etc.)
    /// </summary>
    public InputAction action;
    
    /// <summary>
    /// Sprite for keyboard/mouse input (e.g., Shift key sprite, Mouse Left sprite)
    /// </summary>
    public Sprite keyboardSprite;
    
    /// <summary>
    /// Sprite for Xbox controller input (e.g., Xbox A button, Xbox X button)
    /// </summary>
    public Sprite xboxSprite;
    
    /// <summary>
    /// Sprite for PlayStation controller input (e.g., PS Cross, PS Square)
    /// </summary>
    public Sprite playStationSprite;
}
