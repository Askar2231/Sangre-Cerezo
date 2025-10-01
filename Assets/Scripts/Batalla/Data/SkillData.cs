using UnityEngine;

/// <summary>
/// ScriptableObject that stores skill data
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "Battle/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public string description;
    
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

