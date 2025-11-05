using UnityEngine;
using System;

/// <summary>
/// Base class for all status effects that can be applied to battle characters
/// </summary>
public abstract class StatusEffect
{
    // Properties
    public StatusEffectType Type { get; protected set; }
    public StatusEffectDuration DurationType { get; protected set; }
    public int TurnsRemaining { get; set; }
    public int ActionsRemaining { get; set; }
    public float TimeRemaining { get; set; }
    
    public string DisplayName { get; protected set; }
    public string Description { get; protected set; }
    public Sprite Icon { get; protected set; }
    
    // Track when effect was applied
    public float ApplicationTime { get; private set; }
    
    protected StatusEffect()
    {
        ApplicationTime = Time.time;
    }
    
    /// <summary>
    /// Called when the effect is first applied to a character
    /// </summary>
    /// <param name="target">The character receiving the effect</param>
    public abstract void OnApply(BattleCharacter target);
    
    /// <summary>
    /// Called when the effect is removed from a character
    /// </summary>
    /// <param name="target">The character losing the effect</param>
    public abstract void OnRemove(BattleCharacter target);
    
    /// <summary>
    /// Called each turn or update (optional override)
    /// </summary>
    /// <param name="target">The affected character</param>
    public virtual void OnUpdate(BattleCharacter target) { }
    
    /// <summary>
    /// Check if the effect should expire based on its duration type
    /// </summary>
    /// <returns>True if effect should be removed</returns>
    public virtual bool ShouldExpire()
    {
        switch (DurationType)
        {
            case StatusEffectDuration.OneAction:
                return ActionsRemaining <= 0;
                
            case StatusEffectDuration.OneTurn:
            case StatusEffectDuration.TwoTurns:
            case StatusEffectDuration.ThreeTurns:
                return TurnsRemaining <= 0;
                
            case StatusEffectDuration.Custom:
                return TimeRemaining <= 0;
                
            case StatusEffectDuration.Permanent:
                return false;
                
            default:
                return true;
        }
    }
    
    /// <summary>
    /// Decrement action counter (called when affected character takes an action)
    /// </summary>
    public void OnActionTaken()
    {
        if (DurationType == StatusEffectDuration.OneAction)
        {
            ActionsRemaining--;
        }
    }
    
    /// <summary>
    /// Decrement turn counter (called at end of turn)
    /// </summary>
    public void OnTurnEnd()
    {
        switch (DurationType)
        {
            case StatusEffectDuration.OneTurn:
            case StatusEffectDuration.TwoTurns:
            case StatusEffectDuration.ThreeTurns:
                TurnsRemaining--;
                break;
        }
    }
    
    /// <summary>
    /// Update time-based effects
    /// </summary>
    /// <param name="deltaTime">Time since last update</param>
    public void UpdateTime(float deltaTime)
    {
        if (DurationType == StatusEffectDuration.Custom)
        {
            TimeRemaining -= deltaTime;
        }
    }
}
