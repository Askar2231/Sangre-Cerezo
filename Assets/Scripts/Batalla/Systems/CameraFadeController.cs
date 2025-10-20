using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles camera fade in/out effects for scene transitions
/// </summary>
public class CameraFadeController : MonoBehaviour
{
    private static CameraFadeController _instance;
    public static CameraFadeController Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance
                _instance = FindFirstObjectByType<CameraFadeController>();
                
                // Create new instance if none exists
                if (_instance == null)
                {
                    GameObject fadeObj = new GameObject("CameraFadeController");
                    _instance = fadeObj.AddComponent<CameraFadeController>();
                    DontDestroyOnLoad(fadeObj);
                }
            }
            return _instance;
        }
    }
    
    [Header("Fade Settings")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private float defaultFadeDuration = 0.5f;
    
    [Header("UI Setup")]
    [SerializeField] private Canvas fadeCanvas;
    [SerializeField] private Image fadeImage;
    
    private Coroutine currentFadeCoroutine;
    private bool isFading = false;
    
    public bool IsFading => isFading;
    
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Create fade UI if it doesn't exist
        if (fadeCanvas == null)
        {
            CreateFadeUI();
        }
        
        // Start transparent
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Create the fade UI canvas and image
    /// </summary>
    private void CreateFadeUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);
        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Render on top of everything
        
        // Add CanvasScaler
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Create Image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(canvasObj.transform, false);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        
        // Make image fill screen
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Start hidden
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeCanvas.gameObject.SetActive(false);
        
        Debug.Log("Camera fade UI created successfully");
    }
    
    /// <summary>
    /// Fade to black (or specified color)
    /// </summary>
    public void FadeOut(float duration = -1f)
    {
        if (duration < 0f) duration = defaultFadeDuration;
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration));
    }
    
    /// <summary>
    /// Fade from black to transparent
    /// </summary>
    public void FadeIn(float duration = -1f)
    {
        if (duration < 0f) duration = defaultFadeDuration;
        
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration));
    }
    
    /// <summary>
    /// Fade out then fade in with a callback in between
    /// </summary>
    public IEnumerator FadeOutAndIn(System.Action actionDuringFade, float fadeOutDuration = -1f, float fadeInDuration = -1f, float holdDuration = 0f)
    {
        if (fadeOutDuration < 0f) fadeOutDuration = defaultFadeDuration;
        if (fadeInDuration < 0f) fadeInDuration = defaultFadeDuration;
        
        // Fade out
        yield return FadeCoroutine(0f, 1f, fadeOutDuration);
        
        // Hold at black
        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }
        
        // Execute action while screen is black
        actionDuringFade?.Invoke();
        
        // Small delay to ensure action completes
        yield return new WaitForEndOfFrame();
        
        // Fade in
        yield return FadeCoroutine(1f, 0f, fadeInDuration);
    }
    
    /// <summary>
    /// Core fade coroutine
    /// </summary>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        isFading = true;
        
        // Ensure canvas is active
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            // Smooth fade curve
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }
            
            yield return null;
        }
        
        // Ensure final alpha is set
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = endAlpha;
            fadeImage.color = c;
        }
        
        // Disable canvas if fully transparent
        if (endAlpha <= 0f && fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(false);
        }
        
        isFading = false;
        currentFadeCoroutine = null;
    }
    
    /// <summary>
    /// Instantly set fade to black
    /// </summary>
    public void SetFadeImmediate(float alpha)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
        
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
        
        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(alpha > 0f);
        }
        
        isFading = false;
    }
    
    /// <summary>
    /// Set the fade color
    /// </summary>
    public void SetFadeColor(Color color)
    {
        fadeColor = color;
        if (fadeImage != null)
        {
            Color c = color;
            c.a = fadeImage.color.a; // Preserve current alpha
            fadeImage.color = c;
        }
    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
