using UnityEngine;
using System;
using System.Collections;
using System.Linq;

/// <summary>
/// Boss battle controller - adds boss-specific behavior to enemies
/// Add this component alongside EnemyBattleController on boss enemies
/// Handles: boss attacks with status effects, parrying player attacks, counter-attacks
/// </summary>
public class BossBattleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleCharacter bossCharacter;
    [SerializeField] private EnemyBattleController enemyController; // Reference to existing controller
    
    [Header("Boss Identity")]
    [SerializeField] private bool isBoss = true;
    [SerializeField] private string bossName = "Boss";
    
    [Header("Boss Attacks")]
    [Tooltip("Pool of attacks the boss can use")]
    [SerializeField] private BossAttackData[] attackPool;
    
    [Tooltip("Number of regular turns before using an effect attack")]
    [SerializeField] private int turnsBetweenEffectAttacks = 2;
    
    [Header("Boss Parry System")]
    [Tooltip("Can this boss parry player attacks?")]
    [SerializeField] private bool canParryPlayerAttacks = true;
    
    [Tooltip("Chance to parry light attacks (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float lightAttackParryChance = 0.4f;
    
    [Tooltip("Chance to parry heavy attacks (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float heavyAttackParryChance = 0.2f;
    
    [Tooltip("Animation state name for boss parry")]
    [SerializeField] private string bossParryAnimationName = "BossParry";
    
    [Header("Counter Attack")]
    [Tooltip("Damage dealt by counter-attack after parrying player")]
    [SerializeField] private float counterAttackDamage = 20f;
    
    [Tooltip("Animation state name for counter-attack")]
    [SerializeField] private string counterAttackAnimationName = "CounterAttack";
    
    [Tooltip("Delay before counter-attack after parry")]
    [SerializeField] private float counterAttackDelay = 0.5f;
    
    // State tracking
    private int turnCounter = 0;
    private bool isParryingPlayer = false;
    private bool isCounterAttacking = false;
    private BattleCharacter playerTarget;
    private ParrySystem parrySystem;
    private PlayerBattleController playerController;
    private bool lastParryWasSuccessful = false; // Track last parry result
    
    // Events
    public event Action OnBossParrySuccess;
    public event Action OnCounterAttackComplete;
    public event Action<BossAttackData> OnBossAttackChosen;
    
    // Properties
    public bool IsBoss => isBoss;
    public BattleCharacter Character => bossCharacter;
    public bool IsParrying => isParryingPlayer;
    public bool IsCounterAttacking => isCounterAttacking;
    public string BossName => bossName;
    
    private void Awake()
    {
        if (bossCharacter == null)
            bossCharacter = GetComponent<BattleCharacter>();
            
        if (enemyController == null)
            enemyController = GetComponent<EnemyBattleController>();
            
        ValidateConfiguration();
    }
    
    /// <summary>
    /// Initialize boss with required systems
    /// </summary>
    public void Initialize(ParrySystem parry, PlayerBattleController player)
    {
        this.parrySystem = parry;
        this.playerController = player;
        this.playerTarget = player.Character;
        
        // Subscribe to parry success event to track parry results
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccess += OnPlayerParrySuccess;
            Debug.Log($"🎭 Boss '{bossName}' subscribed to parry success events");
        }
        
        // Subscribe to player attack events to intercept
        if (playerController != null)
        {
            playerController.OnActionStarted += TryInterceptPlayerAttack;
            Debug.Log($"🎭 Boss '{bossName}' subscribed to player attack events");
        }
        
        Debug.Log($"🎭 Boss '{bossName}' initialized!");
    }
    
    /// <summary>
    /// Called when player successfully parries
    /// </summary>
    private void OnPlayerParrySuccess()
    {
        lastParryWasSuccessful = true;
        Debug.Log("🛡️ Player parry detected by boss");
    }
    
    /// <summary>
    /// Choose attack for this turn (called by BattleManager)
    /// </summary>
    public BossAttackData ChooseAttackForTurn()
    {
        turnCounter++;
        
        // Use effect attack periodically
        if (turnCounter >= turnsBetweenEffectAttacks)
        {
            turnCounter = 0;
            BossAttackData effectAttack = GetEffectAttack();
            if (effectAttack != null)
            {
                Debug.Log($"🎭 Boss choosing effect attack: {effectAttack.attackName}");
                return effectAttack;
            }
        }
        
        // Use regular attack
        BossAttackData regularAttack = GetRandomRegularAttack();
        if (regularAttack != null)
        {
            Debug.Log($"🎭 Boss choosing regular attack: {regularAttack.attackName}");
        }
        
        return regularAttack;
    }
    
    /// <summary>
    /// Get an attack that applies status effects
    /// </summary>
    private BossAttackData GetEffectAttack()
    {
        if (attackPool == null || attackPool.Length == 0) return null;
        return attackPool.FirstOrDefault(a => a != null && a.appliesStatusEffect);
    }
    
    /// <summary>
    /// Get a random regular (non-effect) attack
    /// </summary>
    private BossAttackData GetRandomRegularAttack()
    {
        if (attackPool == null || attackPool.Length == 0) return null;
        
        var regularAttacks = attackPool.Where(a => a != null && !a.appliesStatusEffect).ToArray();
        if (regularAttacks.Length > 0)
            return regularAttacks[UnityEngine.Random.Range(0, regularAttacks.Length)];
            
        // Fallback: return any attack
        return attackPool.FirstOrDefault(a => a != null);
    }
    
    /// <summary>
    /// Execute boss attack with status effects (replaces normal enemy attack)
    /// </summary>
    public IEnumerator ExecuteBossAttack(BossAttackData attackData, BattleCharacter target)
    {
        if (attackData == null)
        {
            Debug.LogError("Boss attack data is null!");
            yield break;
        }
        
        Debug.Log($"🎭 <color=red>Boss executes: {attackData.attackName}</color>");
        
        // Notify that boss chose this attack (for UI/effects)
        OnBossAttackChosen?.Invoke(attackData);
        
        // Play animation
        if (bossCharacter?.Animator != null)
        {
            bossCharacter.Animator.Play(attackData.animationStateName, 0);
            Debug.Log($"Playing boss attack animation: {attackData.animationStateName}");
        }
        
        // Wait for parry window timing
        float parryWindowDelay = parrySystem?.WindowOpenDelay ?? 0.4f;
        yield return new WaitForSeconds(parryWindowDelay);
        
        // Open parry window for player
        bool playerParried = false;
        if (parrySystem != null)
        {
            lastParryWasSuccessful = false; // Reset before opening window
            parrySystem.OpenParryWindow();
            Debug.Log("🛡️ Parry window opened for boss attack");
            
            yield return new WaitForSeconds(parrySystem.ParryWindowDuration);
            
            // Check if player parried using our tracked flag
            playerParried = lastParryWasSuccessful;
        }
        
        // Apply damage if not parried
        if (!playerParried && target != null && target.IsAlive)
        {
            target.TakeDamage(attackData.baseDamage);
            Debug.Log($"Boss attack dealt {attackData.baseDamage} damage to {target.name}");
            
            // Try to apply status effect
            TryApplyStatusEffect(attackData, target);
        }
        else if (playerParried)
        {
            Debug.Log("Player parried boss attack! No damage or effects applied.");
        }
        
        // Wait for animation to complete
        float remainingTime = attackData.attackDuration - parryWindowDelay - (parrySystem?.ParryWindowDuration ?? 0);
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
        
        Debug.Log($"Boss attack '{attackData.attackName}' complete");
    }
    
    /// <summary>
    /// Try to intercept player attack with boss parry
    /// </summary>
    private void TryInterceptPlayerAttack(ActionType attackType)
    {
        if (!canParryPlayerAttacks) return;
        if (isParryingPlayer) return; // Already parrying
        if (!bossCharacter.IsAlive) return; // Can't parry if dead
        
        // Only parry light/heavy attacks, not skills
        if (attackType != ActionType.LightAttack && attackType != ActionType.HeavyAttack)
        {
            Debug.Log($"🎭 Boss ignores {attackType} (only parries basic attacks)");
            return;
        }
        
        // Calculate parry chance
        float parryChance = attackType == ActionType.LightAttack 
            ? lightAttackParryChance 
            : heavyAttackParryChance;
        
        float roll = UnityEngine.Random.Range(0f, 1f);
        
        if (roll <= parryChance)
        {
            Debug.Log($"🛡️ <color=yellow>BOSS PARRIES {attackType}!</color> (Roll: {roll:F2} <= {parryChance:F2})");
            StartCoroutine(ExecuteBossParrySequence());
        }
        else
        {
            Debug.Log($"Boss failed to parry (Roll: {roll:F2} > {parryChance:F2})");
        }
    }
    
    /// <summary>
    /// Execute boss parry and counter-attack sequence
    /// </summary>
    private IEnumerator ExecuteBossParrySequence()
    {
        isParryingPlayer = true;
        
        // Notify that boss parried (BattleManager cancels player damage)
        OnBossParrySuccess?.Invoke();
        
        // Play boss parry animation
        if (bossCharacter?.Animator != null && !string.IsNullOrEmpty(bossParryAnimationName))
        {
            bossCharacter.Animator.Play(bossParryAnimationName, 0);
            Debug.Log($"Playing boss parry animation: {bossParryAnimationName}");
        }
        
        // Wait for parry animation
        yield return new WaitForSeconds(counterAttackDelay);
        
        // Execute counter-attack with player parry window (parry trade!)
        yield return ExecuteCounterAttack();
        
        isParryingPlayer = false;
    }
    
    /// <summary>
    /// Execute counter-attack after boss parry (player can parry back!)
    /// This creates the "parry trade" mechanic
    /// </summary>
    private IEnumerator ExecuteCounterAttack()
    {
        isCounterAttacking = true;
        
        Debug.Log("⚔️ <color=orange>Boss counter-attacks! Player can parry!</color>");
        
        // Play counter attack animation
        if (bossCharacter?.Animator != null && !string.IsNullOrEmpty(counterAttackAnimationName))
        {
            bossCharacter.Animator.Play(counterAttackAnimationName, 0);
            Debug.Log($"Playing counter-attack animation: {counterAttackAnimationName}");
        }
        
        // Wait for parry window timing
        float parryWindowDelay = parrySystem?.WindowOpenDelay ?? 0.4f;
        yield return new WaitForSeconds(parryWindowDelay);
        
        // Open parry window for player to parry counter-attack (PARRY TRADE!)
        bool playerParried = false;
        if (parrySystem != null)
        {
            lastParryWasSuccessful = false; // Reset before opening window
            parrySystem.OpenParryWindow();
            Debug.Log("🛡️ <color=cyan>Player can parry the counter-attack! (Parry Trade)</color>");
            
            yield return new WaitForSeconds(parrySystem.ParryWindowDuration);
            
            // Check if player parried using our tracked flag
            playerParried = lastParryWasSuccessful;
        }
        
        // Apply damage if player didn't parry
        if (!playerParried && playerTarget != null && playerTarget.IsAlive)
        {
            playerTarget.TakeDamage(counterAttackDamage);
            Debug.Log($"Boss counter-attack hit for {counterAttackDamage} damage!");
        }
        else if (playerParried)
        {
            Debug.Log("✅ <color=green>Player parried the counter-attack! Parry trade successful!</color>");
        }
        
        yield return new WaitForSeconds(0.5f);
        
        isCounterAttacking = false;
        OnCounterAttackComplete?.Invoke();
    }
    
    /// <summary>
    /// Try to apply status effect from attack
    /// </summary>
    private void TryApplyStatusEffect(BossAttackData attackData, BattleCharacter target)
    {
        if (!attackData.appliesStatusEffect) return;
        if (target.StatusEffectManager == null)
        {
            Debug.LogWarning($"Target {target.name} has no StatusEffectManager!");
            return;
        }
        
        float roll = UnityEngine.Random.Range(0f, 1f);
        
        if (roll <= attackData.effectApplicationChance)
        {
            StatusEffect effect = CreateStatusEffect(attackData);
            if (effect != null)
            {
                target.StatusEffectManager.ApplyEffect(effect);
                Debug.Log($"💀 <color=purple>Applied {effect.DisplayName} to {target.name}!</color>");
            }
        }
        else
        {
            Debug.Log($"Status effect roll failed (Roll: {roll:F2} > {attackData.effectApplicationChance:F2})");
        }
    }
    
    /// <summary>
    /// Create status effect from attack data
    /// </summary>
    private StatusEffect CreateStatusEffect(BossAttackData attackData)
    {
        switch (attackData.effectType)
        {
            case StatusEffectType.DisableParry:
                return new DisableParryEffect(attackData.effectDuration);
                
            case StatusEffectType.ReduceDamage:
                return new ReduceDamageEffect(attackData.effectDuration, 0.5f); // 50% damage reduction
                
            // Add more effect types as needed
            case StatusEffectType.None:
            default:
                Debug.LogWarning($"Cannot create status effect of type: {attackData.effectType}");
                return null;
        }
    }
    
    /// <summary>
    /// Clean up for next battle
    /// </summary>
    public void ResetForBattle()
    {
        turnCounter = 0;
        isParryingPlayer = false;
        isCounterAttacking = false;
        lastParryWasSuccessful = false;
        StopAllCoroutines();
        
        Debug.Log($"🎭 Boss '{bossName}' reset for battle");
    }
    
    /// <summary>
    /// Validate boss configuration
    /// </summary>
    private void ValidateConfiguration()
    {
        if (bossCharacter == null)
            Debug.LogError($"BossBattleController on {gameObject.name}: BattleCharacter reference is missing!");
            
        if (attackPool == null || attackPool.Length == 0)
            Debug.LogWarning($"BossBattleController on {gameObject.name}: No attacks configured!");
            
        if (canParryPlayerAttacks && string.IsNullOrEmpty(bossParryAnimationName))
            Debug.LogWarning($"BossBattleController on {gameObject.name}: Boss can parry but no parry animation is set!");
    }
    
    private void OnDestroy()
    {
        if (playerController != null)
        {
            playerController.OnActionStarted -= TryInterceptPlayerAttack;
        }
        
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccess -= OnPlayerParrySuccess;
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Draw gizmos in editor for visualization
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (bossCharacter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"BOSS: {bossName}");
        }
    }
    #endif
}
