using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages status effects on a battle character
/// Attach this component to any BattleCharacter that can have status effects
/// </summary>
public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> activeEffects = new List<StatusEffect>();
    private BattleCharacter character;
    
    // Events
    public event Action<StatusEffect> OnEffectApplied;
    public event Action<StatusEffect> OnEffectRemoved;
    public event Action<StatusEffect> OnEffectExpired;
    
    // Properties
    public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects.AsReadOnly();
    public int EffectCount => activeEffects.Count;
    
    /// <summary>
    /// Initialize the manager with a reference to the character
    /// </summary>
    public void Initialize(BattleCharacter target)
    {
        character = target;
        activeEffects.Clear();
    }
    
    /// <summary>
    /// Apply a status effect to the character
    /// </summary>
    /// <param name="effect">The effect to apply</param>
    public void ApplyEffect(StatusEffect effect)
    {
        if (effect == null)
        {
            Debug.LogWarning("Attempted to apply null status effect");
            return;
        }
        
        // Check if effect of same type already exists
        StatusEffect existingEffect = activeEffects.FirstOrDefault(e => e.Type == effect.Type);
        
        if (existingEffect != null)
        {
            // Refresh duration instead of stacking
            Debug.Log($"Refreshing existing {effect.Type} effect on {character.name}");
            RefreshEffect(existingEffect, effect);
            return;
        }
        
        // Add new effect
        activeEffects.Add(effect);
        effect.OnApply(character);
        
        Debug.Log($"<color=purple>[StatusEffect]</color> Applied {effect.DisplayName} to {character.name}");
        OnEffectApplied?.Invoke(effect);
    }
    
    /// <summary>
    /// Remove a specific status effect
    /// </summary>
    /// <param name="type">Type of effect to remove</param>
    public void RemoveEffect(StatusEffectType type)
    {
        StatusEffect effect = activeEffects.FirstOrDefault(e => e.Type == type);
        
        if (effect != null)
        {
            RemoveEffect(effect);
        }
    }
    
    /// <summary>
    /// Remove a specific status effect instance
    /// </summary>
    private void RemoveEffect(StatusEffect effect)
    {
        if (effect == null) return;
        
        activeEffects.Remove(effect);
        effect.OnRemove(character);
        
        Debug.Log($"<color=purple>[StatusEffect]</color> Removed {effect.DisplayName} from {character.name}");
        OnEffectRemoved?.Invoke(effect);
    }
    
    /// <summary>
    /// Check if character has a specific status effect
    /// </summary>
    public bool HasEffect(StatusEffectType type)
    {
        return activeEffects.Any(e => e.Type == type);
    }
    
    /// <summary>
    /// Get a specific effect if it exists
    /// </summary>
    public StatusEffect GetEffect(StatusEffectType type)
    {
        return activeEffects.FirstOrDefault(e => e.Type == type);
    }
    
    /// <summary>
    /// Update all active effects (called each frame for time-based effects)
    /// </summary>
    public void UpdateEffects(float deltaTime)
    {
        if (activeEffects.Count == 0) return;
        
        // Update time-based effects
        foreach (var effect in activeEffects.ToList())
        {
            effect.UpdateTime(deltaTime);
            effect.OnUpdate(character);
            
            // Check for expiration
            if (effect.ShouldExpire())
            {
                Debug.Log($"{effect.DisplayName} expired on {character.name}");
                OnEffectExpired?.Invoke(effect);
                RemoveEffect(effect);
            }
        }
    }
    
    /// <summary>
    /// Called when the character takes an action (for action-based duration)
    /// </summary>
    public void OnCharacterAction()
    {
        foreach (var effect in activeEffects.ToList())
        {
            effect.OnActionTaken();
            
            if (effect.ShouldExpire())
            {
                Debug.Log($"{effect.DisplayName} expired after action on {character.name}");
                OnEffectExpired?.Invoke(effect);
                RemoveEffect(effect);
            }
        }
    }
    
    /// <summary>
    /// Called at the end of a turn (for turn-based duration)
    /// </summary>
    public void OnTurnEnd()
    {
        foreach (var effect in activeEffects.ToList())
        {
            effect.OnTurnEnd();
            
            if (effect.ShouldExpire())
            {
                Debug.Log($"{effect.DisplayName} expired at turn end on {character.name}");
                OnEffectExpired?.Invoke(effect);
                RemoveEffect(effect);
            }
        }
    }
    
    /// <summary>
    /// Remove all status effects (e.g., at battle end)
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var effect in activeEffects.ToList())
        {
            RemoveEffect(effect);
        }
        
        activeEffects.Clear();
    }
    
    /// <summary>
    /// Refresh an existing effect with new duration
    /// </summary>
    private void RefreshEffect(StatusEffect existingEffect, StatusEffect newEffect)
    {
        // Update duration values
        existingEffect.TurnsRemaining = Math.Max(existingEffect.TurnsRemaining, newEffect.TurnsRemaining);
        existingEffect.ActionsRemaining = Math.Max(existingEffect.ActionsRemaining, newEffect.ActionsRemaining);
        existingEffect.TimeRemaining = Math.Max(existingEffect.TimeRemaining, newEffect.TimeRemaining);
    }
    
    private void Update()
    {
        // Update time-based effects each frame
        if (activeEffects.Count > 0)
        {
            UpdateEffects(Time.deltaTime);
        }
    }
    
    private void OnDestroy()
    {
        ClearAllEffects();
    }
}
