using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema que mapea acciones de input a sus iconos correspondientes
/// según el dispositivo de entrada activo
/// 
/// COLOR TINTING GUIDE:
/// - Uses TMP sprite tint attribute: <sprite name="Key" tint=1 color=#RRGGBBAA>
/// - For color tinting to work, your sprites in the TMP Sprite Asset MUST be WHITE or GRAYSCALE
/// - TextMeshPro uses multiplicative blending: SpriteColor × TintColor = FinalColor
/// - If sprites are colored (e.g., blue buttons), tinting won't work as expected
/// - The tint=1 attribute MUST be present for color to apply
/// 
/// SOLUTIONS IF TINTING DOESN'T WORK:
/// 1. Best: Make your sprites white/grayscale in your image editor and re-import
/// 2. Check sprite import settings: Use RGBA format, not compressed
/// 3. Alternative: Use 'useAlphaOnly' mode for transparency effects instead of color
/// 4. Advanced: Create separate colored sprite sets for each theme
/// </summary>
public class InputIconMapper : MonoBehaviour
{
    #region Singleton
    private static InputIconMapper _instance;
    public static InputIconMapper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<InputIconMapper>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("InputIconMapper");
                    _instance = go.AddComponent<InputIconMapper>();
                }
            }
            return _instance;
        }
    }
    #endregion

    // Sprite mappings configuration
    [Header("Sprite Name Mappings")]
    [Tooltip("Map each input action to sprite names in your TMP Sprite Atlas")]
    [SerializeField] private List<InputActionSpriteMap> spriteNameMappings = new List<InputActionSpriteMap>();

    [Header("Configuración")]
    [SerializeField] private bool debugMode = false;
    
    [Header("Color/Tint Settings")]
    [Tooltip("Default tint color for input icons. Set to white (255,255,255) for no tint.")]
    [SerializeField] private Color defaultIconTint = Color.white;
    
    [Tooltip("Enable this to apply tint to all icons by default")]
    [SerializeField] private bool applyTintByDefault = false;
    
    [Tooltip("IMPORTANT: For color tinting to work, your sprites must be WHITE/GRAYSCALE in the sprite atlas. Colored sprites won't tint properly due to multiplicative blending.")]
    [SerializeField] private bool spritesAreWhiteOrGrayscale = true;
    
    [Header("Advanced Color Options")]
    [Tooltip("Use alpha channel for intensity instead of RGB tinting. Useful if sprites are colored.")]
    [SerializeField] private bool useAlphaOnly = false;

    private InputDeviceType currentDeviceType = InputDeviceType.Keyboard;
    private Dictionary<InputAction, string> currentSpriteNameSet;

    public delegate void DeviceChangedHandler(InputDeviceType newDevice);
    public event DeviceChangedHandler OnDeviceChanged;

    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // IMPORTANT: Don't use DontDestroyOnLoad for scene-specific objects
        // Only use if this GameObject is truly meant to persist across ALL scenes
        // Comment out to prevent "not cleaned up" warning:
        // DontDestroyOnLoad(gameObject);

        InitializeIconSets();
        DetectCurrentDevice();
    }

    private void OnEnable()
    {
        // Suscribirse a cambios de dispositivo del Input System
        if (InputSystem.devices.Count > 0)
        {
            InputSystem.onDeviceChange += OnInputDeviceChanged;
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnInputDeviceChanged;
    }

    private void OnDestroy()
    {
        // Clean up singleton reference when destroyed
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void InitializeIconSets()
    {
        currentSpriteNameSet = new Dictionary<InputAction, string>();

        // Initialize default mappings if none are set
        if (spriteNameMappings == null || spriteNameMappings.Count == 0)
        {
            InitializeDefaultMappings();
        }

        UpdateCurrentIconSet();
    }

    private void OnInputDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected)
        {
            DetectCurrentDevice();
        }
    }

    private void Update()
    {
        // Detectar cambio de dispositivo basado en input
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SetDeviceType(InputDeviceType.Keyboard);
        }
        else if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
        {
            SetDeviceType(InputDeviceType.Keyboard);
        }
        else if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            DetectGamepadType();
        }
    }

    private void DetectCurrentDevice()
    {
        // Priorizar gamepad si está conectado y fue usado recientemente
        if (Gamepad.current != null)
        {
            DetectGamepadType();
        }
        else
        {
            SetDeviceType(InputDeviceType.Keyboard);
        }
    }

    private void DetectGamepadType()
    {
        if (Gamepad.current == null) return;

        string deviceName = Gamepad.current.name.ToLower();

        if (deviceName.Contains("dualshock") || deviceName.Contains("playstation") || deviceName.Contains("ps4") || deviceName.Contains("ps5"))
        {
            SetDeviceType(InputDeviceType.PlayStationController);
        }
        else if (deviceName.Contains("xbox"))
        {
            SetDeviceType(InputDeviceType.XboxController);
        }
        else
        {
            // Genérico - usar Xbox como fallback
            SetDeviceType(InputDeviceType.XboxController);
        }
    }

    private void SetDeviceType(InputDeviceType newType)
    {
        if (currentDeviceType != newType)
        {
            currentDeviceType = newType;
            UpdateCurrentIconSet();
            OnDeviceChanged?.Invoke(newType);

            if (debugMode) Debug.Log($"Dispositivo de entrada cambiado a: {currentDeviceType}");
        }
    }

    private void UpdateCurrentIconSet()
    {
        currentSpriteNameSet.Clear();

        // Safety check
        if (spriteNameMappings == null || spriteNameMappings.Count == 0)
        {
            if (debugMode) Debug.LogWarning("[InputIconMapper] No sprite name mappings available");
            return;
        }

        // Build sprite name dictionary from mappings
        foreach (var mapping in spriteNameMappings)
        {
            // Skip null mappings
            if (mapping == null) continue;
            
            string spriteName = "";

            switch (currentDeviceType)
            {
                case InputDeviceType.Keyboard:
                    spriteName = mapping.keyboardSpriteName;
                    break;
                case InputDeviceType.XboxController:
                case InputDeviceType.GenericGamepad:
                    spriteName = mapping.xboxSpriteName;
                    break;
                case InputDeviceType.PlayStationController:
                    spriteName = mapping.playStationSpriteName;
                    break;
            }

            if (!string.IsNullOrEmpty(spriteName))
            {
                currentSpriteNameSet[mapping.action] = spriteName;
            }
        }
    }

    /// <summary>
    /// Initialize default sprite name mappings - you can customize these
    /// </summary>
    private void InitializeDefaultMappings()
    {
        spriteNameMappings = new List<InputActionSpriteMap>
        {
            new InputActionSpriteMap
            {
                action = InputAction.Move,
                keyboardSpriteName = "WASD",
                xboxSpriteName = "Xbox_LS",
                playStationSpriteName = "PS_LS"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Run,
                keyboardSpriteName = "Key_Shift",
                xboxSpriteName = "Xbox_R3",
                playStationSpriteName = "PS_R3"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Interact,
                keyboardSpriteName = "Key_E",
                xboxSpriteName = "Xbox_X",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.QTE,
                keyboardSpriteName = "Key_Space",
                xboxSpriteName = "Xbox_X",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Parry,
                keyboardSpriteName = "Key_Space",
                xboxSpriteName = "Xbox_B",
                playStationSpriteName = "PS_Circle"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Confirm,
                keyboardSpriteName = "Key_Enter",
                xboxSpriteName = "Xbox_X",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Cancel,
                keyboardSpriteName = "Key_ESC",
                xboxSpriteName = "Xbox_B",
                playStationSpriteName = "PS_Circle"
            },
            new InputActionSpriteMap
            {
                action = InputAction.LightAttack,
                keyboardSpriteName = "Mouse_Left",
                xboxSpriteName = "Xbox_X",
                playStationSpriteName = "PS_Square"
            },
            new InputActionSpriteMap
            {
                action = InputAction.HeavyAttack,
                keyboardSpriteName = "Mouse_Right",
                xboxSpriteName = "Xbox_Y",
                playStationSpriteName = "PS_Triangle"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Skill1,
                keyboardSpriteName = "Key_Q",
                xboxSpriteName = "Xbox_LT",
                playStationSpriteName = "PS_L1"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Skill2,
                keyboardSpriteName = "Key_R",
                xboxSpriteName = "Xbox_RT",
                playStationSpriteName = "PS_R1"
            },
            new InputActionSpriteMap
            {
                action = InputAction.EndTurn,
                keyboardSpriteName = "Key_Tab",
                xboxSpriteName = "Xbox_RB",
                playStationSpriteName = "PS_R2"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Choice1,
                keyboardSpriteName = "Key_Q",
                xboxSpriteName = "Xbox_Y",
                playStationSpriteName = "PS_Triangle"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Choice2,
                keyboardSpriteName = "Key_R",
                xboxSpriteName = "Xbox_B",
                playStationSpriteName = "PS_Circle"
            }
        };
    }

    /// <summary>
    /// Gets the sprite name for an action to use in TMP Sprite Assets
    /// </summary>
    public string GetSpriteNameForAction(InputAction action)
    {
        // Safety check
        if (currentSpriteNameSet == null)
        {
            if (debugMode) Debug.LogWarning("[InputIconMapper] currentSpriteNameSet is null, reinitializing...");
            InitializeIconSets();
        }
        
        if (currentSpriteNameSet != null && currentSpriteNameSet.TryGetValue(action, out string spriteName))
        {
            return spriteName;
        }

        if (debugMode)
        {
            Debug.LogWarning($"No se encontró sprite name para la acción: {action} en dispositivo {currentDeviceType}");
        }
        return null;
    }

    /// <summary>
    /// Obtiene el texto representativo para una acción (fallback si no hay sprite)
    /// </summary>
    public string GetTextForAction(InputAction action)
    {
        switch (currentDeviceType)
        {
            case InputDeviceType.Keyboard:
                return GetKeyboardText(action);
            case InputDeviceType.XboxController:
                return GetXboxText(action);
            case InputDeviceType.PlayStationController:
                return GetPlayStationText(action);
            default:
                return GetXboxText(action);
        }
    }

    private string GetKeyboardText(InputAction action)
    {
        switch (action)
        {
            case InputAction.Move: return "WASD";
            case InputAction.Run: return "Shift";
            case InputAction.Interact: return "E";
            case InputAction.QTE: return "Espacio";
            case InputAction.Parry: return "Espacio";
            case InputAction.Confirm: return "Enter";
            case InputAction.Cancel: return "ESC";
            case InputAction.LightAttack: return "Clic Izquierdo";
            case InputAction.HeavyAttack: return "Clic Derecho";
            case InputAction.Skill1: return "Q";
            case InputAction.Skill2: return "R";
            case InputAction.EndTurn: return "Tab";
            case InputAction.Choice1: return "Q";
            case InputAction.Choice2: return "R";
            default: return action.ToString();
        }
    }

    private string GetXboxText(InputAction action)
    {
        switch (action)
        {
            case InputAction.Move: return "Stick Izq.";
            case InputAction.Run: return "LT";
            case InputAction.Interact: return "A";
            case InputAction.QTE: return "A";
            case InputAction.Parry: return "A";
            case InputAction.Confirm: return "A";
            case InputAction.Cancel: return "B";
            case InputAction.LightAttack: return "X";
            case InputAction.HeavyAttack: return "Y";
            case InputAction.Skill1: return "LT";
            case InputAction.Skill2: return "RT";
            case InputAction.EndTurn: return "RB";
            case InputAction.Choice1: return "X";
            case InputAction.Choice2: return "B";
            default: return action.ToString();
        }
    }

    private string GetPlayStationText(InputAction action)
    {
        switch (action)
        {
            case InputAction.Move: return "Stick Izq.";
            case InputAction.Run: return "L2";
            case InputAction.Interact: return "Cruz";
            case InputAction.QTE: return "Cruz";
            case InputAction.Parry: return "Cruz";
            case InputAction.Confirm: return "Cruz";
            case InputAction.Cancel: return "Círculo";
            case InputAction.LightAttack: return "Cuadrado";
            case InputAction.HeavyAttack: return "Triángulo";
            case InputAction.Skill1: return "L2";
            case InputAction.Skill2: return "R2";
            case InputAction.EndTurn: return "R1";
            case InputAction.Choice1: return "Cuadrado";
            case InputAction.Choice2: return "Círculo";
            default: return action.ToString();
        }
    }

    /// <summary>
    /// Reemplaza placeholders en texto con sprites (si están disponibles) o texto
    /// Ejemplo: "Presiona {Move} para moverte" -> "Presiona [sprite] para moverte"
    /// </summary>
    public string ProcessTextPlaceholders(string text)
    {
        return ProcessTextPlaceholders(text, null);
    }
    
    /// <summary>
    /// Reemplaza placeholders en texto con sprites con tint personalizado
    /// </summary>
    /// <param name="text">Texto con placeholders como {Move}, {Attack}, etc.</param>
    /// <param name="tintColor">Color para tintar los sprites. Null para usar configuración por defecto.</param>
    public string ProcessTextPlaceholders(string text, Color? tintColor)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string processedText = text;
        
        // Use default tint if none specified and applyTintByDefault is enabled
        Color? colorToUse = tintColor;
        if (!colorToUse.HasValue && applyTintByDefault)
        {
            colorToUse = defaultIconTint;
        }

        // Reemplazar cada placeholder con sprite tag o texto
        processedText = processedText.Replace("{Move}", GetSpriteOrText(InputAction.Move, colorToUse));
        processedText = processedText.Replace("{Run}", GetSpriteOrText(InputAction.Run, colorToUse));
        processedText = processedText.Replace("{Interact}", GetSpriteOrText(InputAction.Interact, colorToUse));
        processedText = processedText.Replace("{QTE}", GetSpriteOrText(InputAction.QTE, colorToUse));
        processedText = processedText.Replace("{Parry}", GetSpriteOrText(InputAction.Parry, colorToUse));
        processedText = processedText.Replace("{Confirm}", GetSpriteOrText(InputAction.Confirm, colorToUse));
        processedText = processedText.Replace("{Cancel}", GetSpriteOrText(InputAction.Cancel, colorToUse));
        processedText = processedText.Replace("{LightAttack}", GetSpriteOrText(InputAction.LightAttack, colorToUse));
        processedText = processedText.Replace("{HeavyAttack}", GetSpriteOrText(InputAction.HeavyAttack, colorToUse));
        processedText = processedText.Replace("{Skill1}", GetSpriteOrText(InputAction.Skill1, colorToUse));
        processedText = processedText.Replace("{Skill2}", GetSpriteOrText(InputAction.Skill2, colorToUse));
        processedText = processedText.Replace("{EndTurn}", GetSpriteOrText(InputAction.EndTurn, colorToUse));
        processedText = processedText.Replace("{Choice1}", GetSpriteOrText(InputAction.Choice1, colorToUse));
        processedText = processedText.Replace("{Choice2}", GetSpriteOrText(InputAction.Choice2, colorToUse));

        return processedText;
    }

    /// <summary>
    /// Gets sprite or text representation for an action (PUBLIC)
    /// Returns TMP sprite tag using sprite name from atlas, with fallback to text if sprite doesn't exist
    /// </summary>
    public string GetSpriteOrText(InputAction action)
    {
        if (applyTintByDefault)
        {
            return GetSpriteOrText(action, defaultIconTint);
        }
        
        return GetSpriteOrText(action, null);
    }
    
    /// <summary>
    /// Gets sprite or text representation for an action with custom tint color
    /// </summary>
    /// <param name="action">The input action to get icon for</param>
    /// <param name="tintColor">Color to tint the sprite. Pass null for no tint.</param>
    public string GetSpriteOrText(InputAction action, Color? tintColor)
    {
        // Get the sprite name for this action from our current sprite name set
        string spriteName = GetSpriteNameForAction(action);

        if (!string.IsNullOrEmpty(spriteName))
        {
            if (debugMode)
            {
                Debug.Log($"[InputIconMapper] Action '{action}' → Sprite: '{spriteName}' with tint: {(tintColor.HasValue ? tintColor.Value.ToString() : "none")}");
            }

            // Return TMP sprite tag using sprite name from atlas with optional color
            // Note: If the sprite doesn't exist in the TMP Sprite Asset at runtime,
            // TextMeshPro will display the sprite tag as text (e.g., "<sprite name=\"Key_E\">")
            // To prevent this, make sure all sprite names in your mappings exist in your TMP Sprite Asset
            
            if (tintColor.HasValue && tintColor.Value != Color.white)
            {
                Color colorToApply = tintColor.Value;
                
                // If using alpha only, convert to white with alpha
                if (useAlphaOnly)
                {
                    colorToApply = new Color(1f, 1f, 1f, tintColor.Value.a);
                }
                
                // Apply color tint using TMP sprite tint attribute
                // NOTE: TMP sprites support tint attribute directly!
                string hexColor = ColorUtility.ToHtmlStringRGBA(colorToApply);
                return $"<sprite name=\"{spriteName}\" tint=1 color=#{hexColor}>";
            }
            else
            {
                return $"<sprite name=\"{spriteName}\">";
            }
        }

        // Fallback to text if no sprite name is assigned
        string textFallback = GetTextForAction(action);

        if (debugMode)
        {
            Debug.LogWarning($"[InputIconMapper] Action '{action}' → No sprite name assigned, using text: {textFallback}");
        }

        // Return formatted text as fallback (with color if specified)
        if (tintColor.HasValue && tintColor.Value != Color.white)
        {
            string hexColor = ColorUtility.ToHtmlStringRGBA(tintColor.Value);
            return $"<color=#{hexColor}><b>[{textFallback}]</b></color>";
        }
        else
        {
            return $"<b>[{textFallback}]</b>";
        }
    }

    /// <summary>
    /// Get icon text/sprite for QTE button specifically
    /// </summary>
    public string GetIconForQTE()
    {
        return GetSpriteOrText(InputAction.QTE);
    }
    
    /// <summary>
    /// Get icon text/sprite for QTE button with custom tint
    /// </summary>
    public string GetIconForQTE(Color tintColor)
    {
        return GetSpriteOrText(InputAction.QTE, tintColor);
    }
    
    /// <summary>
    /// Set the default tint color for icons at runtime
    /// </summary>
    public void SetDefaultIconTint(Color color)
    {
        defaultIconTint = color;
    }
    
    /// <summary>
    /// Get the current default icon tint color
    /// </summary>
    public Color GetDefaultIconTint()
    {
        return defaultIconTint;
    }
    
    /// <summary>
    /// Enable or disable automatic tinting with default color
    /// </summary>
    public void SetApplyTintByDefault(bool apply)
    {
        applyTintByDefault = apply;
    }
    
    /// <summary>
    /// Generate a test string with multiple colored sprites for visual testing
    /// Display this in a TextMeshProUGUI to see if tinting works
    /// </summary>
    [ContextMenu("Generate Tint Test String")]
    public string GenerateTintTestString()
    {
        string testString = "Tint Test:\n";
        testString += $"Red: {GetSpriteOrText(InputAction.Interact, Color.red)}\n";
        testString += $"Blue: {GetSpriteOrText(InputAction.Interact, Color.blue)}\n";
        testString += $"Green: {GetSpriteOrText(InputAction.Interact, Color.green)}\n";
        testString += $"Yellow: {GetSpriteOrText(InputAction.Interact, Color.yellow)}\n";
        testString += $"Orange: {GetSpriteOrText(InputAction.Interact, new Color(1f, 0.5f, 0f))}\n";
        testString += $"Cyan: {GetSpriteOrText(InputAction.Interact, Color.cyan)}\n";
        testString += $"Magenta: {GetSpriteOrText(InputAction.Interact, Color.magenta)}\n";
        testString += $"White: {GetSpriteOrText(InputAction.Interact, Color.white)}\n";
        testString += $"Gray: {GetSpriteOrText(InputAction.Interact, Color.gray)}\n";
        testString += $"50% Alpha: {GetSpriteOrText(InputAction.Interact, new Color(1f, 1f, 1f, 0.5f))}\n";
        
        Debug.Log("Generated test string - copy this to a TextMeshProUGUI component to test tinting:");
        Debug.Log(testString);
        
        return testString;
    }
    
    /// <summary>
    /// Test color tinting - prints debug info and returns a test string
    /// Call this from Unity console or inspector to diagnose tinting issues
    /// </summary>
    [ContextMenu("Test Color Tinting")]
    public string TestColorTinting()
    {
        Debug.Log("=== INPUT ICON COLOR TINT TEST ===");
        Debug.Log($"Apply Tint By Default: {applyTintByDefault}");
        Debug.Log($"Default Icon Tint: {defaultIconTint}");
        Debug.Log($"Sprites Are White/Grayscale: {spritesAreWhiteOrGrayscale}");
        Debug.Log($"Use Alpha Only: {useAlphaOnly}");
        
        if (!spritesAreWhiteOrGrayscale)
        {
            Debug.LogWarning("⚠️ WARNING: Sprites are marked as colored. Color tinting may not work!");
            Debug.LogWarning("For tinting to work, sprites in your TMP Sprite Asset should be WHITE or GRAYSCALE.");
            Debug.LogWarning("SOLUTIONS:");
            Debug.LogWarning("1. Edit your sprite images to be white/grayscale and re-import to Unity");
            Debug.LogWarning("2. Enable 'Use Alpha Only' for transparency effects instead");
            Debug.LogWarning("3. Create separate colored sprite atlases for different themes");
        }
        
        // Test with different colors
        string testRed = GetSpriteOrText(InputAction.Interact, Color.red);
        string testBlue = GetSpriteOrText(InputAction.Interact, Color.blue);
        string testYellow = GetSpriteOrText(InputAction.Interact, Color.yellow);
        string testSemiTransparent = GetSpriteOrText(InputAction.Interact, new Color(1f, 1f, 1f, 0.5f));
        string testOrange = GetSpriteOrText(InputAction.Interact, new Color(1f, 0.5f, 0f));
        
        Debug.Log($"Red tinted: {testRed}");
        Debug.Log($"Blue tinted: {testBlue}");
        Debug.Log($"Yellow tinted: {testYellow}");
        Debug.Log($"Semi-transparent: {testSemiTransparent}");
        Debug.Log($"Orange: {testOrange}");
        
        Debug.Log("=== TMP SPRITE TINT FORMAT ===");
        Debug.Log("TMP sprites use: <sprite name=\"SpriteName\" tint=1 color=#RRGGBBAA>");
        Debug.Log("The 'tint=1' attribute enables color tinting for that specific sprite");
        Debug.Log("Without 'tint=1', the color attribute is ignored");
        
        Debug.Log("=== END TEST ===");
        
        return $"Test: Red: {testRed} Blue: {testBlue} Yellow: {testYellow}";
    }

    public InputDeviceType GetCurrentDeviceType()
    {
        return currentDeviceType;
    }
}
