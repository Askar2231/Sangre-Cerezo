using UnityEngine;
using System;

/// <summary>
/// Handles player-specific battle logic
/// </summary>
public class PlayerBattleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleCharacter playerCharacter;
    
    [Header("Attack Data")]
    [SerializeField] private AttackAnimationData lightAttackData;
    
    [Header("Skills")]
    [SerializeField] private SkillData[] availableSkills;
    
    // Components
    private AnimationSequencer animationSequencer;
    private QTEManager qteManager;
    
    public BattleCharacter Character => playerCharacter;
    
    // Events
    public event Action OnActionComplete;
    public event Action<ActionType> OnActionStarted;
    
    private void Awake()
    {
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<BattleCharacter>();
        }
    }
    
    public void Initialize(AnimationSequencer animSequencer, QTEManager qte)
    {
        animationSequencer = animSequencer; // Puede ser null temporalmente
        qteManager = qte;
        
        if (animationSequencer == null)
        {
            Debug.LogWarning("AnimationSequencer is null - using temporary delay system");
        }
        
        Debug.Log("PlayerBattleController initialized");
    }
    
    /// <summary>
    /// Execute light attack against target
    /// </summary>
    public void ExecuteLightAttack(BattleCharacter target)
    {
        if (!CanPerformAction(ActionType.LightAttack))
        {
            Debug.LogWarning("Cannot perform light attack - insufficient stamina or conditions not met");
            return;
        }
        
        OnActionStarted?.Invoke(ActionType.LightAttack);
        
        // Si no hay AnimationSequencer, ejecutar inmediatamente
        if (animationSequencer == null)
        {
            Debug.Log("No AnimationSequencer - executing attack immediately");
            ExecuteAttackDamage(target);
            OnActionComplete?.Invoke();
            return;
        }
        
        // Con AnimationSequencer (código original cuando esté disponible)
        // animationSequencer.PlayAttackSequence(
        //     lightAttackData,
        //     () => ExecuteAttackDamage(target),
        //     () => OnActionComplete?.Invoke()
        // );
        
        // Por ahora, ejecutar inmediatamente también
        ExecuteAttackDamage(target);
        OnActionComplete?.Invoke();
    }

    /// <summary>
    /// Execute skill against target
    /// </summary>
    public void ExecuteSkill(int skillIndex, BattleCharacter target)
    {
        if (skillIndex < 0 || skillIndex >= availableSkills.Length || availableSkills[skillIndex] == null)
        {
            Debug.LogError($"Invalid skill index: {skillIndex}");
            return;
        }
        
        if (!CanPerformAction(ActionType.Skill))
        {
            Debug.LogWarning("Cannot perform skill - insufficient stamina or conditions not met");
            return;
        }
        
        var skill = availableSkills[skillIndex];
        
        OnActionStarted?.Invoke(ActionType.Skill);
        
        // Si no hay AnimationSequencer, ejecutar inmediatamente
        if (animationSequencer == null)
        {
            Debug.Log($"No AnimationSequencer - executing skill {skill.skillName} immediately");
            ExecuteSkillDamage(skill, target);
            OnActionComplete?.Invoke();
            return;
        }
        
        // Con AnimationSequencer (código original cuando esté disponible)
        // animationSequencer.PlaySkillSequence(
        //     skill,
        //     () => ExecuteSkillDamage(skill, target),
        //     () => OnActionComplete?.Invoke()
        // );
        
        // Por ahora, ejecutar inmediatamente también
        ExecuteSkillDamage(skill, target);
        OnActionComplete?.Invoke();
    }

    /// <summary>
    /// Execute the actual attack damage
    /// </summary>
    private void ExecuteAttackDamage(BattleCharacter target)
    {
        if (lightAttackData == null)
        {
            Debug.LogError("Light attack data is null!");
            return;
        }
        
        // Consume stamina
        playerCharacter.StaminaManager.ConsumeStamina(lightAttackData.staminaCost);
        
        // Calculate and apply damage
        float damage = lightAttackData.baseDamage;
        target.TakeDamage(damage);
        
        Debug.Log($"Player deals {damage} damage to {target.name}");
    }

    /// <summary>
    /// Execute the actual skill damage
    /// </summary>
    private void ExecuteSkillDamage(SkillData skill, BattleCharacter target)
    {
        if (skill == null)
        {
            Debug.LogError("Skill data is null!");
            return;
        }
        
        // Consume stamina
        playerCharacter.StaminaManager.ConsumeStamina(skill.staminaCost);
        
        // Calculate and apply damage - usar 'damageAmount' en lugar de 'baseDamage'
        float damage = skill.damageAmount; // <- CAMBIO AQUÍ
        target.TakeDamage(damage);
        
        // Si la habilidad cura al jugador, aplicar curación
        if (skill.healsPlayer && skill.healAmount > 0f)
        {
            playerCharacter.Heal(skill.healAmount);
            Debug.Log($"Player heals for {skill.healAmount} HP");
        }
        
        Debug.Log($"Player uses {skill.skillName} dealing {damage} damage to {target.name}");
    }
    
    /// <summary>
    /// Check if player can perform an action
    /// </summary>
    public bool CanPerformAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.LightAttack:
                return lightAttackData != null && 
                       playerCharacter.StaminaManager.HasEnoughStamina(lightAttackData.staminaCost);
            
            case ActionType.Skill:
                // Check if any skill is available
                foreach (var skill in availableSkills)
                {
                    if (skill != null && playerCharacter.StaminaManager.HasEnoughStamina(skill.staminaCost))
                        return true;
                }
                return false;
            
            default:
                return false;
        }
    }

    /// <summary>
    /// Get stamina cost for an action
    /// </summary>
    public float GetActionStaminaCost(ActionType actionType, int skillIndex = 0)
    {
        switch (actionType)
        {
            case ActionType.LightAttack:
                return lightAttackData != null ? lightAttackData.staminaCost : 0f;

            case ActionType.Skill:
                if (skillIndex >= 0 && skillIndex < availableSkills.Length && availableSkills[skillIndex] != null)
                    return availableSkills[skillIndex].staminaCost; // Ya está correcto
                return 0f;

            default:
                return 0f;
        }
    }

    
    /// <summary>
    /// Get available skills for UI generation
    /// </summary>
    public SkillData[] GetAvailableSkills()
    {
        return availableSkills ?? new SkillData[0];
    }

    /// <summary>
    /// Check if player has light attack available
    /// </summary>
    public bool HasLightAttack()
    {
        return lightAttackData != null;
    }
    
    /// <summary>
    /// Reset for new battle
    /// </summary>
    public void ResetForBattle()
    {
        playerCharacter.ResetForBattle();
    }
}

