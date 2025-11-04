using UnityEngine;
using System.Collections.Generic;

public class QuestManagerSO : MonoBehaviour
{
    [SerializeField] private List<QuestData> allQuests;
    private Dictionary<string, bool> questCompletion = new Dictionary<string, bool>();

    public static QuestManagerSO Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TryCompleteQuest(QuestConditionType conditionType, string targetId)
    {
        foreach (var quest in allQuests)
        {
            if (!questCompletion.ContainsKey(quest.questId) || !questCompletion[quest.questId])
            {
                if (quest.conditionType == conditionType && quest.conditionTargetId == targetId)
                {
                    questCompletion[quest.questId] = true;
                    if (quest.enemyPrefabToActivate != null)
                        quest.enemyPrefabToActivate.SetActive(true);
                    Debug.Log($"Misión completada: {quest.questName}");
                    // Aquí puedes disparar evento de misión completada y actualizar UI
                }
            }
        }
    }

    public QuestData GetActiveQuest()
    {
        foreach (var quest in allQuests)
        {
            if (!questCompletion.ContainsKey(quest.questId) || !questCompletion[quest.questId])
                return quest;
        }
        return null;
    }
}