using UnityEngine;

/// <summary>
/// Trigger de tutorial basado en colisión o interacción física
/// Puede activarse automáticamente al entrar en la zona o requerir interacción
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialTriggerZone : TutorialTrigger
{
    [Header("Configuración de Zona")]
    [SerializeField] private bool requireInteraction = false;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float interactionDistance = 3f;

    [Header("Indicadores Visuales")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);

    private bool playerInZone = false;
    private Transform playerTransform = null;
    private Collider triggerCollider;

    protected override void Start()
    {
        base.Start();

        // Configurar collider como trigger
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        // Ocultar prompt de interacción
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void Update()
    {
        // Si requiere interacción y el jugador está en la zona
        if (requireInteraction && playerInZone && playerTransform != null)
        {
            // Verificar distancia
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            
            if (distance <= interactionDistance)
            {
                // Mostrar prompt
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                // Verificar input de interacción
                if (CheckInteractionInput())
                {
                    FireTrigger();
                    HideInteractionPrompt();
                }
            }
            else
            {
                HideInteractionPrompt();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = true;
            playerTransform = other.transform;

            if (!requireInteraction)
            {
                // Activación automática
                FireTrigger();
            }

            if (debugMode) Debug.Log($"{gameObject.name}: Jugador entró en zona de tutorial");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInZone = false;
            playerTransform = null;
            HideInteractionPrompt();

            if (debugMode) Debug.Log($"{gameObject.name}: Jugador salió de zona de tutorial");
        }
    }

    private bool CheckInteractionInput()
    {
        // Keyboard
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        // Gamepad
        if (UnityEngine.InputSystem.Gamepad.current != null && 
            UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void HideInteractionPrompt()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    protected override void ExecuteTrigger()
    {
        base.ExecuteTrigger();

        // Si es de una sola vez, deshabilitar el collider
        if (triggerOnce)
        {
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar zona de trigger en el editor
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
            else if (col is CapsuleCollider capsuleCol)
            {
                // Aproximación con esferas para cápsula
                Gizmos.DrawSphere(transform.position + capsuleCol.center, capsuleCol.radius);
            }

            // Dibujar radio de interacción si requiere interacción
            if (requireInteraction)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, interactionDistance);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujar gizmo más visible cuando está seleccionado
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.6f);

            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
                Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
        }
    }
}

