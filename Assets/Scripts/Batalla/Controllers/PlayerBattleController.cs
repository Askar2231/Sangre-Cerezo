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
    
    private BattleAction currentAction;
    
    private void Awake()
    {
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<BattleCharacter>();
        }
    }
    
    public void Initialize(AnimationSequencer animSequencer, QTEManager qte)
    {
        this.animationSequencer = animSequencer;
        this.qteManager = qte;
        
        animationSequencer.Initialize(playerCharacter.Animator, qteManager);
    }
    
    /// <summary>
    /// Execute a light attack
    /// </summary>
    public void ExecuteLightAttack(BattleCharacter target)
    {
        if (lightAttackData == null)
        {
            Debug.LogError("Light attack data not assigned!");
            return;
        }
        
        currentAction = new AttackAction(
            playerCharacter,
            target,
            lightAttackData,
            animationSequencer,
            qteManager
        );
        
        currentAction.OnActionComplete += HandleActionComplete;
        OnActionStarted?.Invoke(ActionType.LightAttack);
        currentAction.Execute();
    }
    
    /// <summary>
    /// Execute a skill
    /// </summary>
    public void ExecuteSkill(int skillIndex, BattleCharacter target)
    {
        if (skillIndex < 0 || skillIndex >= availableSkills.Length)
        {
            Debug.LogError($"Invalid skill index: {skillIndex}");
            return;
        }
        
        SkillData skillData = availableSkills[skillIndex];
        
        currentAction = new SkillAction(
            playerCharacter,
            target,
            skillData,
            playerCharacter.Animator
        );
        
        currentAction.OnActionComplete += HandleActionComplete;
        OnActionStarted?.Invoke(ActionType.Skill);
        currentAction.Execute();
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
                    if (playerCharacter.StaminaManager.HasEnoughStamina(skill.staminaCost))
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
                if (skillIndex >= 0 && skillIndex < availableSkills.Length)
                    return availableSkills[skillIndex].staminaCost;
                return 0f;
            
            default:
                return 0f;
        }
    }
    
    private void HandleActionComplete()
    {
        if (currentAction != null)
        {
            currentAction.OnActionComplete -= HandleActionComplete;
            currentAction = null;
        }
        
        OnActionComplete?.Invoke();
    }
    
    /// <summary>
    /// Reset for new battle
    /// </summary>
    public void ResetForBattle()
    {
        playerCharacter.ResetForBattle();
        currentAction = null;
    }
}

