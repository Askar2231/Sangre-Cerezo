using UnityEngine;

/// <summary>
/// Handles battle initiation when player contacts enemy
/// </summary>
public class BattleTrigger : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] private BattleManagerV2 battleManager;
    [SerializeField] private float playerRetreatDistance = 2f; // REDUCIR de 5f a 2f para jugador pequeño
    
    [Header("References")]
    [SerializeField] private Transform playerTransform; // El transform del jugador
    [SerializeField] private Transform enemyTransform; // El transform del enemigo
    
    [Header("UI")]
    [SerializeField] private GameObject battleUICanvas; // Canvas de UI de combate
    
    private bool battleTriggered = false;
    
    private void Awake()
    {
        // Si no se asignan manualmente, buscar automáticamente
        if (playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerBattleController>();
            if (playerController != null)
                playerTransform = playerController.transform;
        }
        
        if (enemyTransform == null)
        {
            var enemyController = FindFirstObjectByType<EnemyBattleController>();
            if (enemyController != null)
                enemyTransform = enemyController.transform;
        }
        
        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManagerV2>();
        
        // Asegurar que el canvas de batalla esté desactivado al inicio
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y si la batalla no ha sido activada
        if (battleTriggered) return;
        
        if (other.CompareTag("Player") || other.GetComponent<PlayerBattleController>() != null)
        {
            Debug.Log("Player contacted enemy! Starting battle...");
            StartBattleSequence();
        }
    }
    
    /// <summary>
    /// Inicia la secuencia de batalla con posicionamiento
    /// </summary>
    private void StartBattleSequence()
    {
        battleTriggered = true;
        
        // Activar UI de combate inmediatamente
        ActivateBattleUI();
        
        // Posicionar jugador y enemigo para el combate
        PositionCharactersForBattle();
        
        // Iniciar batalla después de un breve delay
        Invoke(nameof(InitiateBattle), 0.5f);
    }
    
    /// <summary>
    /// Posiciona al jugador y enemigo para el combate
    /// </summary>
    private void PositionCharactersForBattle()
    {
        if (playerTransform == null || enemyTransform == null)
        {
            Debug.LogWarning("Player or Enemy transform not assigned!");
            return;
        }
        
        // Obtener la dirección hacia donde está mirando el enemigo
        Vector3 enemyForward = enemyTransform.forward;
        
        // Solo usar componentes horizontales (X, Z) - ignorar Y
        enemyForward.y = 0f;
        enemyForward = enemyForward.normalized;
        
        // Posicionar al jugador 30 unidades EN LA DIRECCIÓN que mira el enemigo
        Vector3 playerBattlePosition = enemyTransform.position + (enemyForward * 30f);
        
        // Mantener la Y original del jugador
        playerBattlePosition.y = playerTransform.position.y;
        
        // Aplicar nueva posición
        playerTransform.position = playerBattlePosition;
        
        // Hacer que ambos se miren mutuamente
        Vector3 lookTarget = new Vector3(enemyTransform.position.x, playerTransform.position.y, enemyTransform.position.z);
        playerTransform.LookAt(lookTarget);
        
        Vector3 enemyLookTarget = new Vector3(playerTransform.position.x, enemyTransform.position.y, playerTransform.position.z);
        enemyTransform.LookAt(enemyLookTarget);
        
        Debug.Log($"Player positioned 30 units in enemy's forward direction. New position: {playerTransform.position}");
        Debug.Log($"Enemy position: {enemyTransform.position}");
        Debug.Log($"Enemy forward direction: {enemyForward}");
    }
    
    /// <summary>
    /// Activa la UI de combate
    /// </summary>
    private void ActivateBattleUI()
    {
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(true);
            Debug.Log("Battle UI Canvas activated");
        }
        else
        {
            Debug.LogWarning("Battle UI Canvas not assigned!");
        }
    }
    
    /// <summary>
    /// Desactiva la UI de combate
    /// </summary>
    private void DeactivateBattleUI()
    {
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
            Debug.Log("Battle UI Canvas deactivated");
        }
    }
    
    /// <summary>
    /// Inicia la batalla en el BattleManager
    /// </summary>
    private void InitiateBattle()
    {
        if (battleManager != null)
        {
            // Suscribirse al evento de fin de batalla para desactivar UI
            battleManager.OnBattleEnded += HandleBattleEnded;
            
            battleManager.StartBattle();
        }
        else
        {
            Debug.LogError("BattleManager not found! Cannot start battle.");
        }
    }
    
    /// <summary>
    /// Maneja el fin de la batalla
    /// </summary>
    private void HandleBattleEnded(BattleResult result)
    {
        Debug.Log($"Battle ended with result: {result}");
        
        // Desactivar UI de combate
        DeactivateBattleUI();
        
        // Desuscribirse del evento
        if (battleManager != null)
        {
            battleManager.OnBattleEnded -= HandleBattleEnded;
        }
        
        // Si el jugador ganó, destruir el trigger para evitar futuras activaciones
        if (result == BattleResult.PlayerVictory)
        {
            Debug.Log("Player victory! Destroying battle trigger...");
            Destroy(gameObject); // Destruir el trigger también
        }
        else
        {
            // Si el jugador perdió, resetear trigger después de un delay
            Invoke(nameof(ResetTrigger), 2f);
        }
    }

    private void OnDestroy()
    
    {
        // Limpiar suscripciones al destruir
        if (battleManager != null)
        {
            battleManager.OnBattleEnded -= HandleBattleEnded;
        }
    }
    
    /// <summary>
    /// Resetea el trigger para permitir nuevas batallas
    /// </summary>
    public void ResetTrigger()
    {
        battleTriggered = false;
    }
    
}
