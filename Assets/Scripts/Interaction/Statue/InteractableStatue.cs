using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Estatua interactiva que muestra lore usando el sistema de diálogos existente
/// VERSION CON DEBUG COMPLETO
/// </summary>
public class InteractableStatue : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private StatueLoreData loreData;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    

    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    
    [Header("DEBUG")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Transform playerTransform;
    private bool playerInRange = false;
    private SimpleDialogPresenter dialogPresenter;
    private bool isShowingDialog = false;
    private int updateCount = 0;
     private bool hasInteracted = false;

    private void Start()
    {
        if (showDebugLogs) Debug.Log("🗿 === STATUE START ===");
        
        // Buscar el dialog presenter
        dialogPresenter = FindObjectOfType<SimpleDialogPresenter>();
        if (showDebugLogs) 
        {
            Debug.Log($"🗿 Dialog Presenter encontrado: {dialogPresenter != null}");
        }
        
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
            if (showDebugLogs) Debug.Log("🗿 Interaction Prompt desactivado");
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
            Debug.Log($"🔄 Update #{updateCount} - Player in range: {playerInRange}");
        }
        
        if (playerTransform == null)
        {
            if (showDebugLogs && updateCount == 1)
            {
                Debug.LogError("❌ playerTransform es NULL - no se puede detectar distancia");
            }
            return;
        }
        
        if (isShowingDialog) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        // Log de distancia cada 60 frames
        if (showDebugLogs && updateCount % 60 == 0)
        {
            Debug.Log($"📏 Distancia al jugador: {distance:F2} / Rango: {interactionRange}");
        }
        
        if (distance <= interactionRange)
        {
            if (!playerInRange)
            {
                OnPlayerEnterRange();
            }
            
            // DETECTAR INPUT - KEYBOARD
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (showDebugLogs) Debug.Log("🔑 === E KEY DETECTADA ===");
                    InteractWithStatue();
                }
            }
            else
            {
                if (showDebugLogs && updateCount == 1)
                {
                    Debug.LogWarning("⚠️ Keyboard.current es NULL");
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
        else
        {
            if (playerInRange)
            {
                OnPlayerExitRange();
            }
        }
    }

    private void OnPlayerEnterRange()
    {
        playerInRange = true;
        
        if (showDebugLogs) 
        {
            Debug.Log($"✅ === JUGADOR ENTRÓ EN RANGO === Estatua: {loreData?.statueName ?? "sin nombre"}");
        }
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            if (showDebugLogs) Debug.Log("👁️ Prompt activado");
        }
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        
        if (showDebugLogs) Debug.Log("❌ Jugador salió de rango");
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
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

    if (hasInteracted)
    {
        Debug.Log("⚠️ Ya interactuaste con esta estatua");
        return;
    }

    hasInteracted = true;

    Debug.Log($"🗿 Mostrando diálogo de: {loreData.statueName}");
    Debug.Log($"📝 Texto: {loreData.loreText}");

    if (InteractionManager.Instance != null)
    {
        InteractionManager.Instance.ShowSimpleText(loreData.statueName, loreData.loreText);
        Debug.Log("✅ ShowSimpleText llamado");
    }
    else
    {
        Debug.LogError("❌ InteractionManager.Instance es null!");
    }
}

    private void OnDialogClosed()
    {
        if (showDebugLogs) Debug.Log("🚪 Diálogo cerrado");
        
        isShowingDialog = false;
        
        if (playerInRange && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    // MÉTODO PÚBLICO PARA LLAMAR DESDE OTRO SCRIPT
    public void TriggerInteraction()
    {
        if (showDebugLogs) Debug.Log("🎯 TriggerInteraction llamado desde script externo");
        
        if (playerInRange)
        {
            InteractWithStatue();
        }
        else
        {
            Debug.LogWarning("⚠️ TriggerInteraction llamado pero jugador no está en rango");
        }
    }
}