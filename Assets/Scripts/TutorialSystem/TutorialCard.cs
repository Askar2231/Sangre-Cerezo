using UnityEngine;

/// <summary>
/// Representa una tarjeta individual dentro de un tutorial de tipo CardPopover
/// </summary>
[System.Serializable]
public class TutorialCard
{
    [Header("Contenido Visual")]
    [Tooltip("Imagen opcional para mostrar en la tarjeta")]
    public Sprite cardImage;

    [Header("Contenido de Texto")]
    [Tooltip("Texto de la tarjeta. Usa {Move}, {Run}, {QTE}, etc. para iconos de input")]
    [TextArea(3, 6)]
    public string cardText;

    [Header("Configuración de Tiempo")]
    [Tooltip("Tiempo mínimo antes de poder avanzar (segundos)")]
    public float minDisplayTime = 1.0f;

    [Tooltip("Requiere confirmación del jugador para continuar")]
    public bool requireConfirmation = true;
}

