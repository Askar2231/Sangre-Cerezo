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
    [SerializeField] private string attackAnimationName = "EnemyAttack";
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackDuration = 2f;

    [Header("Attack Timing")]
    // Note: Parry window open delay is now configured in ParrySystem.cs
    // This value controls when damage is actually applied (after parry window)
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
    /// Execute enemy attack with animation
    /// </summary>
    public void ExecuteAttack(BattleCharacter playerTarget)
    {
        target = playerTarget;
        attackWasParried = false;

        StartCoroutine(AttackRoutine());
    }

    /// <summary>
    /// Attack routine with animation and parry system
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        Debug.Log("Enemy attacks!");

        // Play attack animation
        if (enemyCharacter?.Animator != null)
        {
            enemyCharacter.Animator.Play(attackAnimationName, 0);
            Debug.Log($"Playing enemy attack animation: {attackAnimationName}");
        }

        // Wait until parry window timing (get from ParrySystem)
        float parryWindowStartTime = parrySystem != null ? parrySystem.WindowOpenDelay : 0.4f;
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

        Debug.Log("Enemy attack complete");
        OnAttackComplete?.Invoke();
    }

    private void HandleParrySuccess()
    {
        attackWasParried = true;
        Debug.Log("Enemy attack was parried!");
        // Stamina and counter-attack are handled in BattleManager
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

