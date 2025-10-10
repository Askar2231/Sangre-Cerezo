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
                
                // Cambiar texto según el dispositivo de entrada
                if (promptText != null)
                {
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
        bool interactPressed = false;
        
        // Método 1: Input System (mando y teclado desde InputActions)
        if (interactAction != null && interactAction.action.triggered)
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
        
        if (interactPressed && currentNPC != null)
        {
            currentNPC.TriggerInteraction();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}