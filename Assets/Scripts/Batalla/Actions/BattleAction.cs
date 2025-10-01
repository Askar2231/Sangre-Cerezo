using UnityEngine;
using System;

/// <summary>
/// Base class for all battle actions
/// </summary>
public abstract class BattleAction
{
    public abstract float StaminaCost { get; }
    public abstract ActionType ActionType { get; }
    
    protected BattleCharacter performer;
    protected BattleCharacter target;
    
    public event Action OnActionComplete;
    
    public BattleAction(BattleCharacter performer, BattleCharacter target)
    {
        this.performer = performer;
        this.target = target;
    }
    
    /// <summary>
    /// Check if the action can be executed
    /// </summary>
    public virtual bool CanExecute()
    {
        return performer.StaminaManager.HasEnoughStamina(StaminaCost);
    }
    
    /// <summary>
    /// Execute the action
    /// </summary>
    public abstract void Execute();
    
    /// <summary>
    /// Called when action completes
    /// </summary>
    protected void CompleteAction()
    {
        OnActionComplete?.Invoke();
    }
}

