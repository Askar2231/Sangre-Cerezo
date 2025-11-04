using UnityEngine;

public enum QuestType { Principal, Secundaria }
public enum QuestConditionType { InteractWithNPC, KillEnemy, CollectItem, Custom }

[CreateAssetMenu(menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    public string questId;
    public string questName;
    [TextArea] public string description;
    [TextArea] public string objectiveText;
    public QuestType type;
    public QuestConditionType conditionType;
    public string conditionTargetId; // Ej: NPC id, Enemy id, Item id
    public GameObject enemyPrefabToActivate; // Opcional: enemigo a activar
    // Puedes agregar más campos según lo que necesites
}