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
    
    [Header("Animation Names")]
    [SerializeField] private string takeDamageAnimationName = "TakeDamage";
    [SerializeField] private string deathAnimationName = "Death";
    [SerializeField] private string victoryAnimationName = "Victory";
    
    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }
    public StaminaManager StaminaManager { get; private set; }
    public Animator Animator => animator;
    
    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float> OnDamageTaken; // damage amount
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
        OnDamageTaken?.Invoke(damage); // NEW: Notify damage taken
        Debug.Log($"{gameObject.name} took {damage} damage! HP: {CurrentHealth}/{maxHealth}");
        
        // Play TakeDamage animation if alive
        if (CurrentHealth > 0 && animator != null && !string.IsNullOrEmpty(takeDamageAnimationName))
        {
            animator.Play(takeDamageAnimationName, 0);
            Debug.Log($"{gameObject.name} playing TakeDamage animation");
        }
        
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
        
        // Play Death animation
        if (animator != null && !string.IsNullOrEmpty(deathAnimationName))
        {
            animator.Play(deathAnimationName, 0);
            Debug.Log($"{gameObject.name} playing Death animation");
        }
        
        OnDeath?.Invoke();

       
        if (isPlayer && GamepadVibrationManager.Instance != null)
        {
            GamepadVibrationManager.Instance.VibrateOnDeath();
        }
    }
    
    /// <summary>
    /// Play victory animation (typically for player only)
    /// </summary>
    public void PlayVictoryAnimation()
    {
        if (animator != null && !string.IsNullOrEmpty(victoryAnimationName))
        {
            animator.Play(victoryAnimationName, 0);
            Debug.Log($"{gameObject.name} playing Victory animation");
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

