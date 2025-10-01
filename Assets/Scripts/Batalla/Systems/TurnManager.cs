using UnityEngine;
using System;

/// <summary>
/// Manages turn flow in battle
/// </summary>
public class TurnManager
{
    public BattleState CurrentBattleState { get; private set; }
    public PlayerTurnState CurrentPlayerTurnState { get; private set; }
    public EnemyTurnState CurrentEnemyTurnState { get; private set; }
    
    public event Action<BattleState> OnBattleStateChanged;
    public event Action<PlayerTurnState> OnPlayerTurnStateChanged;
    public event Action<EnemyTurnState> OnEnemyTurnStateChanged;
    
    private BattleCharacter player;
    private BattleCharacter enemy;
    
    public TurnManager(BattleCharacter player, BattleCharacter enemy)
    {
        this.player = player;
        this.enemy = enemy;
        CurrentBattleState = BattleState.BattleStart;
    }
    
    /// <summary>
    /// Change the overall battle state
    /// </summary>
    public void ChangeBattleState(BattleState newState)
    {
        if (CurrentBattleState == newState) return;
        
        Debug.Log($"Battle State: {CurrentBattleState} -> {newState}");
        CurrentBattleState = newState;
        OnBattleStateChanged?.Invoke(newState);
        
        // Initialize substates
        switch (newState)
        {
            case BattleState.PlayerTurn:
                ChangePlayerTurnState(PlayerTurnState.SelectingAction);
                break;
            case BattleState.EnemyTurn:
                ChangeEnemyTurnState(EnemyTurnState.Thinking);
                break;
        }
    }
    
    /// <summary>
    /// Change player turn state
    /// </summary>
    public void ChangePlayerTurnState(PlayerTurnState newState)
    {
        if (CurrentPlayerTurnState == newState) return;
        
        Debug.Log($"Player Turn State: {CurrentPlayerTurnState} -> {newState}");
        CurrentPlayerTurnState = newState;
        OnPlayerTurnStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// Change enemy turn state
    /// </summary>
    public void ChangeEnemyTurnState(EnemyTurnState newState)
    {
        if (CurrentEnemyTurnState == newState) return;
        
        Debug.Log($"Enemy Turn State: {CurrentEnemyTurnState} -> {newState}");
        CurrentEnemyTurnState = newState;
        OnEnemyTurnStateChanged?.Invoke(newState);
    }
    
    /// <summary>
    /// Start player's turn
    /// </summary>
    public void StartPlayerTurn()
    {
        player.StaminaManager.RestoreToMax();
        ChangeBattleState(BattleState.PlayerTurn);
    }
    
    /// <summary>
    /// End player's turn
    /// </summary>
    public void EndPlayerTurn()
    {
        ChangePlayerTurnState(PlayerTurnState.TurnEnd);
        ChangeBattleState(BattleState.EnemyTurn);
    }
    
    /// <summary>
    /// Start enemy's turn
    /// </summary>
    public void StartEnemyTurn()
    {
        enemy.StaminaManager.RestoreToMax();
        ChangeBattleState(BattleState.EnemyTurn);
    }
    
    /// <summary>
    /// End enemy's turn
    /// </summary>
    public void EndEnemyTurn()
    {
        ChangeEnemyTurnState(EnemyTurnState.TurnEnd);
        ChangeBattleState(BattleState.PlayerTurn);
    }
}

