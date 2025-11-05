using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ObjectiveText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private QuestManager questManager; // ← Asignable desde el Inspector

    private void OnEnable()
    {
        TrySubscribeQuestManager();
        UpdateObjectiveFallback();
    }

    private void OnDisable()
    {
        UnsubscribeQuestManager();
    }

    private void TrySubscribeQuestManager()
    {
        // Usar el asignado en el Inspector, si no, buscar en la escena
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        if (questManager != null)
        {
            QuestManager.OnQuestStateChanged += UpdateObjectiveFromRobberyQuest;
            // Actualiza el texto al estado actual
            UpdateObjectiveFromRobberyQuest(questManager.GetCurrentQuestState());
        }
        else
        {
            // Si no existe, muestra el objetivo inicial
            UpdateObjectiveFallback();
        }
    }

    private void UnsubscribeQuestManager()
    {
        QuestManager.OnQuestStateChanged -= UpdateObjectiveFromRobberyQuest;
    }

    private void UpdateObjectiveFromRobberyQuest(RobberyQuestState state)
    {
        // Hide objective text during combat
        if (state == RobberyQuestState.CombatActive || state == RobberyQuestState.BossCombatActive)
        {
            objectiveText.gameObject.SetActive(false);
            Debug.Log("<color=yellow>Objective text hidden during combat</color>");
            return;
        }
        
        // Show objective text for non-combat states
        if (!objectiveText.gameObject.activeSelf)
        {
            objectiveText.gameObject.SetActive(true);
            Debug.Log("<color=green>Objective text shown</color>");
        }
        
        switch (state)
        {
            case RobberyQuestState.NotStarted:
                UpdateObjectiveFallback();
                break;
            case RobberyQuestState.DialogueIntro:
                objectiveText.text = "Habla con el mercader.";
                break;
            case RobberyQuestState.PostCombatDecision:
                objectiveText.text = "Decide el destino del ladrón.";
                break;
            case RobberyQuestState.FindBoss:
                objectiveText.text = "Encuentra al jefe de los ladrones.";
                break;
            case RobberyQuestState.PostBossDecision:
                objectiveText.text = "Decide el destino del jefe.";
                break;
            case RobberyQuestState.ReturnToMerchant:
                objectiveText.text = "Regresa con el mercader.";
                break;
            case RobberyQuestState.Completed:
                objectiveText.text = "¡Misión completada!";
                break;
            case RobberyQuestState.TalkToFirstNPC:
                objectiveText.text = "Habla con el primer NPC.";
                break;
            default:
                UpdateObjectiveFallback();
                break;
        }
    }

    private void UpdateObjectiveFallback()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Exterior")
        {
            objectiveText.text = "Busca al mercader.";
        }
        else
        {
            objectiveText.text = "Sal de la casa.";
        }
    }

    // Llama esto al cargar/cambiar de escena para reintentar la suscripción
    public void RefreshQuestManagerReference()
    {
        UnsubscribeQuestManager();
        TrySubscribeQuestManager();
    }
}
