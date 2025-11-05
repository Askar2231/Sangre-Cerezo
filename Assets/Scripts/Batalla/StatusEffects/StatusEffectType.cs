using UnityEngine;

/// <summary>
/// Types of status effects that can be applied to characters
/// </summary>
public enum StatusEffectType
{
    None,
    DisableParry,      // Cannot parry attacks
    ReduceDamage,      // Deals less damage with attacks
    Slow,              // Future: slower animations
    Stun,              // Future: skip turn
    Bleed              // Future: damage over time
}

/// <summary>
/// How long a status effect lasts
/// </summary>
public enum StatusEffectDuration
{
    OneAction,         // Lasts for one action by the affected character
    OneTurn,           // Lasts for one turn (player or enemy turn)
    TwoTurns,          // Lasts for two turns
    ThreeTurns,        // Lasts for three turns
    Permanent,         // Until dispelled or battle ends
    Custom             // Custom duration in seconds
}
