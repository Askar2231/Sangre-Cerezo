using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Handles enemy-specific battle logic and AI
/// </summary>
public class EnemyBattleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleCharacter enemyCharacter;
    
    [Header("Attack Settings")]
    // [SerializeField] private string attackAnimationName = "EnemyAttack"; // COMENTADO - Sin animaciones por ahora
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackDuration = 2f;
    
    [Header("Attack Timing")]
    [SerializeField] private float parryWindowStartTime = 1.5f; // When to open parry window
    [SerializeField] private float damageApplicationTime = 1.7f; // When damage is dealt
    
    [Header("AI Settings")]
    [SerializeField] private float thinkingDuration = 1f;
    
    public BattleCharacter Character => enemyCharacter;
    
    // Events
    public event Action OnAttackComplete;
    public event Action OnThinkingComplete;
    
    private ParrySystem parrySystem;
    private BattleCharacter target;
    private bool attackWasParried = false;
    
    private void Awake()
    {
        if (enemyCharacter == null)
        {
            enemyCharacter = GetComponent<BattleCharacter>();
        }
    }
    
    public void Initialize(ParrySystem parry)
    {
        this.parrySystem = parry;
        
        // Subscribe to parry events
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccess += HandleParrySuccess;
            parrySystem.OnParryFail += HandleParryFail;
        }
        else
        {
            Debug.LogWarning("ParrySystem is null in EnemyBattleController");
        }
    }
    
    /// <summary>
    /// Execute AI thinking phase
    /// </summary>
    public void ExecuteThinking()
    {
        StartCoroutine(ThinkingRoutine());
    }
    
    private IEnumerator ThinkingRoutine()
    {
        Debug.Log("Enemy is thinking...");
        yield return new WaitForSeconds(thinkingDuration);
        
        OnThinkingComplete?.Invoke();
    }
    
    /// <summary>
    /// Execute enemy attack (sin animaciones, solo delays temporales)
    /// </summary>
    public void ExecuteAttack(BattleCharacter playerTarget)
    {
        target = playerTarget;
        attackWasParried = false;
        
        StartCoroutine(AttackRoutineTemporary());
    }
    
    /// <summary>
    /// Rutina de ataque temporal sin animaciones
    /// </summary>
    private IEnumerator AttackRoutineTemporary()
    {
        Debug.Log("Enemy attack started (simulated animation)");
        
        // COMENTADO - Sin animaciones por ahora
        // enemyCharacter.Animator.Play(attackAnimationName);
        
        // Wait until parry window timing
        yield return new WaitForSeconds(parryWindowStartTime);
        
        // Open parry window
        if (parrySystem != null)
        {
            parrySystem.OpenParryWindow();
            Debug.Log("Parry window opened!");
        }
        
        // Wait until damage application time
        float remainingTime = damageApplicationTime - parryWindowStartTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        
        // Apply damage if not parried
        if (!attackWasParried)
        {
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"Enemy dealt {attackDamage} damage!");
            }
        }
        else
        {
            Debug.Log("Attack was parried! No damage dealt.");
        }
        
        // Wait for attack duration to complete
        float finalWait = attackDuration - damageApplicationTime;
        if (finalWait > 0)
        {
            yield return new WaitForSeconds(finalWait);
        }
        
        Debug.Log("Enemy attack complete (simulated animation finished)");
        OnAttackComplete?.Invoke();
    }

    /// <summary>
    /// Rutina de ataque original con animaciones (para cuando estén disponibles)
    /// </summary>
    private IEnumerator AttackRoutineWithAnimations()
    {
        Debug.Log("Enemy attacks!");

        // Play attack animation
        // enemyCharacter.Animator.Play(attackAnimationName);

        // Wait until parry window timing
        yield return new WaitForSeconds(parryWindowStartTime);

        // Open parry window
        if (parrySystem != null)
        {
            parrySystem.OpenParryWindow();
        }

        // Wait until damage application time
        float remainingTime = damageApplicationTime - parryWindowStartTime;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // Apply damage if not parried
        if (!attackWasParried)
        {
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                Debug.Log($"Enemy dealt {attackDamage} damage!");
            }
        }
        else
        {
            Debug.Log("Attack was parried! No damage dealt.");
        }

        // Wait for animation to complete
        float finalWait = attackDuration - damageApplicationTime;
        if (finalWait > 0)
        {
            yield return new WaitForSeconds(finalWait);
        }

        OnAttackComplete?.Invoke();
    }
    
    /*private void HandleParrySuccess()
    {
        attackWasParried = true;
        
        // Grant stamina to player
        if (target != null && parrySystem != null)
        {
            target.StaminaManager.AddStamina(parrySystem.StaminaReward);
            Debug.Log($"Parry successful! Gained {parrySystem.StaminaReward} stamina!");
        }
    }*/
    
    private void HandleParrySuccess()
    {
        attackWasParried = true;
        Debug.Log("Enemy attack was parried!");
        // La stamina y el contrataque se manejan en BattleManager
    }
    
    private void HandleParryFail()
    {
        attackWasParried = false;
        Debug.Log("Parry failed!");
    }
    
    /// <summary>
    /// Reset for new battle
    /// </summary>
    public void ResetForBattle()
    {
        if (enemyCharacter != null)
        {
            enemyCharacter.ResetForBattle();
        }
        attackWasParried = false;
        StopAllCoroutines();
    }
    
    private void OnDestroy()
    {
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccess -= HandleParrySuccess;
            parrySystem.OnParryFail -= HandleParryFail;
        }
    }
}

