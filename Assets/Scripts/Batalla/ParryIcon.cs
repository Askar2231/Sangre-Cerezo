using UnityEngine;

/// <summary>
/// Parry indicator that appears during parry window
/// Can work as UI element (RectTransform) or world-space object (Transform)
/// Updates with the new parry window system (stays visible for full window duration)
/// </summary>
public class ParryIcon : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;

    private RectTransform rectTransform;
    private Transform regularTransform;
    private CanvasGroup canvasGroup;
    private bool isActive = false;
    private float timer = 0f;
    private bool isUIElement = false;

    private void Awake()
    {
        // Check if this is a UI element or world-space object
        rectTransform = GetComponent<RectTransform>();
        isUIElement = (rectTransform != null);
        
        if (!isUIElement)
        {
            regularTransform = transform;
        }
        
        // Add canvas group for fading if not present (UI elements only)
        if (isUIElement)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Start hidden
        Hide();
    }

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        // Pulsating animation
        float t = Mathf.PingPong(timer * pulseSpeed, 1f);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        
        if (isUIElement && rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * scale;
        }
        else if (regularTransform != null)
        {
            regularTransform.localScale = Vector3.one * scale;
        }
    }

    /// <summary>
    /// Show the parry indicator (called when parry window opens)
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        isActive = true;
        timer = 0f;
        
        // Fade in for UI elements
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        Debug.Log("<color=cyan>[ParryIcon]</color> ✅ Parry indicator shown");
    }

    /// <summary>
    /// Hide the parry indicator (called when parry window closes)
    /// </summary>
    public void Hide()
    {
        isActive = false;
        
        // Fade out for UI elements
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(false);
        
        Debug.Log("<color=cyan>[ParryIcon]</color> ❌ Parry indicator hidden");
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public void Initialize(Transform target, float duration, Vector3 offset)
    {
        // This method is kept for backward compatibility
        // The new system uses Show()/Hide() instead
        Debug.LogWarning("[ParryIcon] Initialize() called but new system uses Show()/Hide(). Showing indicator anyway.");
        Show();
    }
}

