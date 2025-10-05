using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Choice
{
    [TextArea(3, 5)]
    public string choiceText;
    
    public int karmaEffect;
    
    [Header("Sistema de Eventos (Elige uno)")]
    [Tooltip("ID único para esta elección. Usa esto para el ChoiceEventSystem (RECOMENDADO).")]
    public string choiceId;
    
    [Space(10)]
    [Tooltip("UnityEvent tradicional (OPCIONAL - Solo si no usas choiceId). No funcionará con referencias de escena desde ScriptableObjects.")]
    public UnityEvent onChoiceSelected; 
    
    public bool endInteractionOnSelect = true;

    /// <summary>
    /// Ejecuta esta elección. Primero intenta usar el ChoiceEventSystem,
    /// si no tiene choiceId, intenta invocar el UnityEvent.
    /// </summary>
    public void Execute()
    {
        // Prioridad 1: Usar el ChoiceEventSystem si hay un choiceId asignado
        if (!string.IsNullOrEmpty(choiceId))
        {
            if (ChoiceEventSystem.Instance != null)
            {
                ChoiceEventSystem.Instance.InvokeChoice(choiceId);
            }
            else
            {
                Debug.LogError("ChoiceEventSystem.Instance es null. ¿Olvidaste añadirlo a la escena?");
            }
        }
        // Prioridad 2: Usar UnityEvent como fallback (para compatibilidad)
        else if (onChoiceSelected != null && onChoiceSelected.GetPersistentEventCount() > 0)
        {
            onChoiceSelected.Invoke();
        }
        else
        {
            Debug.LogWarning($"La elección '{choiceText}' no tiene ni choiceId ni UnityEvent configurado.");
        }
    }
}