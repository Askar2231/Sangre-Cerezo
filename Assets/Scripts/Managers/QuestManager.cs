using UnityEngine;
using System; 

public enum RobberyQuestState { NotStarted, DialogueIntro, CombatActive, PostCombatDecision, ReturnToMerchant, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Referencias de Assets y GameObjects")]
    [SerializeField] private Decision initialMerchantDialogue; 
    [SerializeField] private Decision thiefPostCombatDecision;     
    [SerializeField] private Decision finalMerchantDecision;       
    [SerializeField] private GameObject thiefPrefab;               
    [SerializeField] private Transform thiefSpawnPoint;            
    [SerializeField] private GameObject merchantNPC;               
    
    private RobberyQuestState currentQuestState = RobberyQuestState.NotStarted;
    private GameObject spawnedThief; 
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
        SpawnThiefAndStartCombat();
    }

    public void PlayerDeclinedHelp()
    {
        InteractionManager.Instance.EndInteraction();
        UpdateQuestState(RobberyQuestState.NotStarted);
        Debug.Log("Jugador rechazó ayudar al mercader.");
    }


   private void SpawnThiefAndStartCombat()
{
    if (thiefPrefab != null && thiefSpawnPoint != null)
    {
        spawnedThief = Instantiate(thiefPrefab, thiefSpawnPoint.position, thiefSpawnPoint.rotation);
        
        // Desactivar BattleTrigger hasta que el jugador se acerque
        BattleTrigger battleTrigger = spawnedThief.GetComponent<BattleTrigger>();
        if (battleTrigger != null)
        {
            battleTrigger.enabled = false;
        }
        
        // Añadir el detector de proximidad
        ThiefProximityDetector detector = spawnedThief.GetComponent<ThiefProximityDetector>();
        if (detector == null)
        {
            detector = spawnedThief.AddComponent<ThiefProximityDetector>();
            Debug.Log("<color=cyan>ThiefProximityDetector añadido automáticamente al ladrón.</color>");
        }
        
        // Find and subscribe to the BattleManagerV2 on the spawned thief
        activeBattleManager = spawnedThief.GetComponentInChildren<BattleManagerV2>();
        if (activeBattleManager != null)
        {
            activeBattleManager.OnBattleEnded += HandleThiefBattleEnded;
            Debug.Log("<color=green>Subscribed to thief battle events</color>");
        }
        else
        {
            Debug.LogError("BattleManagerV2 not found on spawned thief!");
        }
        
        Debug.Log("¡Ladrón aparecido! Acércate para iniciar el combate.");
    }
    else
    {
        Debug.LogError("Prefab del ladrón o punto de spawn no asignados en QuestManager.");
        UpdateQuestState(RobberyQuestState.NotStarted);
    }
}

  public void StartThiefCombat()
{
    Debug.Log($"<color=cyan>StartThiefCombat llamado. Estado actual: {currentQuestState}</color>");
    
    if (currentQuestState == RobberyQuestState.CombatActive && spawnedThief != null)
    {
        Debug.Log("¡Iniciando combate con el ladrón!");
        
        
    }
    else
    {
        Debug.LogWarning($"No se puede iniciar combate. Estado: {currentQuestState}, Thief: {spawnedThief != null}");
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
            if (spawnedThief != null) Destroy(spawnedThief);

            UpdateQuestState(RobberyQuestState.PostCombatDecision);
            InteractionManager.Instance.StartInteraction(thiefPostCombatDecision);
        }
        else
        {
            Debug.Log("Combate con el ladrón terminado. Derrota.");
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
            Debug.Log("<color=yellow>Mostrando diálogo inicial del mercader</color>");
            StartRobberyQuest();
        }
        else
        {
            Debug.LogWarning($"El jugador intentó hablar con el mercader en un estado incorrecto: {currentQuestState}");
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
}