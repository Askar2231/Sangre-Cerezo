using UnityEngine;
using System.Collections;

/// <summary>
/// Attack action with QTE sequences
/// </summary>
public class AttackAction : BattleAction
{
    private AttackAnimationData attackData;
    private AnimationSequencer animationSequencer;
    private QTEManager qteManager;
    
    public override float StaminaCost => attackData.staminaCost;
    public override ActionType ActionType => ActionType.LightAttack;
    
    private int successfulQTEs = 0;
    
    public AttackAction(BattleCharacter performer, BattleCharacter target, 
                        AttackAnimationData attackData,
                        AnimationSequencer animationSequencer,
                        QTEManager qteManager) 
        : base(performer, target)
    {
        this.attackData = attackData;
        this.animationSequencer = animationSequencer;
        this.qteManager = qteManager;
    }
    
    public override void Execute()
    {
        if (!CanExecute())
        {
            Debug.LogWarning("Cannot execute attack - insufficient stamina");
            CompleteAction();
            return;
        }
        
        // Consume stamina
        performer.StaminaManager.ConsumeStamina(StaminaCost);
        
        // Reset QTE counter
        successfulQTEs = 0;
        
        // Setup QTE callbacks
        qteManager.OnQTESuccess += OnQTESuccess;
        qteManager.OnQTEFail += OnQTEFail;
        
        // Play animation sequence with QTE timings
        animationSequencer.PlayAnimationSequence(
            attackData,
            OnAnimationComplete,
            OnHitFrame
        );
    }
    
    private void OnQTESuccess()
    {
        successfulQTEs++;
        Debug.Log($"QTE Success! Total: {successfulQTEs}");
    }
    
    private void OnQTEFail()
    {
        Debug.Log("QTE Failed!");
    }
    
    private void OnHitFrame()
    {
        // Calculate damage based on successful QTEs
        float totalDamage = attackData.baseDamage;
        
        for (int i = 0; i < successfulQTEs; i++)
        {
            totalDamage *= attackData.qteSuccessMultiplier;
        }
        
        // Apply damage to target
        target.TakeDamage(totalDamage);
        
        Debug.Log($"Attack hit for {totalDamage} damage! (Base: {attackData.baseDamage}, QTEs: {successfulQTEs})");
    }
    
    private void OnAnimationComplete()
    {
        // Cleanup
        qteManager.OnQTESuccess -= OnQTESuccess;
        qteManager.OnQTEFail -= OnQTEFail;
        
        CompleteAction();
    }
}

