using UnityEngine;

/// <summary>
/// Status effect that prevents the character from parrying attacks
/// Applied by boss "breaking" attacks (e.g., broken sword)
/// </summary>
public class DisableParryEffect : StatusEffect
{
    public DisableParryEffect(int turns = 1)
    {
        Type = StatusEffectType.DisableParry;
        DurationType = StatusEffectDuration.OneTurn;
        TurnsRemaining = turns;
        ActionsRemaining = 0;
        TimeRemaining = 0;
        
        DisplayName = "Broken Guard";
        Description = "Cannot parry enemy attacks!";
    }
    
    public override void OnApply(BattleCharacter target)
    {
        // Disable parrying on the target
        target.CanParry = false;
        
        Debug.Log($"<color=red>💔 {target.name} cannot parry for {TurnsRemaining} turn(s)!</color>");
        
        // Visual feedback (optional - can add particle effects here)
        if (GamepadVibrationManager.Instance != null && target.GetComponent<PlayerBattleController>() != null)
        {
            // Use generic vibration (VibrateOnDebuff doesn't exist yet)
            GamepadVibrationManager.Instance.VibrateOnTakeDamage(0.3f);
        }
    }
    
    public override void OnRemove(BattleCharacter target)
    {
        // Re-enable parrying
        target.CanParry = true;
        
        Debug.Log($"<color=green>✅ {target.name} can parry again!</color>");
    }
    
    public override void OnUpdate(BattleCharacter target)
    {
        // Ensure parrying stays disabled while effect is active
        if (target.CanParry)
        {
            target.CanParry = false;
        }
    }
}
