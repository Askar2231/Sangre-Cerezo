using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Estatua interactiva que muestra lore usando el sistema de diálogos existente
/// VERSION FINAL - Solo detecta input para ABRIR, InteractionManager maneja el CIERRE
/// </summary>
public class InteractableStatue : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private StatueLoreData loreData;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2.5f;
    [SerializeField] private float exitRange = 4f;
    [SerializeField] private bool allowMultipleInteractions = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject statueInteractionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("DEBUG")]
    [SerializeField] private bool showDebugLogs = false;
    
    private Transform playerTransform;
    private bool playerInRange = false;
    private bool isShowingDialog = false;
    private bool hasInteractedThisSession = false;

    private void Start()
    {
        if (showDebugLogs) Debug.Log("🗿 === STATUE START ===");
        
        // Verificar lore data
        if (loreData == null)
        {
            Debug.LogError("❌ No hay Lore Data asignado en la estatua!");
        }
        else if (showDebugLogs)
        {
            Debug.Log($"🗿 Lore Data: {loreData.statueName}");
        }
        
        // Configurar prompt
        if (statueInteractionPrompt != null)
        {
            statueInteractionPrompt.SetActive(false);
            
            if (promptText == null)
            {
                promptText = statueInteractionPrompt.GetComponentInChildren<TextMeshProUGUI>();
            }
            
            if (showDebugLogs) Debug.Log("🗿 Statue Prompt configurado");
        }
        else
        {
            Debug.LogError("⚠️ Statue Interaction Prompt NO asignado!");
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
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Detectar entrada/salida de rango con hysteresis
        if (!playerInRange && distance <= interactionRange)
        {
            OnPlayerEnterRange();
        }
        else if (playerInRange && distance > exitRange)
        {
            OnPlayerExitRange();
        }
        
        // ✅ SOLO detectar input para ABRIR el diálogo
        // El CIERRE lo maneja el InteractionManager
        if (playerInRange && !isShowingDialog)
        {
            bool inputPressed = false;
            
            // KEYBOARD
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                inputPressed = true;
                if (showDebugLogs) Debug.Log("🔑 E KEY DETECTADA");
            }
            
            // GAMEPAD
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                inputPressed = true;
                if (showDebugLogs) Debug.Log("🎮 GAMEPAD X DETECTADO");
            }
            
            if (inputPressed)
            {
                InteractWithStatue();
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        playerInRange = true;
        
        if (showDebugLogs) Debug.Log($"✅ Jugador entró en rango de: {loreData?.statueName}");
        
        if (!isShowingDialog)
        {
            ShowPrompt();
        }
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        
        if (showDebugLogs) Debug.Log("❌ Jugador salió de rango");
        
        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (statueInteractionPrompt == null) return;
        
        if (!statueInteractionPrompt.activeSelf)
        {
            UpdatePromptText();
            statueInteractionPrompt.SetActive(true);
            
            if (showDebugLogs) Debug.Log("👁️ Statue prompt mostrado");
        }
    }

    private void HidePrompt()
    {
        if (statueInteractionPrompt == null) return;
        
        if (statueInteractionPrompt.activeSelf)
        {
            statueInteractionPrompt.SetActive(false);
            
            if (showDebugLogs) Debug.Log("🚫 Statue prompt ocultado");
        }
    }

    private void UpdatePromptText()
    {
        if (promptText == null) return;
        
        string buttonIcon = GetInteractionButtonText();
        promptText.text = $"{buttonIcon} Leer";
    }

    private string GetInteractionButtonText()
    {
        if (InputIconMapper.Instance != null)
        {
            return InputIconMapper.Instance.GetSpriteOrText(InputAction.Interact);
        }
        
        if (Gamepad.current != null)
        {
            return "[X]";
        }
        else
        {
            return "[E]";
        }
    }

    void InteractWithStatue()
    {
        if (showDebugLogs) Debug.Log("🗿 === ABRIENDO DIÁLOGO DE ESTATUA ===");

        if (loreData == null)
        {
            Debug.LogError("❌ No hay Lore Data asignado!");
            return;
        }

        if (!allowMultipleInteractions && hasInteractedThisSession)
        {
            if (showDebugLogs) Debug.Log("⚠️ Ya interactuaste con esta estatua");
            return;
        }

        hasInteractedThisSession = true;

        if (showDebugLogs)
        {
            Debug.Log($"🗿 Mostrando: {loreData.statueName}");
            Debug.Log($"📝 Texto: {loreData.loreText}");
        }

        HidePrompt();
        isShowingDialog = true;

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.ShowSimpleText(loreData.statueName, loreData.loreText);
        }
        else
        {
            Debug.LogError("❌ InteractionManager.Instance es null!");
            isShowingDialog = false;
        }
    }

    /// <summary>
    /// Llamado por el InteractionManager cuando el diálogo se cierra
    /// </summary>
    public void OnDialogClosed()
    {
        if (showDebugLogs) Debug.Log("🚪 Diálogo cerrado - notificación recibida");
        
        isShowingDialog = false;
        
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
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, exitRange);
    }
    
    public void TriggerInteraction()
    {
        if (playerInRange && !isShowingDialog)
        {
            InteractWithStatue();
        }
    }
}