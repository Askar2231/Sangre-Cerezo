using UnityEngine;
using System; 

public enum RobberyQuestState
{
    NotStarted,
    DialogueIntro,
    CombatActive,
    PostCombatDecision,
    FindBoss,
    BossCombatActive,
    PostBossDecision,
    ReturnToMerchant,
    Completed,
    TalkToFirstNPC 
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Referencias de Assets y GameObjects")]
    [SerializeField] private Decision initialMerchantDialogue; 
    [SerializeField] private Decision thiefPostCombatDecision;     
    [SerializeField] private Decision finalMerchantDecision;       
    [SerializeField] private GameObject thiefEnemy; // Pre-placed enemy GameObject (disabled by default)
    [SerializeField] private GameObject bossEnemy; // Boss enemy GameObject (disabled by default)
    [SerializeField] private Decision bossPostCombatDecision; // Decision after defeating boss
    [SerializeField] private GameObject merchantNPC;               
    
    private RobberyQuestState currentQuestState = RobberyQuestState.NotStarted;
    private bool callbacksRegistered = false;
    private BattleManagerV2 activeBattleManager;
    
    public static event Action<RobberyQuestState> OnQuestStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
       
        if (Instance == this)
        {
            Invoke(nameof(RegisterChoiceCallbacks), 0.2f);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from battle events if still subscribed
        if (activeBattleManager != null)
        {
            activeBattleManager.OnBattleEnded -= HandleThiefBattleEnded;
            activeBattleManager.OnBattleEnded -= HandleBossBattleEnded;
        }
        
        UnregisterChoiceCallbacks();
    }

 
    private void RegisterChoiceCallbacks()
    {
        if (callbacksRegistered) return;
        
        if (ChoiceEventSystem.Instance == null)
        {
            Debug.LogError("ChoiceEventSystem.Instance es null. Reintentando en 0.5 segundos...");
            Invoke(nameof(RegisterChoiceCallbacks), 0.5f);
            return;
        }

        Debug.Log("<color=yellow>Iniciando registro de callbacks...</color>");

        ChoiceEventSystem.Instance.RegisterChoice("merchant_accept_help", PlayerAcceptedHelp);
        ChoiceEventSystem.Instance.RegisterChoice("merchant_decline_help", PlayerDeclinedHelp);
        ChoiceEventSystem.Instance.RegisterChoice("thief_forgive", PlayerForgivenThief);
        ChoiceEventSystem.Instance.RegisterChoice("thief_kill", PlayerKilledThief);
        ChoiceEventSystem.Instance.RegisterChoice("boss_forgive", PlayerForgivenBoss);
        ChoiceEventSystem.Instance.RegisterChoice("boss_kill", PlayerKilledBoss);
        ChoiceEventSystem.Instance.RegisterChoice("merchant_keep_omamori", PlayerKeptOmamori);
        ChoiceEventSystem.Instance.RegisterChoice("merchant_return_omamori", PlayerReturnedOmamori);
        
        callbacksRegistered = true;
        Debug.Log("<color=green>✓ Callbacks de misión registrados exitosamente en ChoiceEventSystem.</color>");
    }


    private void UnregisterChoiceCallbacks()
    {
        if (ChoiceEventSystem.Instance != null)
        {
            ChoiceEventSystem.Instance.UnregisterChoice("merchant_accept_help", PlayerAcceptedHelp);
            ChoiceEventSystem.Instance.UnregisterChoice("merchant_decline_help", PlayerDeclinedHelp);
            ChoiceEventSystem.Instance.UnregisterChoice("thief_forgive", PlayerForgivenThief);
            ChoiceEventSystem.Instance.UnregisterChoice("thief_kill", PlayerKilledThief);
            ChoiceEventSystem.Instance.UnregisterChoice("boss_forgive", PlayerForgivenBoss);
            ChoiceEventSystem.Instance.UnregisterChoice("boss_kill", PlayerKilledBoss);
            ChoiceEventSystem.Instance.UnregisterChoice("merchant_keep_omamori", PlayerKeptOmamori);
            ChoiceEventSystem.Instance.UnregisterChoice("merchant_return_omamori", PlayerReturnedOmamori);
        }
    }

 

    public void StartRobberyQuest()
    {
        if (currentQuestState == RobberyQuestState.NotStarted)
        {
            UpdateQuestState(RobberyQuestState.DialogueIntro);
            InteractionManager.Instance.StartInteraction(initialMerchantDialogue);
        }
    }

    public void PlayerAcceptedHelp()
    {
        Debug.Log("<color=green>PlayerAcceptedHelp EJECUTADO!</color>");
        InteractionManager.Instance.EndInteraction();
        UpdateQuestState(RobberyQuestState.CombatActive);
        EnableThiefEnemy();
    }

    public void PlayerDeclinedHelp()
    {
        InteractionManager.Instance.EndInteraction();
        UpdateQuestState(RobberyQuestState.NotStarted);
        Debug.Log("Jugador rechazó ayudar al mercader.");
    }


   /// <summary>
   /// Enable the pre-placed thief enemy GameObject
   /// </summary>
   private void EnableThiefEnemy()
   {
       if (thiefEnemy == null)
       {
           Debug.LogError("Thief enemy GameObject not assigned in QuestManager!");
           UpdateQuestState(RobberyQuestState.NotStarted);
           return;
       }
       
       // Enable the enemy GameObject
       thiefEnemy.SetActive(true);
       
       // Get BattleTrigger component
       BattleTrigger battleTrigger = thiefEnemy.GetComponentInChildren<BattleTrigger>();
       if (battleTrigger != null)
       {
           // Enable trigger immediately so player can approach and start battle
           battleTrigger.SetTriggerActive(true);
           
           // Subscribe to battle end via BattleManager
           BattleManagerV2 battleManager = FindFirstObjectByType<BattleManagerV2>();
           if (battleManager != null)
           {
               activeBattleManager = battleManager;
               activeBattleManager.OnBattleEnded += HandleThiefBattleEnded;
               Debug.Log("<color=green>Subscribed to BattleManager.OnBattleEnded</color>");
           }
           else
           {
               Debug.LogError("BattleManagerV2 not found in scene!");
           }
           
           Debug.Log("<color=cyan>¡Ladrón habilitado! Acércate para iniciar el combate.</color>");
       }
       else
       {
           Debug.LogError("BattleTrigger not found on thief enemy! Add BattleTrigger component to thief GameObject.");
           UpdateQuestState(RobberyQuestState.NotStarted);
       }
   }

   /// <summary>
   /// Enable the pre-placed boss enemy GameObject
   /// </summary>
   private void EnableBossEnemy()
   {
       if (bossEnemy == null)
       {
           Debug.LogError("Boss enemy GameObject not assigned in QuestManager!");
           UpdateQuestState(RobberyQuestState.ReturnToMerchant); // Skip boss if not configured
           return;
       }
       
       // Enable the enemy GameObject
       bossEnemy.SetActive(true);
       
       // Get BattleTrigger component
       BattleTrigger battleTrigger = bossEnemy.GetComponentInChildren<BattleTrigger>();
       if (battleTrigger != null)
       {
           // Enable trigger immediately so player can approach and start battle
           battleTrigger.SetTriggerActive(true);
           
           // Subscribe to battle end via BattleManager (reuse activeBattleManager if still exists)
           if (activeBattleManager == null)
           {
               activeBattleManager = FindFirstObjectByType<BattleManagerV2>();
           }
           
           if (activeBattleManager != null)
           {
               // Unsubscribe from thief handler and subscribe to boss handler
               activeBattleManager.OnBattleEnded -= HandleThiefBattleEnded;
               activeBattleManager.OnBattleEnded += HandleBossBattleEnded;
               Debug.Log("<color=green>Subscribed to BattleManager.OnBattleEnded for boss</color>");
           }
           else
           {
               Debug.LogError("BattleManagerV2 not found in scene!");
           }
           
           Debug.Log("<color=cyan>¡Jefe habilitado! Acércate para iniciar el combate.</color>");
       }
       else
       {
           Debug.LogError("BattleTrigger not found on boss enemy! Add BattleTrigger component to boss GameObject.");
           UpdateQuestState(RobberyQuestState.ReturnToMerchant); // Skip boss if misconfigured
       }
   }

   /// <summary>
   /// Called when player reaches the boss trigger (NOT USED - kept for potential future use)
   /// </summary>
   private void OnPlayerReachedBoss()
   {
       Debug.Log("<color=yellow>Player reached boss! Starting boss combat...</color>");
       UpdateQuestState(RobberyQuestState.BossCombatActive);
   }

  public void StartThiefCombat()
{
    Debug.Log($"<color=cyan>StartThiefCombat called. Current state: {currentQuestState}</color>");
    
    if (currentQuestState == RobberyQuestState.CombatActive && thiefEnemy != null)
    {
        BattleTrigger battleTrigger = thiefEnemy.GetComponentInChildren<BattleTrigger>();
        if (battleTrigger != null)
        {
            // Enable trigger so player can enter and start battle
            battleTrigger.SetTriggerActive(true);
            Debug.Log("<color=green>Battle trigger enabled! Player can now approach to start combat.</color>");
        }
        else
        {
            Debug.LogError("BattleTrigger not found on thief enemy!");
        }
    }
    else
    {
        Debug.LogWarning($"Cannot start combat. State: {currentQuestState}, Thief enemy assigned: {thiefEnemy != null}");
    }
}
    
    private void SimulateCombatWin()
    {
        OnThiefCombatEnd(true);
    }

 
    public void OnThiefCombatEnd(bool playerWon)
    {
        if (currentQuestState != RobberyQuestState.CombatActive) return;

        if (playerWon)
        {
            Debug.Log("Combate con el ladrón terminado. ¡Victoria!");
            
            // Note: BattleTrigger will destroy the enemy automatically if destroyOnBattleEnd=true
            // If it's not being destroyed automatically, the enemy GameObject stays disabled
            
            UpdateQuestState(RobberyQuestState.PostCombatDecision);
            InteractionManager.Instance.StartInteraction(thiefPostCombatDecision);
        }
        else
        {
            Debug.Log("Combate con el ladrón terminado. Derrota.");
            
            // On defeat, disable the enemy so player can retry
            if (thiefEnemy != null && thiefEnemy.activeInHierarchy)
            {
                thiefEnemy.SetActive(false);
            }
            
            UpdateQuestState(RobberyQuestState.NotStarted);
        }
    }

    /// <summary>
    /// Handles the battle ended event from BattleManagerV2
    /// </summary>
    private void HandleThiefBattleEnded(BattleResult result)
    {
        Debug.Log($"<color=cyan>Thief battle ended with result: {result}</color>");
        
        // Unsubscribe from event
        if (activeBattleManager != null)
        {
            activeBattleManager.OnBattleEnded -= HandleThiefBattleEnded;
            activeBattleManager = null;
        }
        
        // NUEVO: Solo cambiar estado si ganó, NO destruir enemigo automáticamente
        if (result == BattleResult.PlayerVictory)
        {
            Debug.Log("¡Victoria! El enemigo está derrotado pero esperando tu decisión...");
            UpdateQuestState(RobberyQuestState.PostCombatDecision);
            InteractionManager.Instance.StartInteraction(thiefPostCombatDecision);
        }
        else
        {
            Debug.Log("Derrota. Reiniciando misión...");
            if (thiefEnemy != null && thiefEnemy.activeInHierarchy)
            {
                thiefEnemy.SetActive(false);
            }
            UpdateQuestState(RobberyQuestState.NotStarted);
        }
    }

    /// <summary>
    /// Handles the boss battle ended event from BattleManagerV2
    /// </summary>
    private void HandleBossBattleEnded(BattleResult result)
    {
        Debug.Log($"<color=cyan>Boss battle ended with result: {result}</color>");
        
        // Unsubscribe from event
        if (activeBattleManager != null)
        {
            activeBattleManager.OnBattleEnded -= HandleBossBattleEnded;
            activeBattleManager = null;
        }
        
        // Check if player won
        if (result == BattleResult.PlayerVictory)
        {
            Debug.Log("¡Victoria contra el jefe! Decide su destino...");
            UpdateQuestState(RobberyQuestState.PostBossDecision);
            
            // Present post-boss combat decision
            if (bossPostCombatDecision != null)
            {
                InteractionManager.Instance.StartInteraction(bossPostCombatDecision);
            }
            else
            {
                Debug.LogError("Boss post-combat decision not assigned! Skipping to merchant.");
                UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            }
        }
        else
        {
            Debug.Log("Derrota contra el jefe. Reiniciando misión...");
            if (bossEnemy != null && bossEnemy.activeInHierarchy)
            {
                bossEnemy.SetActive(false);
            }
            UpdateQuestState(RobberyQuestState.NotStarted);
        }
    }

    public void PlayerForgivenThief()
    {
        if (currentQuestState == RobberyQuestState.PostCombatDecision)
        {
            InteractionManager.Instance.EndInteraction();
            
            // NUEVO: Ejecutar animación de perdón
            ExecuteThiefForgiveness();
            
            // Enable boss encounter instead of going directly to merchant
            EnableBossEnemy();
            UpdateQuestState(RobberyQuestState.FindBoss);
            Debug.Log("Ladrón perdonado. Ahora debes encontrar al jefe de los ladrones.");
        }
    }

    public void PlayerKilledThief()
    {
        if (currentQuestState == RobberyQuestState.PostCombatDecision)
        {
            InteractionManager.Instance.EndInteraction();
            
            // NUEVO: Ejecutar animación de muerte
            ExecuteThiefDeath();
            
            UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            Debug.Log("Ladrón eliminado. Vuelve con el Mercader.");
        }
    }

    public void PlayerForgivenBoss()
    {
        if (currentQuestState == RobberyQuestState.PostBossDecision)
        {
            InteractionManager.Instance.EndInteraction();
            
            // Execute boss forgiveness animation
            ExecuteBossForgiveness();
            
            UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            Debug.Log("Jefe perdonado. Vuelve con el Mercader.");
        }
    }

    public void PlayerKilledBoss()
    {
        if (currentQuestState == RobberyQuestState.PostBossDecision)
        {
            InteractionManager.Instance.EndInteraction();
            
            // Execute boss death animation
            ExecuteBossDeath();
            
            UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            Debug.Log("Jefe eliminado. Vuelve con el Mercader.");
        }
    }

    
    public void StartFinalMerchantDialogue()
    {
        Debug.Log($"<color=cyan>StartFinalMerchantDialogue llamado. Estado actual: {currentQuestState}</color>");
        
        if (currentQuestState == RobberyQuestState.ReturnToMerchant)
        {
            // Player completed combat and returned to merchant
            if (finalMerchantDecision != null)
            {
                Debug.Log("<color=green>Mostrando diálogo final del mercader</color>");
                InteractionManager.Instance.StartInteraction(finalMerchantDecision);
            }
            else
            {
                Debug.LogError("finalMerchantDecision es NULL! Asigna el ScriptableObject Decision_FinalMercader en el Inspector del QuestManager.");
            }
        }
        else if (currentQuestState == RobberyQuestState.NotStarted)
        {
            // Quest hasn't started yet - show initial dialogue
            Debug.Log("<color=yellow>Mostrando diálogo inicial del mercader</color>");
            StartRobberyQuest();
        }
        else if (currentQuestState == RobberyQuestState.CombatActive || 
                 currentQuestState == RobberyQuestState.DialogueIntro)
        {
            // Player is in middle of quest (combat hasn't finished yet)
            // Show a reminder message
            Debug.Log("<color=yellow>Mercader dice: 'Ve a buscar al ladrón! Está por allí...'</color>");
            // Optionally, you could show a simple conversation here reminding the player of their task
        }
        else if (currentQuestState == RobberyQuestState.PostCombatDecision)
        {
            // Player defeated thief but hasn't made forgive/kill decision yet
            Debug.Log("<color=yellow>Mercader dice: '¿Qué hiciste con el ladrón?'</color>");
            // Player should go back to the thief's location, not the merchant
        }
        else if (currentQuestState == RobberyQuestState.Completed)
        {
            // Quest already completed
            Debug.Log("<color=cyan>Mercader dice: 'Gracias de nuevo por tu ayuda, samurai.'</color>");
        }
        else
        {
            Debug.LogWarning($"Estado de misión no manejado: {currentQuestState}");
        }
    }

    public void PlayerKeptOmamori()
    {
        if (currentQuestState == RobberyQuestState.ReturnToMerchant)
        {
            UpdateQuestState(RobberyQuestState.Completed);
            Debug.Log("Misión completada: Jugador se quedó con el Omamori.");
        }
    }

    public void PlayerReturnedOmamori()
    {
        if (currentQuestState == RobberyQuestState.ReturnToMerchant)
        {
            UpdateQuestState(RobberyQuestState.Completed);
            Debug.Log("Misión completada: Jugador devolvió el Omamori.");
        }
    }

    private void UpdateQuestState(RobberyQuestState newState)
    {
        currentQuestState = newState;
        Debug.Log($"<color=orange>Estado de Misión actualizado:</color> {newState}");
        OnQuestStateChanged?.Invoke(currentQuestState);
    }

    public RobberyQuestState GetCurrentQuestState()
    {
        return currentQuestState;
    }

    // En QuestManager, agrega métodos para activar esta misión:
    public void StartTalkToFirstNPCQuest()
    {
        UpdateQuestState(RobberyQuestState.TalkToFirstNPC);
    }

    // Y en UpdateObjectiveFromRobberyQuest, agrega el texto:
    
    /// <summary>
    /// Execute thief death animation using BattleCharacter
    /// </summary>
    private void ExecuteThiefDeath()
    {
        if (thiefEnemy != null)
        {
            // Buscar BattleCharacter en el enemigo
            BattleCharacter enemyCharacter = thiefEnemy.GetComponent<BattleCharacter>();
            if (enemyCharacter != null)
            {
                Debug.Log("<color=red>Ejecutando muerte del ladrón...</color>");
                
                // Forzar animación de muerte
                if (enemyCharacter.Animator != null)
                {
                    enemyCharacter.Animator.Play("Asesinado", 0);
                }
                
                // Destruir después de un delay para que se vea la animación
                StartCoroutine(DestroyThiefAfterDeathAnimation(3f));
            }
            else
            {
                Debug.LogWarning("BattleCharacter not found on thief enemy!");
                // Fallback: destroy directly after delay
                StartCoroutine(DestroyThiefAfterDelay(2f));
            }
        }
    }

    /// <summary>
    /// Execute thief forgiveness animation using BattleCharacter
    /// </summary>
    private void ExecuteThiefForgiveness()
    {
        if (thiefEnemy != null)
        {
            // Buscar BattleCharacter en el enemigo
            BattleCharacter enemyCharacter = thiefEnemy.GetComponent<BattleCharacter>();
            if (enemyCharacter != null)
            {
                Debug.Log("<color=green>Ejecutando perdón del ladrón...</color>");
                
                // Usar animación de victoria como "perdón" (se va feliz)
                if (enemyCharacter.Animator != null)
                {
                    enemyCharacter.Animator.Play("Perdonado", 0); // O cualquier animación de "irse"
                }
                
                // Desactivar después de un delay para que se vea la animación
                StartCoroutine(DisableThiefAfterForgivenessAnimation(3f));
            }
            else
            {
                Debug.LogWarning("BattleCharacter not found on thief enemy!");
                // Fallback: disable directly after delay
                StartCoroutine(DisableThiefAfterDelay(2f));
            }
        }
    }

    /// <summary>
    /// Destroy thief after death animation completes
    /// </summary>
    private System.Collections.IEnumerator DestroyThiefAfterDeathAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (thiefEnemy != null)
        {
            Debug.Log("<color=red>Ladrón eliminado después de animación de muerte</color>");
            Destroy(thiefEnemy);
        }
    }

    /// <summary>
    /// Disable thief after forgiveness animation completes
    /// </summary>
    private System.Collections.IEnumerator DisableThiefAfterForgivenessAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (thiefEnemy != null)
        {
            Debug.Log("<color=green>Ladrón se va después de ser perdonado</color>");
            thiefEnemy.SetActive(false);
        }
    }

    /// <summary>
    /// Fallback method to destroy thief after delay
    /// </summary>
    private System.Collections.IEnumerator DestroyThiefAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (thiefEnemy != null)
        {
            Debug.Log("<color=red>Ladrón destruido (fallback)</color>");
            Destroy(thiefEnemy);
        }
    }

    /// <summary>
    /// Fallback method to disable thief after delay
    /// </summary>
    private System.Collections.IEnumerator DisableThiefAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (thiefEnemy != null)
        {
            Debug.Log("<color=green>Ladrón desactivado (fallback)</color>");
            thiefEnemy.SetActive(false);
        }
    }

    /// <summary>
    /// Execute boss death animation using BattleCharacter
    /// </summary>
    private void ExecuteBossDeath()
    {
        if (bossEnemy != null)
        {
            // Find BattleCharacter on the boss enemy
            BattleCharacter bossCharacter = bossEnemy.GetComponent<BattleCharacter>();
            if (bossCharacter != null)
            {
                Debug.Log("<color=red>Ejecutando muerte del jefe...</color>");
                
                // Force death animation
                if (bossCharacter.Animator != null)
                {
                    bossCharacter.Animator.Play("Asesinado", 0);
                }
                
                // Destroy after delay to show animation
                StartCoroutine(DestroyBossAfterDeathAnimation(3f));
            }
            else
            {
                Debug.LogWarning("BattleCharacter not found on boss enemy!");
                // Fallback: destroy directly after delay
                StartCoroutine(DestroyBossAfterDelay(2f));
            }
        }
    }

    /// <summary>
    /// Execute boss forgiveness animation using BattleCharacter
    /// </summary>
    private void ExecuteBossForgiveness()
    {
        if (bossEnemy != null)
        {
            // Find BattleCharacter on the boss enemy
            BattleCharacter bossCharacter = bossEnemy.GetComponent<BattleCharacter>();
            if (bossCharacter != null)
            {
                Debug.Log("<color=green>Ejecutando perdón del jefe...</color>");
                
                // Use forgiveness animation (boss leaves)
                if (bossCharacter.Animator != null)
                {
                    bossCharacter.Animator.Play("Perdonado", 0);
                }
                
                // Disable after delay to show animation
                StartCoroutine(DisableBossAfterForgivenessAnimation(3f));
            }
            else
            {
                Debug.LogWarning("BattleCharacter not found on boss enemy!");
                // Fallback: disable directly after delay
                StartCoroutine(DisableBossAfterDelay(2f));
            }
        }
    }

    /// <summary>
    /// Destroy boss after death animation completes
    /// </summary>
    private System.Collections.IEnumerator DestroyBossAfterDeathAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossEnemy != null)
        {
            Debug.Log("<color=red>Jefe eliminado después de animación de muerte</color>");
            Destroy(bossEnemy);
        }
    }

    /// <summary>
    /// Disable boss after forgiveness animation completes
    /// </summary>
    private System.Collections.IEnumerator DisableBossAfterForgivenessAnimation(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossEnemy != null)
        {
            Debug.Log("<color=green>Jefe se va después de ser perdonado</color>");
            bossEnemy.SetActive(false);
        }
    }

    /// <summary>
    /// Fallback method to destroy boss after delay
    /// </summary>
    private System.Collections.IEnumerator DestroyBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossEnemy != null)
        {
            Debug.Log("<color=red>Jefe destruido (fallback)</color>");
            Destroy(bossEnemy);
        }
    }

    /// <summary>
    /// Fallback method to disable boss after delay
    /// </summary>
    private System.Collections.IEnumerator DisableBossAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossEnemy != null)
        {
            Debug.Log("<color=green>Jefe desactivado (fallback)</color>");
            bossEnemy.SetActive(false);
        }
    }

}