using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI que muestra el texto de lore de las estatuas
/// </summary>
public class StatueLoreUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject lorePanel;
    [SerializeField] private TextMeshProUGUI statueNameText;
    [SerializeField] private TextMeshProUGUI loreText;
    [SerializeField] private Image statueIcon;
    [SerializeField] private Button closeButton;
    
    [Header("Settings")]
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    
    private void Start()
    {
        // Ocultar panel al inicio
        if (lorePanel != null)
        {
            lorePanel.SetActive(false);
        }
        
        // Conectar botón de cerrar
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideLore);
        }
    }

    private void Update()
    {
        // Permitir cerrar con tecla
        if (lorePanel != null && lorePanel.activeSelf && Input.GetKeyDown(closeKey))
        {
            HideLore();
        }
    }

    /// <summary>
    /// Muestra el lore de una estatua
    /// </summary>
    public void ShowLore(StatueLoreData loreData)
    {
        if (loreData == null) return;
        
        // Actualizar textos
        if (statueNameText != null)
        {
            statueNameText.text = loreData.statueName;
        }
        
        if (loreText != null)
        {
            loreText.text = loreData.loreText;
        }
        
        // Actualizar icono si existe
        if (statueIcon != null && loreData.statueIcon != null)
        {
            statueIcon.sprite = loreData.statueIcon;
            statueIcon.gameObject.SetActive(true);
        }
        else if (statueIcon != null)
        {
            statueIcon.gameObject.SetActive(false);
        }
        
        // Mostrar panel
        if (lorePanel != null)
        {
            lorePanel.SetActive(true);
        }
        
        // Pausar el juego (opcional)
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Oculta el panel de lore
    /// </summary>
    public void HideLore()
    {
        if (lorePanel != null)
        {
            lorePanel.SetActive(false);
        }
        
        // Reanudar el juego (opcional)
        // Time.timeScale = 1f;
    }
}