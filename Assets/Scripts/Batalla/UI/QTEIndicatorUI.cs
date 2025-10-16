using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI indicator for QTE windows - shows button prompt and timing bar
/// </summary>
public class QTEIndicatorUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image buttonIcon;
    [SerializeField] private Image timingBar;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.8f, 1, 1f);
    
    [Header("Timing Bar Colors")]
    [SerializeField] private Color barColorActive = Color.yellow;
    [SerializeField] private Color barColorSuccess = Color.green;
    [SerializeField] private Color barColorFail = Color.red;
    
    [Header("References")]
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private InputIconMapper iconMapper;
    
    private Coroutine activeAnimation;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        else
        {
            Debug.LogWarning("CanvasGroup not assigned to QTEIndicatorUI!");
        }
        
        gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        if (qteManager != null)
        {
            qteManager.OnQTEWindowStart += HandleQTEWindowStart;
            qteManager.OnQTEWindowEnd += HandleQTEWindowEnd;
            qteManager.OnQTESuccess += HandleQTESuccess;
            qteManager.OnQTEFail += HandleQTEFail;
        }
    }
    
    private void OnDisable()
    {
        if (qteManager != null)
        {
            qteManager.OnQTEWindowStart -= HandleQTEWindowStart;
            qteManager.OnQTEWindowEnd -= HandleQTEWindowEnd;
            qteManager.OnQTESuccess -= HandleQTESuccess;
            qteManager.OnQTEFail -= HandleQTEFail;
        }
    }
    
    /// <summary>
    /// Handle QTE window starting
    /// </summary>
    private void HandleQTEWindowStart(bool isActive)
    {
        if (isActive)
        {
            ShowIndicator();
        }
    }
    
    /// <summary>
    /// Handle QTE window ending
    /// </summary>
    private void HandleQTEWindowEnd()
    {
        HideIndicator();
    }
    
    /// <summary>
    /// Handle successful QTE
    /// </summary>
    private void HandleQTESuccess()
    {
        if (timingBar != null)
        {
            timingBar.color = barColorSuccess;
        }
    }
    
    /// <summary>
    /// Handle failed QTE
    /// </summary>
    private void HandleQTEFail()
    {
        if (timingBar != null)
        {
            timingBar.color = barColorFail;
        }
    }
    
    /// <summary>
    /// Show the QTE indicator with animation
    /// </summary>
    private void ShowIndicator()
    {
        gameObject.SetActive(true);
        
        // Update button icon
        if (iconMapper != null && buttonIcon != null)
        {
            // Get the QTE button action (you may need to adjust based on your input setup)
            // For now, using a placeholder - should match your input system
            string iconText = iconMapper.GetIconForQTE();
            
            // If using sprite, parse it
            // For now, update the prompt text
            if (promptText != null)
            {
                promptText.text = iconText;
            }
        }
        
        // Reset timing bar
        if (timingBar != null)
        {
            timingBar.color = barColorActive;
            timingBar.fillAmount = 1f;
        }
        
        // Animate in
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }
        activeAnimation = StartCoroutine(AnimateIn());
        
        // Start timing bar countdown
        if (qteManager != null)
        {
            StartCoroutine(AnimateTimingBar(qteManager.CurrentQTEDuration));
        }
    }
    
    /// <summary>
    /// Hide the QTE indicator with animation
    /// </summary>
    private void HideIndicator()
    {
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }
        activeAnimation = StartCoroutine(AnimateOut());
    }
    
    /// <summary>
    /// Fade in animation
    /// </summary>
    private IEnumerator AnimateIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = t;
            }
            
            if (rectTransform != null)
            {
                float scale = scaleCurve.Evaluate(t);
                rectTransform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        // Ensure final values
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Fade out animation
    /// </summary>
    private IEnumerator AnimateOut()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f - t;
            }
            
            yield return null;
        }
        
        // Ensure final values
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Animate the timing bar countdown
    /// </summary>
    private IEnumerator AnimateTimingBar(float duration)
    {
        if (timingBar == null) yield break;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float remaining = 1f - (elapsed / duration);
            timingBar.fillAmount = remaining;
            
            yield return null;
        }
        
        timingBar.fillAmount = 0f;
    }
    
    /// <summary>
    /// Set references programmatically
    /// </summary>
    public void Initialize(QTEManager manager, InputIconMapper mapper)
    {
        qteManager = manager;
        iconMapper = mapper;
        
        // Re-subscribe to events
        OnEnable();
    }
}
