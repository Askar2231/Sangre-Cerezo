using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Tooltip("Arrastra aquí el Scriptable Object de Conversación o Decisión para este NPC")]
    public ScriptableObject interactionData;

    [Header("Configuración Especial (Opcional)")]
    [Tooltip("Marca esto si este NPC es el mercader de la misión del robo")]
    public bool isMerchantQuest = false;

    public void TriggerInteraction()
    {
        // Si es el mercader de la misión, usar lógica especial
        if (isMerchantQuest && QuestManager.Instance != null)
        {
            QuestManager.Instance.StartFinalMerchantDialogue();
        }
        else
        {
            // Para cualquier otro NPC, usar el comportamiento normal
            InteractionManager.Instance.StartInteraction(interactionData);
        }
    }
}