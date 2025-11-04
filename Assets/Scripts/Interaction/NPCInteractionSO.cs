using UnityEngine;

public class NPCInteractionSO : MonoBehaviour
{
    [Tooltip("Arrastra aquí el ScriptableObject de la misión relacionada")]
    public QuestData questData;

    public void TriggerInteraction()
    {
        if (questData != null)
        {
            QuestManagerSO.Instance.TryCompleteQuest(questData.conditionType, questData.conditionTargetId);
        }
        // Aquí puedes agregar lógica adicional de interacción (diálogo, animaciones, etc.)
    }
}