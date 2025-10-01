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

    [Header("Sprites de Xbox")]
    [SerializeField] private Sprite xboxMove;
    [SerializeField] private Sprite xboxRun;
    [SerializeField] private Sprite xboxInteract;
    [SerializeField] private Sprite xboxQTE;
    [SerializeField] private Sprite xboxParry;
    [SerializeField] private Sprite xboxConfirm;
    [SerializeField] private Sprite xboxCancel;

    [Header("Sprites de PlayStation")]
    [SerializeField] private Sprite psMove;
    [SerializeField] private Sprite psRun;
    [SerializeField] private Sprite psInteract;
    [SerializeField] private Sprite psQTE;
    [SerializeField] private Sprite psParry;
    [SerializeField] private Sprite psConfirm;
    [SerializeField] private Sprite psCancel;

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
                break;

            case InputDeviceType.XboxController:
                currentIconSet[InputAction.Move] = xboxMove;
                currentIconSet[InputAction.Run] = xboxRun;
                currentIconSet[InputAction.Interact] = xboxInteract;
                currentIconSet[InputAction.QTE] = xboxQTE;
                currentIconSet[InputAction.Parry] = xboxParry;
                currentIconSet[InputAction.Confirm] = xboxConfirm;
                currentIconSet[InputAction.Cancel] = xboxCancel;
                break;

            case InputDeviceType.PlayStationController:
                currentIconSet[InputAction.Move] = psMove;
                currentIconSet[InputAction.Run] = psRun;
                currentIconSet[InputAction.Interact] = psInteract;
                currentIconSet[InputAction.QTE] = psQTE;
                currentIconSet[InputAction.Parry] = psParry;
                currentIconSet[InputAction.Confirm] = psConfirm;
                currentIconSet[InputAction.Cancel] = psCancel;
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
                break;
        }
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
            case InputAction.Attack: return "Clic Izquierdo";
            case InputAction.Skill: return "Clic Derecho";
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
            case InputAction.Attack: return "X";
            case InputAction.Skill: return "Y";
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
            case InputAction.Attack: return "Cuadrado";
            case InputAction.Skill: return "Triángulo";
            case InputAction.EndTurn: return "R1";
            default: return action.ToString();
        }
    }

    /// <summary>
    /// Reemplaza placeholders en texto con nombres de acciones
    /// Ejemplo: "Presiona {Move} para moverte" -> "Presiona WASD para moverte"
    /// </summary>
    public string ProcessTextPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string processedText = text;

        // Reemplazar cada placeholder
        processedText = processedText.Replace("{Move}", GetTextForAction(InputAction.Move));
        processedText = processedText.Replace("{Run}", GetTextForAction(InputAction.Run));
        processedText = processedText.Replace("{Interact}", GetTextForAction(InputAction.Interact));
        processedText = processedText.Replace("{QTE}", GetTextForAction(InputAction.QTE));
        processedText = processedText.Replace("{Parry}", GetTextForAction(InputAction.Parry));
        processedText = processedText.Replace("{Confirm}", GetTextForAction(InputAction.Confirm));
        processedText = processedText.Replace("{Cancel}", GetTextForAction(InputAction.Cancel));
        processedText = processedText.Replace("{Attack}", GetTextForAction(InputAction.Attack));
        processedText = processedText.Replace("{Skill}", GetTextForAction(InputAction.Skill));
        processedText = processedText.Replace("{EndTurn}", GetTextForAction(InputAction.EndTurn));

        return processedText;
    }

    public InputDeviceType GetCurrentDeviceType()
    {
        return currentDeviceType;
    }
}

