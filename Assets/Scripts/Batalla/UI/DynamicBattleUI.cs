using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Dynamically generates battle UI buttons from ScriptableObject data
/// Shows correct input sprites based on current device (keyboard/gamepad)
/// </summary>
public class DynamicBattleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerBattleController playerController;
    [SerializeField] private BattleManagerV2 battleManager;
    
    [Header("UI Containers")]
    [SerializeField] private Transform attackButtonsContainer;
    [SerializeField] private Transform skillButtonsContainer;
    [SerializeField] private Button endTurnButton;
    
    [Header("Button Prefab")]
    [SerializeField] private GameObject actionButtonPrefab;
    
    [Header("Settings")]
    [SerializeField] private bool autoGenerateOnStart = true;
    [SerializeField] private bool updateOnDeviceChange = true;
    [SerializeField] private bool autoSetupLayoutGroups = true;
    
    [Header("Layout Settings")]
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private bool forceExpandButtons = true;
    [SerializeField] private bool useVerticalLayoutForAttacks = true;
    [SerializeField] private float attackButtonSpacing = 40f; // Larger gap for vertical layout
    [SerializeField] private bool maintainAspectRatio = true;
    
    private List<BattleActionButton> generatedButtons = new List<BattleActionButton>();
    private bool isGenerated = false;
    
    private void Start()
    {
        // Setup layout groups if needed
        if (autoSetupLayoutGroups)
        {
            SetupLayoutGroups();
        }
        
        if (autoGenerateOnStart)
        {
            GenerateUI();
        }
        
        // Subscribe to device changes
        if (updateOnDeviceChange && InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged += OnInputDeviceChanged;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from device changes
        if (InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged -= OnInputDeviceChanged;
        }
    }
    
    /// <summary>
    /// Generate all UI buttons from ScriptableObject data
    /// </summary>
    public void GenerateUI()
    {
        if (isGenerated)
        {
            Debug.LogWarning("UI already generated. Call ClearUI() first to regenerate.");
            return;
        }
        
        if (playerController == null)
        {
            Debug.LogError("PlayerBattleController not assigned to DynamicBattleUI!");
            return;
        }
        
        // Generate attack buttons
        GenerateAttackButtons();
        
        // Generate skill buttons
        GenerateSkillButtons();
        
        // Setup end turn button
        SetupEndTurnButton();
        
        isGenerated = true;
        Debug.Log($"Generated {generatedButtons.Count} battle UI buttons");
    }
    
    /// <summary>
    /// Clear all generated UI
    /// </summary>
    public void ClearUI()
    {
        foreach (var button in generatedButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Destroy(button.gameObject);
            }
        }
        
        generatedButtons.Clear();
        isGenerated = false;
    }
    
    /// <summary>
    /// Regenerate UI (useful when device changes)
    /// </summary>
    public void RegenerateUI()
    {
        ClearUI();
        GenerateUI();
    }
    
    /// <summary>
    /// Update existing buttons with new device sprites (faster than regenerating)
    /// </summary>
    public void UpdateButtonSprites()
    {
        foreach (var button in generatedButtons)
        {
            if (button != null)
            {
                button.UpdateInputDisplay();
            }
        }
    }
    
    /// <summary>
    /// Generate attack buttons from AttackAnimationData
    /// </summary>
    private void GenerateAttackButtons()
    {
        if (attackButtonsContainer == null)
        {
            Debug.LogWarning("Attack buttons container not assigned!");
            return;
        }
        
        // Get attacks from PlayerBattleController via reflection or add public getter
        var lightAttack = GetLightAttackData();
        var heavyAttack = GetHeavyAttackData();
        
        if (lightAttack != null)
        {
            CreateAttackButton(lightAttack, attackButtonsContainer, false);
        }
        
        if (heavyAttack != null)
        {
            CreateAttackButton(heavyAttack, attackButtonsContainer, true);
        }
    }
    
    /// <summary>
    /// Generate skill buttons from SkillData array
    /// </summary>
    private void GenerateSkillButtons()
    {
        if (skillButtonsContainer == null)
        {
            Debug.LogWarning("Skill buttons container not assigned!");
            return;
        }
        
        var skills = GetSkillsData();
        if (skills == null || skills.Length == 0)
        {
            Debug.Log("No skills to generate buttons for");
            return;
        }
        
        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] != null)
            {
                CreateSkillButton(skills[i], i, skillButtonsContainer);
            }
        }
    }
    
    /// <summary>
    /// Create a button for an attack
    /// </summary>
    private void CreateAttackButton(AttackAnimationData attackData, Transform parent, bool isHeavy)
    {
        if (actionButtonPrefab == null)
        {
            Debug.LogError("Action button prefab not assigned!");
            return;
        }
        
        GameObject buttonObj = Instantiate(actionButtonPrefab, parent);
        buttonObj.name = $"{attackData.displayName}Button";
        
        // Setup layout for responsive sizing
        SetupButtonLayout(buttonObj);
        
        var battleButton = buttonObj.GetComponent<BattleActionButton>();
        if (battleButton == null)
        {
            battleButton = buttonObj.AddComponent<BattleActionButton>();
        }
        
        // Initialize button
        battleButton.Initialize(
            attackData.displayName,
            attackData.description,
            attackData.inputAction,
            () => {
                if (isHeavy)
                {
                    ExecuteHeavyAttack();
                }
                else
                {
                    ExecuteLightAttack();
                }
            }
        );
        
        generatedButtons.Add(battleButton);
    }
    
    /// <summary>
    /// Create a button for a skill
    /// </summary>
    private void CreateSkillButton(SkillData skillData, int skillIndex, Transform parent)
    {
        if (actionButtonPrefab == null)
        {
            Debug.LogError("Action button prefab not assigned!");
            return;
        }
        
        GameObject buttonObj = Instantiate(actionButtonPrefab, parent);
        buttonObj.name = $"{skillData.skillName}Button";
        
        // Setup layout for responsive sizing
        SetupButtonLayout(buttonObj);
        
        var battleButton = buttonObj.GetComponent<BattleActionButton>();
        if (battleButton == null)
        {
            battleButton = buttonObj.AddComponent<BattleActionButton>();
        }
        
        // Initialize button
        battleButton.Initialize(
            skillData.skillName,
            skillData.description,
            skillData.inputAction,
            () => ExecuteSkill(skillIndex)
        );
        
        generatedButtons.Add(battleButton);
    }
    
    /// <summary>
    /// Setup the end turn button
    /// </summary>
    private void SetupEndTurnButton()
    {
        if (endTurnButton == null)
        {
            Debug.LogWarning("End turn button not assigned!");
            return;
        }
        
        // Add click listener
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        
        // Update button text with input sprite
        var textComponent = endTurnButton.GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            // You can create an InputActionReference for EndTurn or just use text
            textComponent.text = "End Turn";
        }
    }
    
    /// <summary>
    /// Handle device changed event
    /// </summary>
    private void OnInputDeviceChanged(InputDeviceType newDevice)
    {
        Debug.Log($"Device changed to {newDevice}, updating button sprites");
        UpdateButtonSprites();
    }
    
    #region Action Execution
    
    private void ExecuteLightAttack()
    {
        if (playerController != null && battleManager != null)
        {
            var target = battleManager.GetCurrentEnemy();
            if (target != null)
            {
                playerController.ExecuteLightAttack(target);
            }
        }
    }
    
    private void ExecuteHeavyAttack()
    {
        if (playerController != null && battleManager != null)
        {
            var target = battleManager.GetCurrentEnemy();
            if (target != null)
            {
                playerController.ExecuteHeavyAttack(target);
            }
        }
    }
    
    private void ExecuteSkill(int skillIndex)
    {
        if (playerController != null && battleManager != null)
        {
            var target = battleManager.GetCurrentEnemy();
            if (target != null)
            {
                playerController.ExecuteSkill(skillIndex, target);
            }
        }
    }
    
    private void OnEndTurnClicked()
    {
        if (battleManager != null)
        {
            // Signal battle manager to end player turn
            Debug.Log("End Turn button clicked");
        }
    }
    
    #endregion
    
    #region Helper Methods to Get Data from PlayerBattleController
    
    // These methods access private fields - you may want to add public getters to PlayerBattleController instead
    private AttackAnimationData GetLightAttackData()
    {
        var field = typeof(PlayerBattleController).GetField("lightAttackData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(playerController) as AttackAnimationData;
    }
    
    private AttackAnimationData GetHeavyAttackData()
    {
        var field = typeof(PlayerBattleController).GetField("heavyAttackData", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(playerController) as AttackAnimationData;
    }
    
    private SkillData[] GetSkillsData()
    {
        var field = typeof(PlayerBattleController).GetField("availableSkills", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(playerController) as SkillData[];
    }
    
    #endregion
    
    #region Layout Setup
    
    /// <summary>
    /// Automatically setup layout groups on containers for proper button arrangement
    /// </summary>
    private void SetupLayoutGroups()
    {
        // Setup attack buttons container (can use vertical layout)
        if (attackButtonsContainer != null)
        {
            if (useVerticalLayoutForAttacks)
            {
                SetupVerticalContainerLayout(attackButtonsContainer, "Attack Buttons", attackButtonSpacing);
            }
            else
            {
                SetupContainerLayout(attackButtonsContainer, "Attack Buttons");
            }
        }
        
        // Setup skill buttons container (horizontal by default)
        if (skillButtonsContainer != null)
        {
            SetupContainerLayout(skillButtonsContainer, "Skill Buttons");
        }
    }
    
    /// <summary>
    /// Setup vertical layout group on a specific container
    /// </summary>
    private void SetupVerticalContainerLayout(Transform container, string containerName, float spacing)
    {
        // Ensure RectTransform exists
        RectTransform rectTransform = container.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning($"{containerName} is not a RectTransform! Adding RectTransform component.");
            rectTransform = container.gameObject.AddComponent<RectTransform>();
        }
        
        // Remove any existing horizontal layout
        HorizontalLayoutGroup horizontalLayout = container.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayout != null)
        {
            DestroyImmediate(horizontalLayout);
            Debug.Log($"Removed HorizontalLayoutGroup from {containerName}");
        }
        
        // Check if vertical layout already exists
        VerticalLayoutGroup verticalLayout = container.GetComponent<VerticalLayoutGroup>();
        
        if (verticalLayout == null)
        {
            // Add vertical layout group
            verticalLayout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            Debug.Log($"Added VerticalLayoutGroup to {containerName}");
        }
        
        // Configure vertical layout
        verticalLayout.spacing = spacing;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlWidth = false; // Don't force width - let buttons maintain aspect ratio
        verticalLayout.childControlHeight = false; // Don't force height - let buttons maintain aspect ratio
        verticalLayout.childForceExpandWidth = false;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childScaleWidth = true; // Allow uniform scaling
        verticalLayout.childScaleHeight = true; // Allow uniform scaling
        
        Debug.Log($"Configured VerticalLayoutGroup on {containerName} with {spacing}px spacing");
    }
    
    /// <summary>
    /// Setup layout group on a specific container
    /// </summary>
    private void SetupContainerLayout(Transform container, string containerName)
    {
        // Ensure RectTransform exists
        RectTransform rectTransform = container.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning($"{containerName} is not a RectTransform! Adding RectTransform component.");
            rectTransform = container.gameObject.AddComponent<RectTransform>();
        }
        
        // Check if layout group already exists
        HorizontalLayoutGroup horizontalLayout = container.GetComponent<HorizontalLayoutGroup>();
        VerticalLayoutGroup verticalLayout = container.GetComponent<VerticalLayoutGroup>();
        
        if (horizontalLayout == null && verticalLayout == null)
        {
            // Add horizontal layout group (default for battle UI)
            horizontalLayout = container.gameObject.AddComponent<HorizontalLayoutGroup>();
            
            // Configure layout
            horizontalLayout.spacing = buttonSpacing;
            horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayout.childControlWidth = true;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = forceExpandButtons;
            horizontalLayout.childForceExpandHeight = false;
            
            Debug.Log($"Added HorizontalLayoutGroup to {containerName}");
        }
        else
        {
            // Update existing layout settings
            if (horizontalLayout != null)
            {
                horizontalLayout.spacing = buttonSpacing;
                horizontalLayout.childControlWidth = true;
                horizontalLayout.childControlHeight = true;
                horizontalLayout.childForceExpandWidth = forceExpandButtons;
                Debug.Log($"Updated existing HorizontalLayoutGroup on {containerName}");
            }
            else if (verticalLayout != null)
            {
                verticalLayout.spacing = buttonSpacing;
                verticalLayout.childControlWidth = true;
                verticalLayout.childControlHeight = true;
                verticalLayout.childForceExpandWidth = forceExpandButtons;
                Debug.Log($"Updated existing VerticalLayoutGroup on {containerName}");
            }
        }
        
        // Ensure LayoutElement on buttons for proper sizing
        // This will be handled when buttons are created
    }
    
    /// <summary>
    /// Setup layout element on a button for proper sizing
    /// </summary>
    private void SetupButtonLayout(GameObject buttonObj)
    {
        LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = buttonObj.AddComponent<LayoutElement>();
        }
        
        // Add AspectRatioFitter to maintain button proportions
        if (maintainAspectRatio)
        {
            AspectRatioFitter aspectFitter = buttonObj.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = buttonObj.AddComponent<AspectRatioFitter>();
            }
            
            // Get the original size from RectTransform to calculate aspect ratio
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float aspectRatio = rectTransform.rect.width / rectTransform.rect.height;
                
                // If aspect ratio couldn't be calculated from rect, use default
                if (float.IsNaN(aspectRatio) || aspectRatio == 0)
                {
                    aspectRatio = 2.5f; // Default aspect ratio (e.g., 150x60 = 2.5)
                }
                
                aspectFitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
                aspectFitter.aspectRatio = aspectRatio;
            }
        }
        
        // Configure layout element
        if (forceExpandButtons)
        {
            layoutElement.flexibleWidth = 1f;
            layoutElement.minWidth = 100f;
        }
        else
        {
            layoutElement.preferredWidth = 150f;
        }
        
        layoutElement.preferredHeight = 60f;
    }
    
    #endregion
}
