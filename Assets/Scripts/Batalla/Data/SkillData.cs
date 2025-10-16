using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ScriptableObject that stores skill data
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "Battle/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    
    [TextArea(2, 4)]
    public string description;
    
    [Header("Input Binding")]
    [Tooltip("Input action for this skill (drag from Input Actions asset)")]
    public InputActionReference inputAction;
    
    [Header("Animation")]
    public string animationStateName;
    
    [Header("Stats")]
    public float staminaCost = 30f;
    public float damageAmount = 15f;
    
    [Header("Effects")]
    [Tooltip("Additional effects can be added here")]
    public bool healsPlayer = false;
    public float healAmount = 0f;
}

