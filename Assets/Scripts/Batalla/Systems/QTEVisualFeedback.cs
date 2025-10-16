using UnityEngine;
using System.Collections;

/// <summary>
/// Manages visual feedback for QTE (Quick Time Event) windows
/// Shows glow effect on player during QTE, with color-coded feedback
/// </summary>
public class QTEVisualFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QTEManager qteManager;
    [SerializeField] private Renderer playerRenderer; // The player's main renderer
    
    [Header("Glow Settings")]
    [SerializeField] private Color glowColorActive = new Color(0.5f, 0.8f, 1f, 1f); // Blue/white during QTE
    [SerializeField] private Color glowColorSuccess = Color.green;
    [SerializeField] private Color glowColorFail = Color.red;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float glowDuration = 0.3f; // How long success/fail glow lasts
    
    [Header("Warning Settings")]
    [SerializeField] private bool showPreWarning = true;
    [SerializeField] private float warningDuration = 0.3f; // Warning before QTE starts
    [SerializeField] private Color warningColor = new Color(1f, 1f, 0.5f, 0.5f); // Subtle yellow
    [SerializeField] private float warningIntensity = 1f;
    
    [Header("Particle System (Optional)")]
    [SerializeField] private ParticleSystem glowParticles;
    
    // Material properties
    private Material playerMaterial;
    private bool hasEmissionSupport = false;
    private Color originalEmissionColor;
    private Coroutine activeGlowCoroutine;
    
    // State tracking
    private bool isQTEActive = false;
    private bool isShowingWarning = false;
    
    private void Awake()
    {
        InitializeMaterial();
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
        
        // Clean up
        if (activeGlowCoroutine != null)
        {
            StopCoroutine(activeGlowCoroutine);
        }
        DisableGlow();
    }
    
    /// <summary>
    /// Initialize material and check for emission support
    /// </summary>
    private void InitializeMaterial()
    {
        if (playerRenderer != null)
        {
            // Create instance of material to avoid modifying shared material
            playerMaterial = playerRenderer.material;
            
            // Check if material supports emission
            hasEmissionSupport = playerMaterial.HasProperty("_EmissionColor");
            
            if (hasEmissionSupport)
            {
                // Store original emission color
                originalEmissionColor = playerMaterial.GetColor("_EmissionColor");
                
                // Enable emission keyword if not already enabled
                playerMaterial.EnableKeyword("_EMISSION");
            }
            else
            {
                Debug.LogWarning("Player material does not support emission. QTE glow will be limited.");
            }
        }
        else
        {
            Debug.LogWarning("Player renderer not assigned to QTEVisualFeedback!");
        }
    }
    
    /// <summary>
    /// Show pre-warning before QTE window
    /// </summary>
    public void ShowPreWarning()
    {
        if (!showPreWarning || isShowingWarning) return;
        
        if (activeGlowCoroutine != null)
        {
            StopCoroutine(activeGlowCoroutine);
        }
        
        activeGlowCoroutine = StartCoroutine(PreWarningSequence());
    }
    
    /// <summary>
    /// Pre-warning animation sequence
    /// </summary>
    private IEnumerator PreWarningSequence()
    {
        isShowingWarning = true;
        
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            float intensity = Mathf.Lerp(0f, warningIntensity, elapsed / warningDuration);
            
            SetGlow(warningColor, intensity);
            
            yield return null;
        }
        
        isShowingWarning = false;
    }
    
    /// <summary>
    /// Handle QTE window starting
    /// </summary>
    private void HandleQTEWindowStart(bool isActive)
    {
        if (isActive)
        {
            isQTEActive = true;
            
            if (activeGlowCoroutine != null)
            {
                StopCoroutine(activeGlowCoroutine);
            }
            
            // Show active QTE glow
            SetGlow(glowColorActive, glowIntensity);
            
            // Start particle effect if available
            if (glowParticles != null && !glowParticles.isPlaying)
            {
                glowParticles.Play();
            }
        }
    }
    
    /// <summary>
    /// Handle QTE window ending
    /// </summary>
    private void HandleQTEWindowEnd()
    {
        isQTEActive = false;
        DisableGlow();
        
        // Stop particles
        if (glowParticles != null && glowParticles.isPlaying)
        {
            glowParticles.Stop();
        }
    }
    
    /// <summary>
    /// Handle successful QTE
    /// </summary>
    private void HandleQTESuccess()
    {
        if (activeGlowCoroutine != null)
        {
            StopCoroutine(activeGlowCoroutine);
        }
        
        activeGlowCoroutine = StartCoroutine(FlashGlow(glowColorSuccess));
    }
    
    /// <summary>
    /// Handle failed QTE
    /// </summary>
    private void HandleQTEFail()
    {
        if (activeGlowCoroutine != null)
        {
            StopCoroutine(activeGlowCoroutine);
        }
        
        activeGlowCoroutine = StartCoroutine(FlashGlow(glowColorFail));
    }
    
    /// <summary>
    /// Flash a color briefly for feedback
    /// </summary>
    private IEnumerator FlashGlow(Color color)
    {
        // Quick flash
        SetGlow(color, glowIntensity * 1.5f);
        yield return new WaitForSeconds(glowDuration);
        
        // Return to active glow if QTE still active, otherwise disable
        if (isQTEActive)
        {
            SetGlow(glowColorActive, glowIntensity);
        }
        else
        {
            DisableGlow();
        }
    }
    
    /// <summary>
    /// Set glow effect with specified color and intensity
    /// </summary>
    private void SetGlow(Color color, float intensity)
    {
        if (!hasEmissionSupport || playerMaterial == null) return;
        
        Color emissionColor = color * Mathf.LinearToGammaSpace(intensity);
        playerMaterial.SetColor("_EmissionColor", emissionColor);
        playerMaterial.EnableKeyword("_EMISSION");
        
        // Update global illumination
        DynamicGI.SetEmissive(playerRenderer, emissionColor);
    }
    
    /// <summary>
    /// Disable glow effect
    /// </summary>
    private void DisableGlow()
    {
        if (!hasEmissionSupport || playerMaterial == null) return;
        
        playerMaterial.SetColor("_EmissionColor", originalEmissionColor);
        
        // If original was black, disable emission
        if (originalEmissionColor == Color.black)
        {
            playerMaterial.DisableKeyword("_EMISSION");
        }
    }
    
    /// <summary>
    /// Set references programmatically
    /// </summary>
    public void Initialize(QTEManager manager, Renderer renderer)
    {
        qteManager = manager;
        playerRenderer = renderer;
        InitializeMaterial();
        
        // Re-subscribe to events
        OnEnable();
    }
    
    private void OnDestroy()
    {
        // Clean up material instance
        if (playerMaterial != null)
        {
            Destroy(playerMaterial);
        }
    }
}
