using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Centralized notification system for battle events
/// Displays text notifications for QTE, parry, damage, etc.
/// </summary>
public class BattleNotificationSystem : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Parent GameObject containing the notification UI (will be shown/hidden)")]
    [SerializeField] private GameObject notificationContainer;
    [Tooltip("TextMeshProUGUI component that will display notifications")]
    [SerializeField] private TextMeshProUGUI notificationText;
    
    [Header("Display Settings")]
    [SerializeField] private float notificationDuration = 2f;
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    
    [Header("Color Settings")]
    [SerializeField] private Color successColor = new Color(0.2f, 1f, 0.2f); // Bright green
    [SerializeField] private Color perfectColor = new Color(1f, 0.84f, 0f); // Gold
    [SerializeField] private Color damageDealtColor = new Color(1f, 0.4f, 0.2f); // Orange-red
    [SerializeField] private Color damageReceivedColor = new Color(1f, 0.2f, 0.2f); // Red
    [SerializeField] private Color infoColor = new Color(0.8f, 0.8f, 1f); // Light blue
    
    private Coroutine currentNotificationCoroutine;
    
    private void Awake()
    {
        if (notificationText == null)
        {
            Debug.LogError("BattleNotificationSystem: notificationText is not assigned!");
            return;
        }
        
        // Start hidden
        if (notificationContainer != null)
        {
            notificationContainer.SetActive(false);
        }
        else
        {
            SetTextAlpha(0f);
        }
    }
    
    #region Public Notification Methods
    
    /// <summary>
    /// Show notification for successful QTE
    /// </summary>
    public void ShowQTESuccess(float damageDealt)
    {
        string message = $"¡Ataque Perfecto! Has hecho {damageDealt:F0} daño";
        ShowNotification(message, perfectColor);
    }
    
    /// <summary>
    /// Show notification for failed QTE (normal attack)
    /// </summary>
    public void ShowQTEFailed(float damageDealt)
    {
        string message = $"Has hecho {damageDealt:F0} daño";
        ShowNotification(message, damageDealtColor);
    }
    
    /// <summary>
    /// Show notification for successful parry
    /// </summary>
    public void ShowParrySuccess(float staminaGained, float counterDamage)
    {
        string message = $"¡Bloqueo! +{staminaGained:F0} stamina. ¡Contrataque de {counterDamage:F0} daño!";
        Debug.Log($"<color=lime>[BattleNotificationSystem]</color> 🛡️ Showing PARRY SUCCESS notification: '{message}'");
        ShowNotification(message, successColor);
    }
    
    /// <summary>
    /// Show notification for perfect parry
    /// </summary>
    public void ShowPerfectParry(float staminaGained, float counterDamage)
    {
        string message = $"¡¡BLOQUEO PERFECTO!! +{staminaGained:F0} stamina. ¡Contrataque de {counterDamage:F0} daño!";
        Debug.Log($"<color=yellow>[BattleNotificationSystem]</color> ⭐ Showing PERFECT PARRY notification: '{message}'");
        ShowNotification(message, perfectColor);
    }
    
    /// <summary>
    /// Show notification for failed parry (player took damage)
    /// </summary>
    public void ShowParryFailed(float damageTaken)
    {
        string message = $"Has recibido {damageTaken:F0} daño";
        ShowNotification(message, damageReceivedColor);
    }
    
    /// <summary>
    /// Show notification when player gets parried by the boss
    /// </summary>
    public void ShowPlayerParriedByBoss()
    {
        string message = "¡Tu ataque ha sido bloqueado!";
        Debug.Log($"<color=red>[BattleNotificationSystem]</color> 🛡️ Showing PLAYER PARRIED notification: '{message}'");
        ShowNotification(message, damageReceivedColor);
    }
    
    /// <summary>
    /// Show notification for damage dealt (non-QTE)
    /// </summary>
    public void ShowDamageDealt(float damage)
    {
        string message = $"Has infligido {damage:F0} daño";
        ShowNotification(message, damageDealtColor);
    }
    
    /// <summary>
    /// Show notification for damage received
    /// </summary>
    public void ShowDamageReceived(float damage)
    {
        string message = $"Has recibido {damage:F0} daño";
        ShowNotification(message, damageReceivedColor);
    }
    
    /// <summary>
    /// Show custom notification with custom color
    /// </summary>
    public void ShowCustomNotification(string message, Color? color = null)
    {
        ShowNotification(message, color ?? infoColor);
    }
    
    /// <summary>
    /// Show notification for insufficient stamina/resources
    /// </summary>
    public void ShowInsufficientResources(string actionName)
    {
        string message = $"No tienes suficiente stamina para {actionName}";
        ShowNotification(message, damageReceivedColor);
    }
    
    #endregion
    
    #region Core Display Logic
    
    /// <summary>
    /// Display a notification with fade in/out animation
    /// </summary>
    private void ShowNotification(string message, Color color)
    {
        Debug.Log($"<color=cyan>[BattleNotificationSystem]</color> 📢 ShowNotification() called with message: '{message}'");
        
        if (notificationText == null)
        {
            Debug.LogWarning("<color=red>[BattleNotificationSystem]</color> ❌ Cannot show notification: notificationText is null!");
            return;
        }
        
        if (notificationContainer != null)
        {
            Debug.Log($"<color=cyan>[BattleNotificationSystem]</color> Container: {notificationContainer.name}, Active: {notificationContainer.activeSelf}");
            
            // FORCE ENABLE: Make sure notification container stays active even during enemy turns
            // This ensures parry notifications are visible during enemy attack animations
            if (!notificationContainer.activeSelf)
            {
                Debug.Log($"<color=yellow>[BattleNotificationSystem]</color> ⚠️ Notification container was disabled, force-enabling it!");
            }
        }
        
        // Cancel any existing notification
        if (currentNotificationCoroutine != null)
        {
            Debug.Log("<color=yellow>[BattleNotificationSystem]</color> ⚠️ Stopping previous notification to show new one");
            StopCoroutine(currentNotificationCoroutine);
        }
        
        // Start new notification coroutine
        Debug.Log($"<color=lime>[BattleNotificationSystem]</color> ✅ Starting notification sequence with duration {notificationDuration}s");
        currentNotificationCoroutine = StartCoroutine(NotificationSequence(message, color));
    }
    
    /// <summary>
    /// Coroutine that handles the full notification display sequence
    /// </summary>
    private IEnumerator NotificationSequence(string message, Color color)
    {
        // Set text and color
        notificationText.text = message;
        notificationText.color = color;
        
        // If using container, just show/hide it
        if (notificationContainer != null)
        {
            // IMPORTANT: Enable the container AND ensure it's visible
            // This is needed because notifications should be visible even during enemy turns
            notificationContainer.SetActive(true);
            
            // Double-check it's actually enabled (parent might be disabled)
            if (!notificationContainer.activeInHierarchy)
            {
                Debug.LogWarning($"<color=yellow>[BattleNotificationSystem]</color> ⚠️ Notification container '{notificationContainer.name}' is not showing! " +
                    $"Check if parent GameObject is disabled. Notification: '{message}'");
                Debug.LogWarning($"<color=yellow>[BattleNotificationSystem]</color> 💡 TIP: Make sure the Notification Container is NOT a child of actionSelectionUI or any UI that gets hidden during enemy turns!");
            }
            else
            {
                Debug.Log($"<color=lime>[BattleNotificationSystem]</color> ✅ Notification container is visible and showing message!");
            }
            
            // Hold
            yield return new WaitForSeconds(notificationDuration);
            
            notificationContainer.SetActive(false);
        }
        else
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                SetTextAlpha(alpha);
                yield return null;
            }
            SetTextAlpha(1f);
            
            // Hold
            yield return new WaitForSeconds(notificationDuration);
            
            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                SetTextAlpha(alpha);
                yield return null;
            }
            SetTextAlpha(0f);
        }
        
        currentNotificationCoroutine = null;
    }
    
    /// <summary>
    /// Set the alpha of the notification text
    /// </summary>
    private void SetTextAlpha(float alpha)
    {
        if (notificationText != null)
        {
            Color color = notificationText.color;
            color.a = alpha;
            notificationText.color = color;
        }
    }
    
    /// <summary>
    /// Force hide the notification immediately
    /// </summary>
    public void HideNotification()
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
            currentNotificationCoroutine = null;
        }
        
        if (notificationContainer != null)
        {
            notificationContainer.SetActive(false);
        }
        else
        {
            SetTextAlpha(0f);
        }
    }
    
    #endregion
}
