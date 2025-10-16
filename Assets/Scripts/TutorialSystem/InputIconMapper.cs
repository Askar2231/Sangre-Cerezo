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

    [Header("Sprites de Teclado")]
    [SerializeField] private Sprite keyboardMove;
    [SerializeField] private Sprite keyboardRun;
    [SerializeField] private Sprite keyboardInteract;
    [SerializeField] private Sprite keyboardQTE;
    [SerializeField] private Sprite keyboardParry;
    [SerializeField] private Sprite keyboardConfirm;
    [SerializeField] private Sprite keyboardCancel;
    [SerializeField] private Sprite keyboardLightAttack;
    [SerializeField] private Sprite keyboardHeavyAttack;
    [SerializeField] private Sprite keyboardSkill1;
    [SerializeField] private Sprite keyboardSkill2;
    [SerializeField] private Sprite keyboardEndTurn;

    [Header("Sprites de Xbox")]
    [SerializeField] private Sprite xboxMove;
    [SerializeField] private Sprite xboxRun;
    [SerializeField] private Sprite xboxInteract;
    [SerializeField] private Sprite xboxQTE;
    [SerializeField] private Sprite xboxParry;
    [SerializeField] private Sprite xboxConfirm;
    [SerializeField] private Sprite xboxCancel;
    [SerializeField] private Sprite xboxLightAttack;
    [SerializeField] private Sprite xboxHeavyAttack;
    [SerializeField] private Sprite xboxSkill1;
    [SerializeField] private Sprite xboxSkill2;
    [SerializeField] private Sprite xboxEndTurn;

    [Header("Sprites de PlayStation")]
    [SerializeField] private Sprite psMove;
    [SerializeField] private Sprite psRun;
    [SerializeField] private Sprite psInteract;
    [SerializeField] private Sprite psQTE;
    [SerializeField] private Sprite psParry;
    [SerializeField] private Sprite psConfirm;
    [SerializeField] private Sprite psCancel;
    [SerializeField] private Sprite psLightAttack;
    [SerializeField] private Sprite psHeavyAttack;
    [SerializeField] private Sprite psSkill1;
    [SerializeField] private Sprite psSkill2;
    [SerializeField] private Sprite psEndTurn;

    [Header("Configuración")]
    [SerializeField] private bool debugMode = false;

    private InputDeviceType currentDeviceType = InputDeviceType.Keyboard;
    private Dictionary<InputAction, Sprite> currentIconSet;

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

    private void InitializeIconSets()
    {
        currentIconSet = new Dictionary<InputAction, Sprite>();
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
        currentIconSet.Clear();

        switch (currentDeviceType)
        {
            case InputDeviceType.Keyboard:
                currentIconSet[InputAction.Move] = keyboardMove;
                currentIconSet[InputAction.Run] = keyboardRun;
                currentIconSet[InputAction.Interact] = keyboardInteract;
                currentIconSet[InputAction.QTE] = keyboardQTE;
                currentIconSet[InputAction.Parry] = keyboardParry;
                currentIconSet[InputAction.Confirm] = keyboardConfirm;
                currentIconSet[InputAction.Cancel] = keyboardCancel;
                currentIconSet[InputAction.LightAttack] = keyboardLightAttack;
                currentIconSet[InputAction.HeavyAttack] = keyboardHeavyAttack;
                currentIconSet[InputAction.Skill1] = keyboardSkill1;
                currentIconSet[InputAction.Skill2] = keyboardSkill2;
                currentIconSet[InputAction.EndTurn] = keyboardEndTurn;
                break;

            case InputDeviceType.XboxController:
                currentIconSet[InputAction.Move] = xboxMove;
                currentIconSet[InputAction.Run] = xboxRun;
                currentIconSet[InputAction.Interact] = xboxInteract;
                currentIconSet[InputAction.QTE] = xboxQTE;
                currentIconSet[InputAction.Parry] = xboxParry;
                currentIconSet[InputAction.Confirm] = xboxConfirm;
                currentIconSet[InputAction.Cancel] = xboxCancel;
                currentIconSet[InputAction.LightAttack] = xboxLightAttack;
                currentIconSet[InputAction.HeavyAttack] = xboxHeavyAttack;
                currentIconSet[InputAction.Skill1] = xboxSkill1;
                currentIconSet[InputAction.Skill2] = xboxSkill2;
                currentIconSet[InputAction.EndTurn] = xboxEndTurn;
                break;

            case InputDeviceType.PlayStationController:
                currentIconSet[InputAction.Move] = psMove;
                currentIconSet[InputAction.Run] = psRun;
                currentIconSet[InputAction.Interact] = psInteract;
                currentIconSet[InputAction.QTE] = psQTE;
                currentIconSet[InputAction.Parry] = psParry;
                currentIconSet[InputAction.Confirm] = psConfirm;
                currentIconSet[InputAction.Cancel] = psCancel;
                currentIconSet[InputAction.LightAttack] = psLightAttack;
                currentIconSet[InputAction.HeavyAttack] = psHeavyAttack;
                currentIconSet[InputAction.Skill1] = psSkill1;
                currentIconSet[InputAction.Skill2] = psSkill2;
                currentIconSet[InputAction.EndTurn] = psEndTurn;
                break;

            case InputDeviceType.GenericGamepad:
                // Usar Xbox como fallback
                currentIconSet[InputAction.Move] = xboxMove;
                currentIconSet[InputAction.Run] = xboxRun;
                currentIconSet[InputAction.Interact] = xboxInteract;
                currentIconSet[InputAction.QTE] = xboxQTE;
                currentIconSet[InputAction.Parry] = xboxParry;
                currentIconSet[InputAction.Confirm] = xboxConfirm;
                currentIconSet[InputAction.Cancel] = xboxCancel;
                currentIconSet[InputAction.LightAttack] = xboxLightAttack;
                currentIconSet[InputAction.HeavyAttack] = xboxHeavyAttack;
                currentIconSet[InputAction.Skill1] = xboxSkill1;
                currentIconSet[InputAction.Skill2] = xboxSkill2;
                currentIconSet[InputAction.EndTurn] = xboxEndTurn;
                break;
        }
        
        // No longer need sprite indices - we use sprites directly
    }
    
    /// <summary>
    /// Obtiene el sprite de icono para una acción específica
    /// </summary>
    public Sprite GetIconForAction(InputAction action)
    {
        if (currentIconSet.TryGetValue(action, out Sprite icon))
        {
            return icon;
        }

        Debug.LogWarning($"No se encontró icono para la acción: {action} en dispositivo {currentDeviceType}");
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
    /// Gets sprite or text representation for an action
    /// Uses the assigned sprites directly - no sprite asset needed!
    /// </summary>
    private string GetSpriteOrText(InputAction action)
    {
        // Get the sprite for this action from our current icon set
        Sprite sprite = GetIconForAction(action);
        
        if (sprite != null)
        {
            // Use sprite name in TMP sprite tag
            // The sprite must be in a TMP Sprite Asset assigned to the TextMeshPro component
            string spriteName = sprite.name;
            spriteName = spriteName.Replace("(Clone)", "").Trim();
            
            if (debugMode)
            {
                Debug.Log($"[InputIconMapper] Action '{action}' → Sprite: '{spriteName}'");
            }
            
            // Return TMP sprite tag using sprite name
            return $"<sprite name=\"{spriteName}\">";
        }
        
        // Fallback to text if no sprite is assigned
        string textFallback = GetTextForAction(action);
        
        if (debugMode)
        {
            Debug.LogWarning($"[InputIconMapper] Action '{action}' → No sprite assigned, using text: {textFallback}");
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
