using UnityEngine;

/// <summary>
/// Contiene la información de lore para una estatua
/// </summary>
[CreateAssetMenu(fileName = "New Statue Lore", menuName = "Sangre y Cerezo/Statue Lore Data")]
public class StatueLoreData : ScriptableObject
{
    [Header("Statue Info")]
    [Tooltip("Nombre de la estatua (ej: 'Estatua del Mentor')")]
    public string statueName;
    
    [Header("Lore Text")]
    [TextArea(5, 15)]
    [Tooltip("El texto de lore que aparecerá al interactuar")]
    public string loreText;
    
    [Header("Optional: Voice/Audio")]
    [Tooltip("Clip de audio opcional para narración")]
    public AudioClip narratorVoice;
    
    [Header("Visual")]
    [Tooltip("Icono o imagen opcional para mostrar en el UI")]
    public Sprite statueIcon;
}