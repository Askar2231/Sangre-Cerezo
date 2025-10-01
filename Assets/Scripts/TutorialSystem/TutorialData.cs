using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define un tutorial completo
/// </summary>
[CreateAssetMenu(fileName = "New Tutorial", menuName = "Tutorial System/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("ID único del tutorial (ej: 'movement_basic')")]
    public string tutorialId;

    [Header("Configuración General")]
    [Tooltip("Tipo de tutorial a mostrar")]
    public TutorialType tutorialType;

    [Tooltip("Mostrar solo una vez")]
    public bool showOnlyOnce = true;

    [Tooltip("Pausar el juego durante el tutorial")]
    public bool pauseGame = true;


    [Header("Configuración de Diálogo Simple")]
    [Tooltip("Solo para TutorialType.SimpleDialog")]
    [TextArea(2, 4)]
    public string dialogText;

    [Tooltip("Posición del diálogo en pantalla")]
    public DialogPosition dialogPosition = DialogPosition.TopCenter;

    [Tooltip("Duración de visualización (0 = cerrar manualmente)")]
    public float displayDuration = 5.0f;

    [Header("Configuración de Tarjetas")]
    [Tooltip("Solo para TutorialType.CardPopover")]
    public List<TutorialCard> cards = new List<TutorialCard>();

    private void OnValidate()
    {
        // Validación en el editor
        if (string.IsNullOrEmpty(tutorialId))
        {
            Debug.LogWarning($"Tutorial '{name}' no tiene un tutorialId asignado!", this);
        }

        if (tutorialType == TutorialType.SimpleDialog && string.IsNullOrEmpty(dialogText))
        {
            Debug.LogWarning($"Tutorial '{name}' es de tipo SimpleDialog pero no tiene texto!", this);
        }

        if (tutorialType == TutorialType.CardPopover && (cards == null || cards.Count == 0))
        {
            Debug.LogWarning($"Tutorial '{name}' es de tipo CardPopover pero no tiene tarjetas!", this);
        }
    }
}

