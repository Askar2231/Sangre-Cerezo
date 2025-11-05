using UnityEngine;

/// <summary>
/// Status effect that reduces the damage dealt by the character
/// Applied by boss attacks that weaken the player
/// </summary>
public class ReduceDamageEffect : StatusEffect
{
    private float damageMultiplier;
    
    /// <summary>
    /// Create a damage reduction effect
    /// </summary>
    /// <param name="turns">Number of turns the effect lasts</param>
    /// <param name="multiplier">Damage multiplier (0.5 = 50% damage, 0.25 = 25% damage)</param>
    public ReduceDamageEffect(int turns = 1, float multiplier = 0.5f)
    {
        Type = StatusEffectType.ReduceDamage;
        DurationType = StatusEffectDuration.OneTurn;
        TurnsRemaining = turns;
        ActionsRemaining = 0;
        TimeRemaining = 0;
        
        damageMultiplier = Mathf.Clamp01(multiplier);
        
        int percentReduction = Mathf.RoundToInt((1f - damageMultiplier) * 100f);
        DisplayName = "Weakened";
        Description = $"Damage reduced by {percentReduction}%";
    }
    
    public override void OnApply(BattleCharacter target)
    {
        // Store the multiplier in the character (you'll need to add this property)
        // For now, we'll just log it
        int percentReduction = Mathf.RoundToInt((1f - damageMultiplier) * 100f);
        Debug.Log($"<color=orange>⚠️ {target.name} damage reduced by {percentReduction}% for {TurnsRemaining} turn(s)!</color>");
        
        // Visual feedback
        if (GamepadVibrationManager.Instance != null && target.GetComponent<PlayerBattleController>() != null)
        {
            // Use generic vibration
            GamepadVibrationManager.Instance.VibrateOnTakeDamage(0.2f);
        }
    }
    
    public override void OnRemove(BattleCharacter target)
    {
        Debug.Log($"<color=green>✅ {target.name} damage restored to normal!</color>");
    }
    
    /// <summary>
    /// Get the damage multiplier for this effect
    /// Call this when calculating damage output
    /// </summary>
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }
}
