using UnityEngine;

/// <summary>
/// ScriptableObject that defines a boss attack with damage, animations, and status effects
/// Create instances via: Assets > Create > Battle > Boss Attack Data
/// </summary>
[CreateAssetMenu(fileName = "BossAttackData", menuName = "Battle/Boss Attack Data")]
public class BossAttackData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Display name of this attack")]
    public string attackName = "Boss Attack";
    
    [TextArea(2, 4)]
    [Tooltip("Description shown in UI or debug")]
    public string description = "A powerful boss attack";
    
    [Header("Animation")]
    [Tooltip("Name of the animation state in the boss's Animator")]
    public string animationStateName = "Attack";
    
    [Tooltip("Total duration of the attack animation in seconds")]
    public float attackDuration = 2f;
    
    [Tooltip("When damage should be applied during the animation (after parry window)")]
    public float damageApplicationTime = 1.5f;
    
    [Header("Damage")]
    [Tooltip("Base damage dealt by this attack")]
    public float baseDamage = 20f;
    
    [Header("Status Effect (Optional)")]
    [Tooltip("Does this attack apply a status effect?")]
    public bool appliesStatusEffect = false;
    
    [Tooltip("Type of status effect to apply")]
    public StatusEffectType effectType = StatusEffectType.None;
    
    [Tooltip("How many turns the effect lasts")]
    public int effectDuration = 1;
    
    [Tooltip("Chance (0-1) that the effect will be applied")]
    [Range(0f, 1f)]
    public float effectApplicationChance = 0.5f;
    
    [Header("Visual/Audio (Optional)")]
    [Tooltip("VFX prefab to spawn during attack")]
    public GameObject attackEffectPrefab;
    
    [Tooltip("Sound effect name to play")]
    public string attackSoundName;
    
    [Header("UI")]
    [Tooltip("Icon shown in battle UI")]
    public Sprite attackIcon;
    
    [Tooltip("Color for attack name display")]
    public Color attackColor = Color.red;
    
    /// <summary>
    /// Validate the attack data
    /// </summary>
    private void OnValidate()
    {
        // Ensure damage application happens before attack ends
        if (damageApplicationTime > attackDuration)
        {
            damageApplicationTime = attackDuration * 0.75f;
            Debug.LogWarning($"BossAttackData '{attackName}': Damage application time adjusted to {damageApplicationTime}s");
        }
        
        // Ensure effect type is set if applying effect
        if (appliesStatusEffect && effectType == StatusEffectType.None)
        {
            Debug.LogWarning($"BossAttackData '{attackName}': appliesStatusEffect is true but effectType is None!");
        }
    }
}
