using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Estatua interactiva que muestra lore usando el sistema de diálogos existente
/// VERSION SIN PARPADEO Y CON MÚLTIPLES INTERACCIONES
/// </summary>
public class InteractableStatue : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private StatueLoreData loreData;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private float exitRange = 3.5f;
    [SerializeField] private bool allowMultipleInteractions = true; // ← NUEVO: Permitir varias interacciones
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("DEBUG")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Transform playerTransform;
    private bool playerInRange = false;
    private bool isShowingDialog = false;
    private int updateCount = 0;
    private bool hasInteractedThisSession = false; // ← Cambiado el nombre para claridad
    private bool promptCurrentlyActive = false; // ← NUEVO: Controlar estado del prompt

    private void Start()
    {
        if (showDebugLogs) Debug.Log("🗿 === STATUE START ===");
        
        // Verificar lore data
        if (showDebugLogs)
        {
            Debug.Log($"🗿 Lore Data asignado: {loreData != null}");
            if (loreData != null)
            {
                Debug.Log($"🗿 Lore Data nombre: {loreData.statueName}");
            }
        }
        
        // Configurar prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
            promptCurrentlyActive = false;
            if (showDebugLogs) Debug.Log("🗿 Interaction Prompt desactivado");
            
            // Buscar el TextMeshProUGUI si no está asignado
            if (promptText == null)
            {
                promptText = interactionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            }
        }
        else
        {
            if (showDebugLogs) Debug.Log("⚠️ Interaction Prompt NO asignado");
        }
        
        // Buscar jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            if (showDebugLogs) Debug.Log($"✅ Jugador encontrado: {player.name}");
        }
        else
        {
            Debug.LogError("❌ NO SE ENCONTRÓ JUGADOR CON TAG 'Player'");
        }
    }

    private void Update()
    {
        // Log cada 60 frames (1 vez por segundo aprox)
        updateCount++;
        if (showDebugLogs && updateCount % 60 == 0)
        {
            Debug.Log($"🔄 Update #{updateCount} - Player in range: {playerInRange} - Prompt active: {promptCurrentlyActive} - Showing dialog: {isShowingDialog}");
        }
        
        if (playerTransform == null)
        {
            if (showDebugLogs && updateCount == 1)
            {
                Debug.LogError("❌ playerTransform es NULL - no se puede detectar distancia");
            }
            return;
        }
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Usar hysteresis para evitar parpadeo
        if (!playerInRange)
        {
            // Si NO está en rango, usar el rango de entrada (más pequeño)
            if (distance <= interactionRange)
            {
                OnPlayerEnterRange();
            }
        }
        else
        {
            // Si YA está en rango, usar el rango de salida (más grande)
            if (distance > exitRange)
            {
                OnPlayerExitRange();
            }
        }
        
        // Solo procesar input si el jugador está en rango Y no está mostrando diálogo
        if (playerInRange && !isShowingDialog)
        {
            // Asegurarse de que el prompt esté visible
            ShowPrompt();
            
            // DETECTAR INPUT - KEYBOARD
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (showDebugLogs) Debug.Log("🔑 === E KEY DETECTADA ===");
                    InteractWithStatue();
                }
            }
            
            // DETECTAR INPUT - GAMEPAD
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonWest.wasPressedThisFrame) // X en Xbox, Square en PS
                {
                    if (showDebugLogs) Debug.Log("🎮 === GAMEPAD X/SQUARE DETECTADO ===");
                    InteractWithStatue();
                }
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        if (playerInRange) return; // Ya está en rango, no hacer nada
        
        playerInRange = true;
        
        if (showDebugLogs) 
        {
            Debug.Log($"✅ === JUGADOR ENTRÓ EN RANGO === Estatua: {loreData?.statueName ?? "sin nombre"}");
        }
        
        // Mostrar el prompt solo si no está mostrando diálogo
        if (!isShowingDialog)
        {
            ShowPrompt();
        }
    }

    private void OnPlayerExitRange()
    {
        if (!playerInRange) return; // Ya está fuera de rango, no hacer nada
        
        playerInRange = false;
        
        if (showDebugLogs) Debug.Log("❌ Jugador salió de rango");
        
        HidePrompt();
    }

    /// <summary>
    /// Muestra el prompt solo si no está ya visible
    /// </summary>
    private void ShowPrompt()
    {
        if (interactionPrompt == null) return;
        
        // Solo activar si no está ya activo (evita parpadeo)
        if (!promptCurrentlyActive)
        {
            UpdatePromptText();
            interactionPrompt.SetActive(true);
            promptCurrentlyActive = true;
            if (showDebugLogs) Debug.Log("👁️ Prompt mostrado");
        }
    }

    /// <summary>
    /// Oculta el prompt solo si está visible
    /// </summary>
    private void HidePrompt()
    {
        if (interactionPrompt == null) return;
        
        // Solo desactivar si está activo (evita llamadas innecesarias)
        if (promptCurrentlyActive)
        {
            interactionPrompt.SetActive(false);
            promptCurrentlyActive = false;
            if (showDebugLogs) Debug.Log("🚫 Prompt ocultado");
        }
    }

    /// <summary>
    /// Actualiza el texto del prompt con el botón correcto según el dispositivo actual
    /// </summary>
    private void UpdatePromptText()
    {
        if (promptText == null) return;
        
        string buttonIcon = GetInteractionButtonText();
        promptText.text = $"{buttonIcon} Interactuar";
    }

    /// <summary>
    /// Obtiene el texto/icono del botón de interacción según el dispositivo actual
    /// </summary>
    private string GetInteractionButtonText()
    {
        // Intentar usar InputIconMapper si está disponible
        if (InputIconMapper.Instance != null)
        {
            return InputIconMapper.Instance.GetSpriteOrText(InputAction.Interact);
        }
        
        // Fallback: detectar manualmente
        if (Gamepad.current != null)
        {
            return "[X]"; // Botón X en Xbox (buttonWest)
        }
        else
        {
            return "[E]";
        }
    }

    void InteractWithStatue()
    {
        Debug.Log("🗿 === INTERACT WITH STATUE LLAMADO ===");

        if (loreData == null)
        {
            Debug.LogError("❌ No hay Lore Data asignado en la estatua!");
            return;
        }

        // Verificar si ya interactuó y no se permiten múltiples interacciones
        if (!allowMultipleInteractions && hasInteractedThisSession)
        {
            Debug.Log("⚠️ Ya interactuaste con esta estatua (múltiples interacciones deshabilitadas)");
            return;
        }

        hasInteractedThisSession = true;

        Debug.Log($"🗿 Mostrando diálogo de: {loreData.statueName}");
        Debug.Log($"📝 Texto: {loreData.loreText}");

        // Ocultar el prompt mientras se muestra el diálogo
        HidePrompt();
        
        isShowingDialog = true;

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowSimpleText(loreData.statueName, loreData.loreText);
            Debug.Log("✅ ShowSimpleText llamado");
        }
        else
        {
            Debug.LogError("❌ InteractionManager.Instance es null!");
            isShowingDialog = false; // Resetear si falla
        }
    }

    /// <summary>
    /// Llamar este método cuando el InteractionManager cierre el diálogo
    /// </summary>
    public void OnDialogClosed()
    {
        if (showDebugLogs) Debug.Log("🚪 Diálogo cerrado - Reseteando estado");
        
        isShowingDialog = false;
        
        // Si se permiten múltiples interacciones, resetear el flag
        if (allowMultipleInteractions)
        {
            hasInteractedThisSession = false;
        }
        
        // Si el jugador sigue en rango, volver a mostrar el prompt
        if (playerInRange)
        {
            ShowPrompt();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        // Mostrar también el rango de salida
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, exitRange);
    }
    
    // MÉTODO PÚBLICO PARA LLAMAR DESDE OTRO SCRIPT
    public void TriggerInteraction()
    {
        if (showDebugLogs) Debug.Log("🎯 TriggerInteraction llamado desde script externo");
        
        if (playerInRange && !isShowingDialog)
        {
            InteractWithStatue();
        }
        else
        {
            Debug.LogWarning("⚠️ TriggerInteraction: jugador no está en rango o ya hay diálogo activo");
        }
    }
}