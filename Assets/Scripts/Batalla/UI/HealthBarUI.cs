using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Reusable health bar UI component
/// Can be used in both screen-space and worldspace contexts
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Visual Settings")]
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private bool showNumericalValue = false;

    [Header("Animation Settings")]
    [SerializeField] private bool useSmoothLerp = true;
    [SerializeField] private float damageFlashDuration = 0.1f;
    [SerializeField] private Color damageFlashColor = Color.red;

    // Runtime state
    private float currentHealth;
    private float maxHealth;
    private float targetFillAmount;
    private float currentFillAmount;
    private Color originalFillColor;

    private void Awake()
    {
        ValidateReferences();
        if (fillImage != null)
        {
            originalFillColor = fillImage.color;
        }
    }

    /// <summary>
    /// Initialize the health bar with max health value
    /// </summary>
    public void Initialize(float maxHealthValue)
    {
        maxHealth = maxHealthValue;
        currentHealth = maxHealthValue;
        targetFillAmount = 1f;
        currentFillAmount = 1f;

        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            UpdateFillColor(1f);
        }

        UpdateHealthText();
    }

    /// <summary>
    /// Update health with smooth animation
    /// </summary>
    public void UpdateHealth(float currentHealthValue, float maxHealthValue)
    {
        currentHealth = Mathf.Clamp(currentHealthValue, 0f, maxHealthValue);
        maxHealth = maxHealthValue;
        
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        targetFillAmount = healthPercentage;

        UpdateHealthText();
        UpdateFillColor(healthPercentage);
    }

    /// <summary>
    /// Set health immediately without lerp animation
    /// </summary>
    public void SetHealthImmediate(float currentHealthValue, float maxHealthValue)
    {
        currentHealth = Mathf.Clamp(currentHealthValue, 0f, maxHealthValue);
        maxHealth = maxHealthValue;
        
        float healthPercentage = maxHealth > 0 ? currentHealth / maxHealth : 0f;
        targetFillAmount = healthPercentage;
        currentFillAmount = healthPercentage;

        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercentage;
        }

        UpdateHealthText();
        UpdateFillColor(healthPercentage);
    }

    private void Update()
    {
        if (!useSmoothLerp || fillImage == null) return;

        // Smooth lerp to target fill amount
        if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * lerpSpeed);
            fillImage.fillAmount = currentFillAmount;
        }
        else
        {
            currentFillAmount = targetFillAmount;
            fillImage.fillAmount = currentFillAmount;
        }
    }

    /// <summary>
    /// Update the fill color based on health gradient
    /// </summary>
    private void UpdateFillColor(float healthPercentage)
    {
        if (fillImage != null && healthGradient != null && healthGradient.colorKeys.Length > 0)
        {
            fillImage.color = healthGradient.Evaluate(healthPercentage);
        }
    }

    /// <summary>
    /// Update health text display
    /// </summary>
    private void UpdateHealthText()
    {
        if (healthText != null && showNumericalValue)
        {
            healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            healthText.gameObject.SetActive(true);
            Debug.Log($"[HealthBarUI] Health text updated: {healthText.text}, Active: {healthText.gameObject.activeSelf}, Enabled: {healthText.enabled}");
        }
        else if (healthText != null)
        {
            healthText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Flash effect when taking damage
    /// </summary>
    public void PlayDamageFlash()
    {
        if (fillImage != null)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    private System.Collections.IEnumerator DamageFlashCoroutine()
    {
        Color originalColor = fillImage.color;
        fillImage.color = damageFlashColor;
        
        yield return new WaitForSeconds(damageFlashDuration);
        
        fillImage.color = originalColor;
    }

    /// <summary>
    /// Show or hide the health bar
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Validate all required references
    /// </summary>
    private void ValidateReferences()
    {
        if (fillImage == null)
        {
            Debug.LogError($"[HealthBarUI] Fill Image not assigned on {gameObject.name}!", this);
        }

        if (healthGradient == null || healthGradient.colorKeys.Length == 0)
        {
            Debug.LogWarning($"[HealthBarUI] Health Gradient not configured on {gameObject.name}. Using default colors.", this);
            
            // Create default gradient (green -> yellow -> red)
            healthGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.green, 1f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);
            
            healthGradient.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    /// <summary>
    /// Enable or disable numerical health text display
    /// </summary>
    public void SetShowNumericalValue(bool show)
    {
        showNumericalValue = show;
        UpdateHealthText();
        Debug.Log($"[HealthBarUI] Numerical value display set to: {show}");
    }

    #region Public Getters
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool ShowNumericalValue => showNumericalValue;
    
    #endregion
}
