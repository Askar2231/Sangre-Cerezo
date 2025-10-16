using UnityEngine;
using System.Collections;

/// <summary>
/// Skill action
/// </summary>
public class SkillAction : BattleAction
{
    private SkillData skillData;
    private Animator animator;
    private MonoBehaviour coroutineRunner; // For running coroutines
    
    public override float StaminaCost => skillData.staminaCost;
    public override ActionType ActionType => ActionType.Skill;
    
    public SkillAction(BattleCharacter performer, BattleCharacter target, 
                       SkillData skillData, Animator animator) 
        : base(performer, target)
    {
        this.skillData = skillData;
        this.animator = animator;
        this.coroutineRunner = performer.GetComponent<MonoBehaviour>();
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
        
        // Apply skill effects immediately
        ApplySkillEffects();
        
        // Wait for animation to complete
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(WaitForAnimationComplete());
        }
        else
        {
            CompleteAction();
        }
    }
    
    private IEnumerator WaitForAnimationComplete()
    {
        // Wait one frame for animation to start
        yield return null;
        
        // Wait for animation to complete
        AnimatorStateInfo stateInfo;
        do
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        while (stateInfo.IsName(skillData.animationStateName) && stateInfo.normalizedTime < 1f);
        
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

