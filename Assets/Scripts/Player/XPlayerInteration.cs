using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class XPlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float interactionRadius = 10f;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt; // ← Este es el prompt de los NPCs
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    
    private NPCInteraction currentNPC;
    private bool isUsingGamepad = false;
    private float interactionCooldown = 0f;
    private const float INTERACTION_COOLDOWN_TIME = 0.5f;
    
    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();
    }
    
    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.action.Disable();
    }
    
    private void Update()
    {
        DetectInputDevice();
        DetectNearbyNPC();
        UpdatePrompt();
        CheckInteraction();
        
        // Update cooldown timer
        if (interactionCooldown > 0f)
        {
            interactionCooldown -= Time.deltaTime;
        }
    }
    
    private void DetectInputDevice()
    {
        // Detectar si hay gamepad conectado y se está usando
        isUsingGamepad = Gamepad.current != null;
    }
    
    private void DetectNearbyNPC()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRadius);
        currentNPC = null;
        
        foreach (var hitCollider in hitColliders)
        {
            NPCInteraction npc = hitCollider.GetComponent<NPCInteraction>();
            if (npc != null)
            {
                currentNPC = npc;
                break;
            }
        }
    }
    
    private void UpdatePrompt()
    {
        if (interactionPrompt == null) return;
        
        if (currentNPC != null)
        {
            // Hay NPC cerca: Mostrar prompt
            if (!interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(true);
            }
            
            // Actualizar texto del prompt
            if (promptText != null)
            {
                string iconText = GetButtonIcon();
                promptText.text = iconText + " Interactuar";
            }
        }
        else
        {
            // No hay NPC: Ocultar prompt
            if (interactionPrompt.activeSelf)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    private string GetButtonIcon()
    {
        // Use InputIconMapper for dynamic button display
        if (InputIconMapper.Instance != null)
        {
            return InputIconMapper.Instance.GetSpriteOrText(InputAction.Interact);
        }
        
        // Fallback
        if (isUsingGamepad)
        {
            return "[X]";
        }
        else
        {
            return "[E]";
        }
    }
    
    private void CheckInteraction()
    {
        // Don't check if on cooldown or no NPC nearby
        if (interactionCooldown > 0f || currentNPC == null)
            return;
        
        bool interactPressed = false;
        
        // Método 1: Input System (mando y teclado desde InputActions)
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            interactPressed = true;
        }
        
        // Método 2: Teclado directo (fallback)
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactPressed = true;
        }
        
        // Método 3: Botón X del mando directo (fallback)
        if (Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            interactPressed = true;
        }
        
        if (interactPressed)
        {
            currentNPC.TriggerInteraction();
            interactionCooldown = INTERACTION_COOLDOWN_TIME;
            
            // Ocultar el prompt mientras se muestra el diálogo
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}