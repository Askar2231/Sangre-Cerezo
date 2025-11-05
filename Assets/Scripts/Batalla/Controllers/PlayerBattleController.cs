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
    [SerializeField] private AttackAnimationData heavyAttackData;

    [Header("Skills")]
    [SerializeField] private SkillData[] availableSkills;

    [Header("Parry Animations")]
    [SerializeField] private string parryAnimationName = "Parry";
    [SerializeField] private string perfectParryAnimationName = "ParryPerfect";

    // Components
    private AnimationSequencer animationSequencer;
    private QTEManager qteManager;
    private BattleNotificationSystem notificationSystem;

    // Current action being executed
    private BattleAction currentAction;
    
    // Attack cancellation (for boss parry)
    private bool isAttackCancelled = false;

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

    public void Initialize(AnimationSequencer animSequencer, QTEManager qte, BattleNotificationSystem notifSystem = null)
    {
        animationSequencer = animSequencer;
        qteManager = qte;
        notificationSystem = notifSystem;

        if (animationSequencer == null)
        {
            Debug.LogWarning("AnimationSequencer is null - attacks will execute without animation tracking");
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

        // Create and execute AttackAction
        if (animationSequencer != null && qteManager != null && lightAttackData != null)
        {
            Debug.Log("Executing attack through AttackAction system");

            // Create attack action
            currentAction = new AttackAction(
                playerCharacter,
                target,
                lightAttackData,
                animationSequencer,
                qteManager,
                notificationSystem
            );

            // Subscribe to completion
            currentAction.OnActionComplete += HandleActionComplete;

            // Execute the action
            currentAction.Execute();
        }
        else
        {
            // Fallback: Execute immediately if systems not available
            Debug.LogWarning("AttackAction system not available - executing attack immediately");
            ExecuteAttackDamageFallback(target);
            OnActionComplete?.Invoke();

            // Haptic feedback
            if (GamepadVibrationManager.Instance != null)
            {
                GamepadVibrationManager.Instance.VibrateOnLightAttack();
            }
        }
    }

    /// <summary>
    /// Execute heavy attack against target
    /// Heavy attacks deal more damage but may require more stamina/have longer animations
    /// </summary>
    public void ExecuteHeavyAttack(BattleCharacter target)
    {
        if (!CanPerformAction(ActionType.HeavyAttack))
        {
            Debug.LogWarning("Cannot perform heavy attack - insufficient stamina or conditions not met");
            return;
        }

        OnActionStarted?.Invoke(ActionType.HeavyAttack);

        // Create and execute AttackAction with heavy attack data
        if (animationSequencer != null && qteManager != null && heavyAttackData != null)
        {
            Debug.Log("Executing heavy attack through AttackAction system");

            // Create attack action
            currentAction = new AttackAction(
                playerCharacter,
                target,
                heavyAttackData,
                animationSequencer,
                qteManager,
                notificationSystem
            );

            // Subscribe to completion
            currentAction.OnActionComplete += HandleActionComplete;

            // Execute the action
            currentAction.Execute();
        }
        else
        {
            // Fallback: Execute immediately if systems not available
            Debug.LogWarning("AttackAction system not available for heavy attack - executing immediately");
            ExecuteAttackDamageFallback(target, isHeavy: true);
            OnActionComplete?.Invoke();

            // Haptic feedback (stronger for heavy attack)
            if (GamepadVibrationManager.Instance != null)
            {
                GamepadVibrationManager.Instance.VibrateOnHeavyAttack();
            }
        }
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

        // Create and execute SkillAction
        if (playerCharacter?.Animator != null && skill != null)
        {
            Debug.Log($"Executing skill through SkillAction system: {skill.skillName}");

            // Create skill action
            currentAction = new SkillAction(
                playerCharacter,
                target,
                skill,
                playerCharacter.Animator,
                notificationSystem
            );

            // Subscribe to completion
            currentAction.OnActionComplete += HandleActionComplete;

            // Execute the action
            currentAction.Execute();
        }
        else
        {
            // Fallback
            Debug.LogWarning("SkillAction system not available - executing skill immediately");
            ExecuteSkillDamageFallback(skill, target);
            OnActionComplete?.Invoke();
        }

        // Haptic feedback
        if (GamepadVibrationManager.Instance != null)
        {
            if (skill.damageAmount >= 30f)
            {
                GamepadVibrationManager.Instance.VibrateOnHeavyAttack();
            }
            else if (skill.healsPlayer)
            {
                GamepadVibrationManager.Instance.VibrateOnItemUse();
            }
            else
            {
                GamepadVibrationManager.Instance.VibrateOnLightAttack();
            }
        }
    }

    /// <summary>
    /// Handle action completion callback
    /// </summary>
    private void HandleActionComplete()
    {
        // Check if attack was cancelled (boss parried)
        if (isAttackCancelled)
        {
            Debug.Log("⚠️ Attack was cancelled - no damage dealt (boss parried)");
            isAttackCancelled = false; // Reset flag
            
            // Still complete the action but damage was already prevented
            // (Boss parry system handled everything)
        }
        
        // Unsubscribe from the action
        if (currentAction != null)
        {
            currentAction.OnActionComplete -= HandleActionComplete;
            currentAction = null;
        }

        // Notify battle manager
        OnActionComplete?.Invoke();

        // Haptic feedback
        if (GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnLightAttack();
        }

        Debug.Log("Player action completed");
    }

    /// <summary>
    /// Fallback attack damage (used when AttackAction system unavailable)
    /// </summary>
    private void ExecuteAttackDamageFallback(BattleCharacter target, bool isHeavy = false)
    {
        AttackAnimationData attackData = isHeavy ? heavyAttackData : lightAttackData;

        if (attackData == null)
        {
            Debug.LogError($"{(isHeavy ? "Heavy" : "Light")} attack data is null!");
            return;
        }

        // Consume stamina
        playerCharacter.StaminaManager.ConsumeStamina(attackData.staminaCost);

        // Calculate and apply damage (no QTE bonus in fallback)
        float damage = attackData.baseDamage;
        target.TakeDamage(damage);

        Debug.Log($"Player deals {damage} damage to {target.name} (fallback mode)");
    }

    /// <summary>
    /// Fallback skill damage (used when SkillAction system unavailable)
    /// </summary>
    private void ExecuteSkillDamageFallback(SkillData skill, BattleCharacter target)
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

            case ActionType.HeavyAttack:
                return heavyAttackData != null &&
                       playerCharacter.StaminaManager.HasEnoughStamina(heavyAttackData.staminaCost);

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

            case ActionType.HeavyAttack:
                return heavyAttackData != null ? heavyAttackData.staminaCost : 0f;

            case ActionType.Skill:
                if (skillIndex >= 0 && skillIndex < availableSkills.Length && availableSkills[skillIndex] != null)
                    return availableSkills[skillIndex].staminaCost;
                return 0f;

            default:
                return 0f;
        }
    }

    /// <summary>
    /// Get base damage for an action
    /// </summary>
    public float GetActionDamage(ActionType actionType, int skillIndex = 0)
    {
        switch (actionType)
        {
            case ActionType.LightAttack:
                return lightAttackData != null ? lightAttackData.baseDamage : 0f;

            case ActionType.HeavyAttack:
                return heavyAttackData != null ? heavyAttackData.baseDamage : 0f;

            case ActionType.Skill:
                if (skillIndex >= 0 && skillIndex < availableSkills.Length && availableSkills[skillIndex] != null)
                    return availableSkills[skillIndex].damageAmount;
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
    /// Set whether current attack should be cancelled (called when boss parries)
    /// </summary>
    public void SetAttackCancelled(bool cancelled)
    {
        isAttackCancelled = cancelled;
        
        if (cancelled)
        {
            Debug.Log("⚠️ <color=yellow>Player attack has been cancelled by boss!</color>");
        }
    }
    
    /// <summary>
    /// Check if current attack is cancelled (for use in AttackAction)
    /// </summary>
    public bool IsAttackCancelled => isAttackCancelled;

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

    /// <summary>
    /// Play parry animation based on timing quality
    /// 
    /// ANIMATOR CONTROLLER SETUP (same as Light/Heavy Attack):
    /// 1. Create states "Parry" and "ParryPerfect" in Animator Controller
    /// 2. Assign animation clips to these states
    /// 3. NO transitions FROM Idle TO Parry (we use Animator.Play() to force state)
    /// 4. Only add transitions FROM Parry/ParryPerfect BACK TO Idle
    /// 5. Set "Has Exit Time" on the return transition to match animation length
    /// 
    /// This matches the pattern used for attack animations (see AnimationSequencer.cs line 66)
    /// </summary>
    [ContextMenu("TEST: Play Normal Parry Animation")]
    public void TestPlayNormalParry()
    {
        PlayParryAnimation(false);
    }

    [ContextMenu("TEST: Play Perfect Parry Animation")]
    public void TestPlayPerfectParry()
    {
        PlayParryAnimation(true);
    }

    public void PlayParryAnimation(bool wasPerfect)
    {
        Debug.Log($"<color=cyan>[PlayerBattleController]</color> 🛡️ PlayParryAnimation() called! wasPerfect={wasPerfect}");
        Debug.Log($"<color=magenta>[PlayerBattleController]</color> 🔧 THIS IS THE MODIFIED PlayerBattleController.cs!");
        Debug.Log($"<color=magenta>[PlayerBattleController]</color> 🔧 parryAnimationName='{parryAnimationName}', perfectParryAnimationName='{perfectParryAnimationName}'");
        
        if (playerCharacter?.Animator == null)
        {
            Debug.LogWarning("<color=red>[PlayerBattleController]</color> ❌ Player animator is null, cannot play parry animation!");
            Debug.LogWarning($"playerCharacter is {(playerCharacter == null ? "NULL" : "NOT NULL")}");
            if (playerCharacter != null)
            {
                Debug.LogWarning($"playerCharacter.Animator is {(playerCharacter.Animator == null ? "NULL" : "NOT NULL")}");
                Debug.LogWarning($"<color=red>🔍 SOLUTION: Make sure your Player GameObject has a BattleCharacter component with an Animator reference assigned!</color>");
            }
            return;
        }

        string animationToPlay = wasPerfect ? perfectParryAnimationName : parryAnimationName;
        
        Debug.Log($"<color=lime>[PlayerBattleController]</color> ✅ Playing player parry animation: '{animationToPlay}'");
        Debug.Log($"<color=yellow>🎬 Animator.Play(\"{animationToPlay}\", 0, 0f) - Layer 0, start from beginning!</color>");
        
        // Verify the animation state exists
        var animatorController = playerCharacter.Animator.runtimeAnimatorController;
        if (animatorController != null)
        {
            Debug.Log($"<color=cyan>🎭 Animator Controller: {animatorController.name}</color>");
            Debug.Log($"<color=cyan>🔍 Make sure '{animationToPlay}' state exists in the Animator Controller!</color>");
            
            // Check current animator state
            var currentState = playerCharacter.Animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"<color=yellow>📊 Current Animator State: {currentState.fullPathHash} (normalized time: {currentState.normalizedTime})</color>");
        }
        else
        {
            Debug.LogWarning($"<color=red>❌ No AnimatorController assigned to the Animator!</color>");
        }
        
        // Force play the animation on layer 0, starting from time 0
        // This immediately transitions to the parry animation regardless of current state
        // Same pattern as AnimationSequencer.cs for attack animations
        playerCharacter.Animator.Play(animationToPlay, 0, 0f);
        
        Debug.Log($"<color=lime>[PlayerBattleController]</color> 🎭 Parry animation '{animationToPlay}' triggered successfully!");
        Debug.Log($"<color=cyan>💡 ANIMATOR SETUP (same as attack animations):</color>");
        Debug.Log($"<color=cyan>  1. State '{animationToPlay}' must exist in Animator Controller</color>");
        Debug.Log($"<color=cyan>  2. State has animation clip assigned</color>");
        Debug.Log($"<color=cyan>  3. NO transition FROM Idle TO {animationToPlay} (we use .Play() to force it)</color>");
        Debug.Log($"<color=cyan>  4. Only transition FROM {animationToPlay} BACK TO Idle (with 'Has Exit Time')</color>");
        Debug.Log($"<color=cyan>  5. This matches Light Attack / Heavy Attack setup pattern</color>");
    }

    /// <summary>
    /// Execute counter-attack after successful parry
    /// </summary>
    public void ExecuteCounterAttackOnEnemy(BattleCharacter target)
    {
        if (lightAttackData == null)
        {
            Debug.LogError("Cannot execute counter-attack - light attack data is null!");
            return;
        }

        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("Cannot execute counter-attack - target is null or dead!");
            return;
        }

        Debug.Log("Executing counter-attack!");

        // Play attack animation
        if (playerCharacter?.Animator != null)
        {
            playerCharacter.Animator.Play(lightAttackData.animationStateName);
            Debug.Log($"Playing counter-attack animation: {lightAttackData.animationStateName}");
        }

        // Apply counter-attack damage (can be boosted or use normal damage)
        float damage = lightAttackData.baseDamage * 1.5f; // 50% bonus damage on counter
        target.TakeDamage(damage);

        Debug.Log($"Counter-attack deals {damage} damage to {target.name}!");

        // Haptic feedback
        if (GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnHeavyAttack(); // Use heavy attack vibration for counter
        }
    }
}

