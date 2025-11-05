using UnityEngine;
using System; 

public enum RobberyQuestState
{
    NotStarted,
    DialogueIntro,
    CombatActive,
    PostCombatDecision,
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
        
        // Call existing combat end logic
        bool playerWon = (result == BattleResult.PlayerVictory);
        OnThiefCombatEnd(playerWon);
    }

    public void PlayerForgivenThief()
    {
        if (currentQuestState == RobberyQuestState.PostCombatDecision)
        {
            InteractionManager.Instance.EndInteraction();
            UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            Debug.Log("Ladrón perdonado. Vuelve con el Mercader.");
        }
    }

    public void PlayerKilledThief()
    {
        if (currentQuestState == RobberyQuestState.PostCombatDecision)
        {
            InteractionManager.Instance.EndInteraction();
            UpdateQuestState(RobberyQuestState.ReturnToMerchant);
            Debug.Log("Ladrón eliminado. Vuelve con el Mercader.");
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
    
}