using UnityEngine;

/// <summary>
/// Skill action
/// </summary>
public class SkillAction : BattleAction
{
    private SkillData skillData;
    private Animator animator;
    
    public override float StaminaCost => skillData.staminaCost;
    public override ActionType ActionType => ActionType.Skill;
    
    public SkillAction(BattleCharacter performer, BattleCharacter target, 
                       SkillData skillData, Animator animator) 
        : base(performer, target)
    {
        this.skillData = skillData;
        this.animator = animator;
    }
    
    public override void Execute()
    {
        if (!CanExecute())
        {
            Debug.LogWarning("Cannot execute skill - insufficient stamina");
            CompleteAction();
            return;
        }
        
        // Consume stamina
        performer.StaminaManager.ConsumeStamina(StaminaCost);
        
        // Play animation
        animator.Play(skillData.animationStateName);
        
        // Apply skill effects
        ApplySkillEffects();
        
        // For now, complete immediately
        // TODO: Wait for animation to complete
        CompleteAction();
    }
    
    private void ApplySkillEffects()
    {
        // Apply damage
        if (skillData.damageAmount > 0)
        {
            target.TakeDamage(skillData.damageAmount);
            Debug.Log($"Skill dealt {skillData.damageAmount} damage!");
        }
        
        // Apply healing
        if (skillData.healsPlayer && skillData.healAmount > 0)
        {
            performer.Heal(skillData.healAmount);
            Debug.Log($"Skill healed {skillData.healAmount} HP!");
        }
    }
}

