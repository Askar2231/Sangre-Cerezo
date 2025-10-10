using UnityEngine;
using System;

/// <summary>
/// Base class for all characters in battle (Player and Enemy)
/// </summary>
public class BattleCharacter : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxStamina = 100f;

    [Header("Components")]
    [SerializeField] private Animator animator;
    
    [Header("Character Type")]
    [SerializeField] private bool isPlayer = false;
    
    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public StaminaManager StaminaManager { get; private set; }
    public Animator Animator => animator;
    
    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;
    
    public bool IsAlive => CurrentHealth > 0;
    
    protected virtual void Awake()
    {
        CurrentHealth = maxHealth;
        StaminaManager = new StaminaManager(maxStamina);
    }
    
    /// <summary>
    /// Take damage
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;
        
        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(0, CurrentHealth);
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Debug.Log($"{gameObject.name} took {damage} damage! HP: {CurrentHealth}/{maxHealth}");
        
        if (isPlayer && GamepadVibrationManager.Instance != null)
        {
            float damagePercent = damage / maxHealth;
            GamepadVibrationManager.Instance.VibrateOnTakeDamage(damagePercent);
        }
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// Heal health
    /// </summary>
    public void Heal(float amount)
    {
        if (!IsAlive) return;
        
        CurrentHealth += amount;
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Debug.Log($"{gameObject.name} healed {amount} HP! HP: {CurrentHealth}/{maxHealth}");

        
        if (isPlayer && GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnHeal();
        }
    }
    
    /// <summary>
    /// Called when character dies
    /// </summary>
    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} has been defeated!");
        OnDeath?.Invoke();

       
        if (isPlayer && GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnDeath();
        }
    }
    
    /// <summary>
    /// Reset character for new battle
    /// </summary>
    public virtual void ResetForBattle()
    {
        CurrentHealth = maxHealth;
        StaminaManager.RestoreToMax();
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

