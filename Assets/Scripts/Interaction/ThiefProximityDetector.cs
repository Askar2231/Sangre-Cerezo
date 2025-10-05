using UnityEngine;

/// <summary>
/// Script para detectar cuando el jugador se acerca al ladrón e iniciar el combate.
/// Adjunta este script al prefab del ladrón.
/// </summary>
public class ThiefProximityDetector : MonoBehaviour
{
    [Header("Configuración de Detección")]
    [Tooltip("Distancia a la que el jugador debe estar para iniciar el combate")]
    [SerializeField] private float detectionRadius = 3f;
    
    [Tooltip("Tag del jugador (normalmente 'Player')")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Visual (Opcional)")]
    [Tooltip("Mostrar el rango de detección en el editor")]
    [SerializeField] private bool showDetectionGizmo = true;
    
    private bool combatInitiated = false;
    private Transform playerTransform;

    private void Start()
{
    Debug.Log("<color=magenta>ThiefProximityDetector.Start() ejecutándose...</color>");
    
    // Buscar al jugador en la escena
    GameObject player = GameObject.FindGameObjectWithTag(playerTag);
    if (player != null)
    {
        playerTransform = player.transform;
        Debug.Log($"<color=green>Jugador encontrado: {player.name}</color>");
    }
    else
    {
        Debug.LogError($"No se encontró ningún GameObject con el tag '{playerTag}'. Asegúrate de que el jugador tenga ese tag.");
    }
}

    private void Update()
{
    // Si ya se inició el combate, no hacer nada
    if (combatInitiated) return;
    
    // Verificar si se encontró al jugador
    if (playerTransform == null)
    {
        Debug.LogWarning("playerTransform es null en el ladrón");
        return;
    }

    // Calcular distancia entre el ladrón y el jugador
    float distance = Vector3.Distance(transform.position, playerTransform.position);
    
    // Debug continuo para ver la distancia
    Debug.Log($"Distancia al jugador: {distance:F2} / Detección en: {detectionRadius}");

    // Si el jugador está dentro del rango de detección
    if (distance <= detectionRadius)
    {
        Debug.Log("<color=yellow>¡Jugador dentro del rango!</color>");
        InitiateCombat();
    }
}    private void InitiateCombat()
    {
        combatInitiated = true;
        Debug.Log("<color=red>¡El jugador se acercó al ladrón! Iniciando combate...</color>");
        
        // Notificar al QuestManager que debe iniciar el combate
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.StartThiefCombat();
        }
        else
        {
            Debug.LogError("QuestManager.Instance es null. No se puede iniciar el combate.");
        }
    }

    /// <summary>
    /// Método alternativo usando Trigger Collider en lugar de Update.
    /// Si prefieres usar colliders, activa esta opción.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!combatInitiated && other.CompareTag(playerTag))
        {
            InitiateCombat();
        }
    }

    /// <summary>
    /// Para 2D, usa OnTriggerEnter2D en su lugar.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!combatInitiated && other.CompareTag(playerTag))
        {
            InitiateCombat();
        }
    }

    /// <summary>
    /// Dibuja el rango de detección en el editor (solo en Scene view).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (showDetectionGizmo)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Rojo semi-transparente
            Gizmos.DrawSphere(transform.position, detectionRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}