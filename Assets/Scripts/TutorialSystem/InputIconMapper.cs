using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema que mapea acciones de input a sus iconos correspondientes
/// según el dispositivo de entrada activo
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
        DontDestroyOnLoad(gameObject);

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

        // Build sprite name dictionary from mappings
        foreach (var mapping in spriteNameMappings)
        {
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
                keyboardSpriteName = "Shift",
                xboxSpriteName = "Xbox_R3",
                playStationSpriteName = "PS_R3"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Interact,
                keyboardSpriteName = "E",
                xboxSpriteName = "Xbox_A",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.QTE,
                keyboardSpriteName = "Space",
                xboxSpriteName = "Xbox_A",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Parry,
                keyboardSpriteName = "Space",
                xboxSpriteName = "Xbox_A",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Confirm,
                keyboardSpriteName = "Enter",
                xboxSpriteName = "Xbox_A",
                playStationSpriteName = "PS_Cross"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Cancel,
                keyboardSpriteName = "ESC",
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
                keyboardSpriteName = "Q",
                xboxSpriteName = "Xbox_LT",
                playStationSpriteName = "PS_L2"
            },
            new InputActionSpriteMap
            {
                action = InputAction.Skill2,
                keyboardSpriteName = "R",
                xboxSpriteName = "Xbox_RT",
                playStationSpriteName = "PS_R2"
            },
            new InputActionSpriteMap
            {
                action = InputAction.EndTurn,
                keyboardSpriteName = "Tab",
                xboxSpriteName = "Xbox_RB",
                playStationSpriteName = "PS_R1"
            }
        };
    }

    /// <summary>
    /// Obtiene el sprite de icono para una acción específica
    /// Returns null - use GetSpriteNameForAction instead for TMP sprite names
    /// </summary>
    [System.Obsolete("Use GetSpriteNameForAction instead")]
    public Sprite GetIconForAction(InputAction action)
    {
        Debug.LogWarning("GetIconForAction is deprecated. Use GetSpriteNameForAction instead.");
        return null;
    }

    /// <summary>
    /// Gets the sprite name for an action to use in TMP Sprite Assets
    /// </summary>
    public string GetSpriteNameForAction(InputAction action)
    {
        if (currentSpriteNameSet.TryGetValue(action, out string spriteName))
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
            default: return action.ToString();
        }
    }

    /// <summary>
    /// Reemplaza placeholders en texto con sprites (si están disponibles) o texto
    /// Ejemplo: "Presiona {Move} para moverte" -> "Presiona [sprite] para moverte"
    /// </summary>
    public string ProcessTextPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string processedText = text;

        // Reemplazar cada placeholder con sprite tag o texto
        processedText = processedText.Replace("{Move}", GetSpriteOrText(InputAction.Move));
        processedText = processedText.Replace("{Run}", GetSpriteOrText(InputAction.Run));
        processedText = processedText.Replace("{Interact}", GetSpriteOrText(InputAction.Interact));
        processedText = processedText.Replace("{QTE}", GetSpriteOrText(InputAction.QTE));
        processedText = processedText.Replace("{Parry}", GetSpriteOrText(InputAction.Parry));
        processedText = processedText.Replace("{Confirm}", GetSpriteOrText(InputAction.Confirm));
        processedText = processedText.Replace("{Cancel}", GetSpriteOrText(InputAction.Cancel));
        processedText = processedText.Replace("{LightAttack}", GetSpriteOrText(InputAction.LightAttack));
        processedText = processedText.Replace("{HeavyAttack}", GetSpriteOrText(InputAction.HeavyAttack));
        processedText = processedText.Replace("{Skill1}", GetSpriteOrText(InputAction.Skill1));
        processedText = processedText.Replace("{Skill2}", GetSpriteOrText(InputAction.Skill2));
        processedText = processedText.Replace("{EndTurn}", GetSpriteOrText(InputAction.EndTurn));

        return processedText;
    }

    /// <summary>
    /// Gets sprite or text representation for an action (PUBLIC)
    /// Returns TMP sprite tag using sprite name from atlas
    /// </summary>
    public string GetSpriteOrText(InputAction action)
    {
        // Get the sprite name for this action from our current sprite name set
        string spriteName = GetSpriteNameForAction(action);

        if (!string.IsNullOrEmpty(spriteName))
        {
            if (debugMode)
            {
                Debug.Log($"[InputIconMapper] Action '{action}' → Sprite: '{spriteName}'");
            }

            // Return TMP sprite tag using sprite name from atlas
            return $"<sprite name=\"{spriteName}\">";
        }

        // Fallback to text if no sprite name is assigned
        string textFallback = GetTextForAction(action);

        if (debugMode)
        {
            Debug.LogWarning($"[InputIconMapper] Action '{action}' → No sprite name assigned, using text: {textFallback}");
        }

        // Return formatted text as fallback
        return $"<b>[{textFallback}]</b>";
    }

    /// <summary>
    /// Get icon text/sprite for QTE button specifically
    /// </summary>
    public string GetIconForQTE()
    {
        return GetSpriteOrText(InputAction.QTE);
    }

    public InputDeviceType GetCurrentDeviceType()
    {
        return currentDeviceType;
    }
}
