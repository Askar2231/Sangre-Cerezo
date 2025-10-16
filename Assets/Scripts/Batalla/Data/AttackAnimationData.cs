using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that stores animation data with normalized QTE timings
/// </summary>
[CreateAssetMenu(fileName = "AttackAnimationData", menuName = "Battle/Attack Animation Data")]
public class AttackAnimationData : ScriptableObject
{
    [Header("UI Display")]
    [Tooltip("Display name for this attack")]
    public string displayName = "Attack";
    
    [Tooltip("Description shown in UI")]
    [TextArea(2, 4)]
    public string description = "Basic attack";
    
    [Header("Input Binding")]
    [Tooltip("Input action for this attack (drag from Input Actions asset)")]
    public InputActionReference inputAction;
    
    [Header("Animation")]
    public string animationStateName; // Name of the animation state in Animator
    public float animationDuration; // For reference only
    
    [Header("QTE Timings (Normalized 0-1)")]
    [Tooltip("Normalized time points (0-1) when QTE windows should appear")]
    public List<float> qteNormalizedTimings = new List<float>();
    
    [Header("Damage")]
    [Tooltip("Base damage for this attack")]
    public float baseDamage = 10f;
    
    [Tooltip("Damage multiplier per successful QTE")]
    public float qteSuccessMultiplier = 1.5f;
    
    [Header("Hit Timing")]
    [Tooltip("Normalized time when damage should be applied")]
    public float hitNormalizedTime = 0.75f;
    
    [Header("Stamina")]
    public float staminaCost = 20f;
}

