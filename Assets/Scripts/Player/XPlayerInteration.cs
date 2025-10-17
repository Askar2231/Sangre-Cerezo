using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class XPlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float interactionRadius = 10f;
    
    [Header("UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI promptText;
    
    [Header("Input")]
    [SerializeField] private InputActionReference interactAction;
    
    private NPCInteraction currentNPC;
    private bool isUsingGamepad = false;
    private float interactionCooldown = 0f;
    private const float INTERACTION_COOLDOWN_TIME = 0.5f; // Prevent multiple triggers
    
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
        if (currentNPC != null)
        {
            // Mostrar prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
                
                // Use InputIconMapper for dynamic button display
                if (promptText != null)
                {
                    if (InputIconMapper.Instance != null)
                    {
                        // Use InputIconMapper to get device-specific button icon/text
                        string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Interact);
                        promptText.text = iconText + " Interactuar";
                    }
                    else
                    {
                        // Fallback if InputIconMapper not available
                        if (isUsingGamepad)
                        {
                            promptText.text = "[Y] Interactuar";
                        }
                        else
                        {
                            promptText.text = "[E] Interactuar";
                        }
                    }
                }
            }
        }
        else
        {
            // Ocultar prompt
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
    
    private void CheckInteraction()
    {
        // Don't check if on cooldown
        if (interactionCooldown > 0f || currentNPC == null)
            return;
        
        bool interactPressed = false;
        
        // Método 1: Input System (mando y teclado desde InputActions)
        // FIXED: Use WasPressedThisFrame() instead of triggered to prevent auto-fire on gamepad
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            interactPressed = true;
        }
        
        // Método 2: Teclado directo (fallback)
        if (Input.GetKeyDown(KeyCode.E))
        {
            interactPressed = true;
        }
        
        // Método 3: Botón Y del mando directo (fallback)
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            interactPressed = true;
        }
        
        if (interactPressed)
        {
            currentNPC.TriggerInteraction();
            interactionCooldown = INTERACTION_COOLDOWN_TIME; // Start cooldown
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}