using UnityEngine;

/// <summary>
/// Battle state enums and definitions
/// </summary>

public enum BattleState
{
    BattleStart,
    PlayerTurn,
    EnemyTurn,
    BattleEnd
}

public enum PlayerTurnState
{
    SelectingAction,
    ExecutingAttack,
    ExecutingSkill,
    WaitingForInput,
    TurnEnd
}

public enum EnemyTurnState
{
    Thinking,
    Attacking,
    TurnEnd
}

public enum BattleResult
{
    None,
    PlayerVictory,
    PlayerDefeated
}

public enum ActionType
{
    LightAttack,
    HeavyAttack,
    Skill,
    EndTurn
}

