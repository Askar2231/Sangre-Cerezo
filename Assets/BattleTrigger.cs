using UnityEngine;

/// <summary>
/// Handles battle initiation when player contacts enemy
/// </summary>
public class BattleTrigger : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] private BattleManagerV2 battleManager;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform; // El transform del jugador
    [SerializeField] private Transform enemyTransform; // El transform del enemigo (ESTE GameObject)
    
    [Header("UI")]
    [SerializeField] private GameObject battleUICanvas; // Canvas de UI de combate
    
    private bool battleTriggered = false;
    private EnemyBattleController thisEnemyController; // Controller de ESTE enemigo
    
    private void Awake()
    {
        Debug.Log($"BattleTrigger Awake on: {gameObject.name}");
        
        // Si no se asignan manualmente, buscar automáticamente
        if (playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerBattleController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                Debug.Log($"Auto-found player: {playerTransform.name}");
            }
        }
        
        // IMPORTANTE: Obtener el EnemyBattleController de ESTE GameObject
        thisEnemyController = GetComponent<EnemyBattleController>();
        if (thisEnemyController == null)
        {
            // Si no está en este GameObject, buscar en el padre
            thisEnemyController = GetComponentInParent<EnemyBattleController>();
        }
        
        // El enemyTransform es ESTE GameObject
        if (enemyTransform == null)
            enemyTransform = transform;
        
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManagerV2>();
            if (battleManager != null)
            {
                Debug.Log($"Auto-found battle manager: {battleManager.name}");
            }
        }
        
        // Asegurar que el canvas de batalla esté desactivado al inicio
        if (battleUICanvas != null)
        {
            battleUICanvas.SetActive(false);
        }
        
        // Validar que encontramos el EnemyBattleController
        if (thisEnemyController == null)
        {
            Debug.LogError($"No EnemyBattleController found on {gameObject.name} or its parent! This trigger won't work.");
        }
        else
        {
            Debug.Log($"Found EnemyBattleController: {thisEnemyController.name} on {gameObject.name}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y si la batalla no ha sido activada
        if (battleTriggered) return;
        
        if (other.CompareTag("Player") || other.GetComponent<PlayerBattleController>() != null)
        {
            Debug.Log($"Player contacted enemy: {gameObject.name}! Starting battle...");
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
        if (battleManager != null && thisEnemyController != null)
        {
            // Suscribirse al evento de fin de batalla para desactivar UI
            battleManager.OnBattleEnded += HandleBattleEnded;
            
            // PASAR EL ENEMIGO ESPECÍFICO al BattleManager
            battleManager.StartBattleWithEnemy(thisEnemyController);
        }
        else
        {
            Debug.LogError("BattleManager or EnemyBattleController not found! Cannot start battle.");
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
        
        // Si el jugador ganó, destruir el enemigo y trigger
        if (result == BattleResult.PlayerVictory)
        {
            Debug.Log("Player victory! Cleaning up...");
            
            // FORZAR reset del BattleManager
            if (battleManager != null)
            {
                battleManager.ForceReset();
            }
            
            // Destruir después de un delay para permitir efectos
            Invoke(nameof(DestroyAfterVictory), 1.5f);
        }
        else
        {
            // Si el jugador perdió, resetear para permitir reintento
            if (battleManager != null)
            {
                battleManager.ForceReset();
            }
            
            Invoke(nameof(ResetTrigger), 2f);
        }
    }

    /// <summary>
    /// Destruye el enemigo y trigger después de victoria
    /// </summary>
    private void DestroyAfterVictory()
    {
        Debug.Log("Destroying defeated enemy and trigger...");
        Destroy(gameObject); // Esto destruye todo el enemigo + trigger
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
        Debug.Log($"Trigger reset on: {gameObject.name}");
    }
}
