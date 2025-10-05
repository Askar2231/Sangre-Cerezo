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
    
    public static event Action<RobberyQuestState> OnQuestStateChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // Registrar callbacks inmediatamente después de inicializar la instancia
        if (Instance == this)
        {
            Invoke(nameof(RegisterChoiceCallbacks), 0.2f);
        }
    }

    private void OnDestroy()
    {
        UnregisterChoiceCallbacks();
    }

    /// <summary>
    /// Registra todos los callbacks de las elecciones en el ChoiceEventSystem.
    /// Los IDs deben coincidir con los que configures en tus ScriptableObjects.
    /// </summary>
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

    /// <summary>
    /// Desregistra los callbacks cuando se destruye el QuestManager.
    /// </summary>
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

    // --- Métodos de Control de Estado de la Misión ---

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

    /// <summary>
    /// Spawnea al ladrón y añade automáticamente el detector de proximidad.
    /// El combate se iniciará cuando el jugador se acerque.
    /// </summary>
    private void SpawnThiefAndStartCombat()
    {
        if (thiefPrefab != null && thiefSpawnPoint != null)
        {
            spawnedThief = Instantiate(thiefPrefab, thiefSpawnPoint.position, thiefSpawnPoint.rotation);
            
            // Añadir el detector de proximidad si no lo tiene
            ThiefProximityDetector detector = spawnedThief.GetComponent<ThiefProximityDetector>();
            if (detector == null)
            {
                detector = spawnedThief.AddComponent<ThiefProximityDetector>();
                Debug.Log("<color=cyan>ThiefProximityDetector añadido automáticamente al ladrón.</color>");
            }
            
            Debug.Log("¡Ladrón aparecido! Acércate para iniciar el combate.");
        }
        else
        {
            Debug.LogError("Prefab del ladrón o punto de spawn no asignados en QuestManager.");
            UpdateQuestState(RobberyQuestState.NotStarted);
        }
    }

    /// <summary>
    /// Llama a este método desde el script del ladrón cuando el jugador se acerque.
    /// </summary>
    public void StartThiefCombat()
    {
        Debug.Log($"<color=cyan>StartThiefCombat llamado. Estado actual: {currentQuestState}</color>");
        
        if (currentQuestState == RobberyQuestState.CombatActive)
        {
            Debug.Log("¡Iniciando combate con el ladrón!");
            // TODO: Aquí llamarías a tu CombatManager para iniciar el combate real
            // CombatManager.Instance.StartCombat(spawnedThief);
            
            // Simulación temporal (elimina esto cuando tengas tu CombatManager real)
            Invoke("SimulateCombatWin", 2f);
        }
        else
        {
            Debug.LogWarning($"No se puede iniciar combate. Estado actual: {currentQuestState}, se esperaba: CombatActive");
        }
    }

    /// <summary>
    /// Simulación de victoria en combate (REEMPLAZAR por la lógica real de tu CombatManager).
    /// </summary>
    private void SimulateCombatWin()
    {
        OnThiefCombatEnd(true);
    }

    /// <summary>
    /// Maneja el final del combate con el ladrón. Recibe si el jugador ganó o perdió.
    /// (Normalmente llamado por tu CombatManager).
    /// </summary>
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

    /// <summary>
    /// Inicia el diálogo final con el mercader. 
    /// Llama a este método desde el NPCInteraction del mercader cuando el jugador interactúe con él.
    /// </summary>
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