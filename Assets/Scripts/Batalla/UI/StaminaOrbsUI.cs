using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Displays stamina as a series of orbs/dots
/// Each orb represents a fixed amount of stamina (default: 20)
/// </summary>
public class StaminaOrbsUI : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private Transform orbContainer;
    [SerializeField] private float staminaPerOrb = 20f;

    [Header("Visual Settings")]
    [SerializeField] private Color fullOrbColor = new Color(0.2f, 0.6f, 1f, 1f); // Cyan
    [SerializeField] private Color emptyOrbColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Dark gray
    [SerializeField] private float fillLerpSpeed = 8f;
    [SerializeField] private bool useSmoothLerp = true;

    [Header("Layout")]
    [SerializeField] private float orbSpacing = 10f;

    // Runtime state
    private List<Image> spawnedOrbs = new List<Image>();
    private List<float> targetFillAmounts = new List<float>();
    private List<float> currentFillAmounts = new List<float>();
    private float currentStamina;
    private float maxStamina;

    private void Awake()
    {
        ValidateReferences();
        SetupLayout();
    }

    /// <summary>
    /// Initialize the stamina orb display
    /// </summary>
    public void Initialize(float maxStaminaValue, float staminaPerOrbValue = 20f)
    {
        maxStamina = maxStaminaValue;
        staminaPerOrb = staminaPerOrbValue;
        currentStamina = maxStaminaValue;

        // Clear existing orbs
        ClearOrbs();

        // Calculate how many orbs we need (máximo 3)
        int orbCount = Mathf.CeilToInt(maxStamina / staminaPerOrb);
        orbCount = Mathf.Min(orbCount, 3); // Limitar a máximo 3 orbes

        // Spawn orbs
        SpawnOrbs(orbCount);

        // Set all orbs to full
        UpdateStamina(maxStamina);
        
        Debug.Log($"[StaminaOrbsUI] Initialized with {orbCount} orbs (Max Stamina: {maxStamina}, Per Orb: {staminaPerOrb})");
    }

    /// <summary>
    /// Update stamina display
    /// </summary>
    public void UpdateStamina(float currentStaminaValue)
    {
        currentStamina = Mathf.Clamp(currentStaminaValue, 0f, maxStamina);

        // Calculate how many full orbs and partial fill
        int fullOrbs = Mathf.FloorToInt(currentStamina / staminaPerOrb);
        float remainingStamina = currentStamina % staminaPerOrb;
        float partialFill = remainingStamina / staminaPerOrb;

        // Update target fill amounts for all orbs
        for (int i = 0; i < spawnedOrbs.Count; i++)
        {
            if (i < fullOrbs)
            {
                // Full orb
                targetFillAmounts[i] = 1f;
            }
            else if (i == fullOrbs)
            {
                // Partial orb
                targetFillAmounts[i] = partialFill;
            }
            else
            {
                // Empty orb
                targetFillAmounts[i] = 0f;
            }

            // If not using smooth lerp, set immediately
            if (!useSmoothLerp)
            {
                currentFillAmounts[i] = targetFillAmounts[i];
                ApplyOrbFill(i);
            }
        }
    }

    private void Update()
    {
        if (!useSmoothLerp) return;

        // Smooth lerp all orbs to their target fill amounts
        for (int i = 0; i < spawnedOrbs.Count; i++)
        {
            if (Mathf.Abs(currentFillAmounts[i] - targetFillAmounts[i]) > 0.001f)
            {
                currentFillAmounts[i] = Mathf.Lerp(
                    currentFillAmounts[i], 
                    targetFillAmounts[i], 
                    Time.deltaTime * fillLerpSpeed
                );
                ApplyOrbFill(i);
            }
            else
            {
                currentFillAmounts[i] = targetFillAmounts[i];
            }
        }
    }

    /// <summary>
    /// Apply fill amount to a specific orb
    /// </summary>
    private void ApplyOrbFill(int index)
    {
        if (index < 0 || index >= spawnedOrbs.Count) return;

        Image orbImage = spawnedOrbs[index];
        float fillAmount = currentFillAmounts[index];

        orbImage.fillAmount = fillAmount;
        
        // Color interpolation between empty and full
        orbImage.color = Color.Lerp(emptyOrbColor, fullOrbColor, fillAmount);
    }

    /// <summary>
    /// Spawn the required number of orbs
    /// </summary>
    private void SpawnOrbs(int count)
    {
        if (orbPrefab == null || orbContainer == null)
        {
            Debug.LogError("[StaminaOrbsUI] Cannot spawn orbs - prefab or container is null!");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject orbObject = Instantiate(orbPrefab, orbContainer);
            
            // Reset to global scale (1,1,1) to avoid inheriting parent's scale
            orbObject.transform.localScale = Vector3.one;
            
            // Try to get Image component (supports both direct Image and nested structure)
            Image orbImage = orbObject.GetComponent<Image>();
            
            // If not on root, search in children (for nested prefab structure)
            if (orbImage == null)
            {
                orbImage = orbObject.GetComponentInChildren<Image>();
            }

            if (orbImage == null)
            {
                Debug.LogError($"[StaminaOrbsUI] Orb prefab must have an Image component (either on root or in children)! Prefab: {orbPrefab.name}");
                Destroy(orbObject);
                continue;
            }

            Debug.Log($"[StaminaOrbsUI] Orb {i} spawned. Image found on: {orbImage.gameObject.name}");

            // Configure orb image
            orbImage.type = Image.Type.Filled;
            orbImage.fillMethod = Image.FillMethod.Radial360;
            orbImage.fillOrigin = (int)Image.Origin360.Bottom;
            orbImage.fillAmount = 1f;
            orbImage.color = fullOrbColor;

            spawnedOrbs.Add(orbImage);
            targetFillAmounts.Add(1f);
            currentFillAmounts.Add(1f);
        }

        Debug.Log($"[StaminaOrbsUI] Spawned {count} orbs successfully");
    }

    /// <summary>
    /// Clear the last 3 spawned orbs (if hay más de 3, elimina los últimos 3; si hay 3 o menos, elimina todos)
    /// </summary>
    private void ClearOrbs()
    {
        int count = spawnedOrbs.Count;

        // Si hay más de 3 orbes, elimina solo los últimos 3
        if (count > 3)
        {
            for (int i = count - 1; i >= count - 3; i--)
            {
                if (spawnedOrbs[i] != null && spawnedOrbs[i].gameObject != null)
                {
                    Destroy(spawnedOrbs[i].gameObject);
                }
                spawnedOrbs.RemoveAt(i);
                targetFillAmounts.RemoveAt(i);
                currentFillAmounts.RemoveAt(i);
            }
        }
        else
        {
            // Si hay 3 o menos, elimina todos
            foreach (Image orb in spawnedOrbs)
            {
                if (orb != null && orb.gameObject != null)
                {
                    Destroy(orb.gameObject);
                }
            }
            spawnedOrbs.Clear();
            targetFillAmounts.Clear();
            currentFillAmounts.Clear();
        }

        // También limpiar hijos huérfanos en el container (por si acaso)
        if (orbContainer != null)
        {
            foreach (Transform child in orbContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Setup horizontal layout group
    /// </summary>
    private void SetupLayout()
    {
        if (orbContainer == null) return;

        HorizontalLayoutGroup layoutGroup = orbContainer.GetComponent<HorizontalLayoutGroup>();
        
        if (layoutGroup == null)
        {
            layoutGroup = orbContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        layoutGroup.spacing = orbSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
    }

    /// <summary>
    /// Show or hide the stamina orbs
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// Validate required references
    /// </summary>
    private void ValidateReferences()
    {
        if (orbPrefab == null)
        {
            Debug.LogError($"[StaminaOrbsUI] Orb Prefab not assigned on {gameObject.name}!", this);
        }

        if (orbContainer == null)
        {
            Debug.LogError($"[StaminaOrbsUI] Orb Container not assigned on {gameObject.name}!", this);
        }
    }

    private void OnDestroy()
    {
        ClearOrbs();
    }

    #region Public Getters

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public int OrbCount => spawnedOrbs.Count;
    public float StaminaPerOrb => staminaPerOrb;

    #endregion
}
