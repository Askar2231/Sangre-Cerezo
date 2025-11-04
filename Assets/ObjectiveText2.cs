using UnityEngine;
using TMPro;

public class ObjectiveText2 : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;

    private void OnEnable()
    {
        UpdateObjective();
    }

    public void UpdateObjective()
    {
        var activeQuest = QuestManagerSO.Instance?.GetActiveQuest();
        if (activeQuest != null)
            objectiveText.text = activeQuest.objectiveText;
        else
            objectiveText.text = "Sal de la casa.";
    }
}