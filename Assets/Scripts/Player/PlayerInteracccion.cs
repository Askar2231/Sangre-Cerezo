using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    private NPCInteraction currentNPC;
    
    private void Update()
    {
        DetectNearbyNPC();
        CheckInteraction();
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
    
    private void CheckInteraction()
    {
        if (Input.GetKeyDown(interactionKey) && currentNPC != null)
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