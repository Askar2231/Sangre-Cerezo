using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Estatua interactiva que muestra lore usando el sistema de diálogos existente
/// </summary>
public class InteractableStatue : MonoBehaviour
{
    [Header("Statue Configuration")]
    [SerializeField] private StatueLoreData loreData;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 3f;
    
    [Header("Dialog Settings")]
    [SerializeField] private DialogPosition dialogPosition = DialogPosition.TopCenter;
    [SerializeField] private float dialogDuration = 0f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionPrompt;
    
    private Transform playerTransform;
    private bool playerInRange = false;
    private SimpleDialogPresenter dialogPresenter;
    private bool isShowingDialog = false;

    private void Start()
    {
        dialogPresenter = FindObjectOfType<SimpleDialogPresenter>();
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null || isShowingDialog) return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distance <= interactionRange)
        {
            if (!playerInRange)
            {
                OnPlayerEnterRange();
            }
            
            // USAR NEW INPUT SYSTEM
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("🔑 E KEY PRESSED!");
                InteractWithStatue();
            }
            
            // TAMBIÉN GAMEPAD
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                Debug.Log("🎮 GAMEPAD X PRESSED!");
                InteractWithStatue();
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
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
        
        Debug.Log($"✅ Player in range of {loreData?.statueName ?? "statue"}");
    }

    private void OnPlayerExitRange()
    {
        playerInRange = false;
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    private void InteractWithStatue()
    {
        if (loreData == null || dialogPresenter == null) return;
        
        Debug.Log($"🗿 INTERACTING WITH STATUE: {loreData.statueName}");
        
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
        
        isShowingDialog = true;
        
        string fullText = $"<b>{loreData.statueName}</b>\n\n{loreData.loreText}";
        
        dialogPresenter.ShowDialog(fullText, dialogPosition, dialogDuration, OnDialogClosed);
        
        if (loreData.narratorVoice != null)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.PlayOneShot(loreData.narratorVoice);
        }
    }

    private void OnDialogClosed()
    {
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
}