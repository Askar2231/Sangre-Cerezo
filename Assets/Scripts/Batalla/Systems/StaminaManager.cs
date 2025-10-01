using UnityEngine;
using System;

/// <summary>
/// Manages stamina for a battle character
/// </summary>
public class StaminaManager
{
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }
    
    public event Action<float, float> OnStaminaChanged; // current, max
    
    public StaminaManager(float maxStamina)
    {
        MaxStamina = maxStamina;
        CurrentStamina = maxStamina;
    }
    
    /// <summary>
    /// Restore stamina to max at the start of turn
    /// </summary>
    public void RestoreToMax()
    {
        CurrentStamina = MaxStamina;
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
    
    /// <summary>
    /// Check if there's enough stamina for an action
    /// </summary>
    public bool HasEnoughStamina(float cost)
    {
        return CurrentStamina >= cost;
    }
    
    /// <summary>
    /// Consume stamina for an action
    /// </summary>
    public bool ConsumeStamina(float amount)
    {
        if (!HasEnoughStamina(amount))
        {
            return false;
        }
        
        CurrentStamina -= amount;
        CurrentStamina = Mathf.Max(0, CurrentStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        return true;
    }
    
    /// <summary>
    /// Add stamina (e.g., from successful parry)
    /// </summary>
    public void AddStamina(float amount)
    {
        CurrentStamina += amount;
        CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
    
    /// <summary>
    /// Set max stamina (for upgrades/buffs)
    /// </summary>
    public void SetMaxStamina(float newMax)
    {
        MaxStamina = newMax;
        CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
}

