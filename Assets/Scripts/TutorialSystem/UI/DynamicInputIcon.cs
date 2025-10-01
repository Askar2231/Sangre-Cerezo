using UnityEngine;
using TMPro;

/// <summary>
/// Componente que actualiza automáticamente los iconos de input en TextMeshPro
/// cuando el jugador cambia de dispositivo de entrada
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class DynamicInputIcon : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private bool updateOnDeviceChange = true;
    
    [Header("Texto Original")]
    [Tooltip("El texto con placeholders como {Move}, {Run}, etc.")]
    [TextArea(3, 6)]
    [SerializeField] private string originalText;

    private TextMeshProUGUI textComponent;
    private string lastProcessedText;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        // Si no hay texto original asignado, usar el texto actual del componente
        if (string.IsNullOrEmpty(originalText))
        {
            originalText = textComponent.text;
        }
    }

    private void OnEnable()
    {
        // Suscribirse a cambios de dispositivo
        if (updateOnDeviceChange && InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged += HandleDeviceChanged;
        }

        // Procesar texto inicial
        UpdateText();
    }

    private void OnDisable()
    {
        // Desuscribirse de cambios de dispositivo
        if (InputIconMapper.Instance != null)
        {
            InputIconMapper.Instance.OnDeviceChanged -= HandleDeviceChanged;
        }
    }

    private void HandleDeviceChanged(InputDeviceType newDevice)
    {
        UpdateText();
    }

    /// <summary>
    /// Actualiza el texto procesando los placeholders
    /// </summary>
    public void UpdateText()
    {
        if (textComponent == null || InputIconMapper.Instance == null)
        {
            return;
        }

        string processedText = InputIconMapper.Instance.ProcessTextPlaceholders(originalText);

        // Solo actualizar si el texto cambió
        if (processedText != lastProcessedText)
        {
            textComponent.text = processedText;
            lastProcessedText = processedText;
        }
    }

    /// <summary>
    /// Establece un nuevo texto con placeholders
    /// </summary>
    public void SetText(string newText)
    {
        originalText = newText;
        UpdateText();
    }

    /// <summary>
    /// Obtiene el texto original con placeholders
    /// </summary>
    public string GetOriginalText()
    {
        return originalText;
    }

    /// <summary>
    /// Obtiene el texto procesado actual
    /// </summary>
    public string GetProcessedText()
    {
        return lastProcessedText;
    }

#if UNITY_EDITOR
    // Validación en el editor
    private void OnValidate()
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        // Si el texto original está vacío, usar el texto del componente
        if (string.IsNullOrEmpty(originalText) && textComponent != null)
        {
            originalText = textComponent.text;
        }
    }
#endif
}

