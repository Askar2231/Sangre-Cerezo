using UnityEngine;
using TMPro;
using System;

public class KarmaManager : MonoBehaviour
{
    public static KarmaManager Instance { get; private set; }
    
    [Header("Karma Settings")]
    private int currentKarma = 0;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI karmaText;
    [SerializeField] private GameObject karmaUI; // Optional: parent GameObject to show/hide karma display
    
    [Header("Display Settings")]
    [SerializeField] private bool showKarmaUI = true;
    [SerializeField] private string karmaPrefix = "Karma : ";
    [SerializeField] private Color positiveKarmaColor = Color.green;
    [SerializeField] private Color neutralKarmaColor = Color.white;
    [SerializeField] private Color negativeKarmaColor = Color.red;
    
    // Event for other systems to listen to karma changes
    public static event Action<int> OnKarmaChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        UpdateKarmaUI();
    }
    
    private void Start()
    {
        // Initial UI setup
        if (karmaUI != null)
        {
            karmaUI.SetActive(showKarmaUI);
        }
        
        UpdateKarmaUI();
    }

    public void AddKarma(int amount)
    {
        int previousKarma = currentKarma;
        currentKarma += amount;
        
        Debug.Log($"<color=cyan>[KarmaManager]</color> Karma {(amount > 0 ? "añadido" : "reducido")}: {amount}. " +
                  $"Karma anterior: {previousKarma} → Karma actual: {currentKarma}");
        
        UpdateKarmaUI();
        OnKarmaChanged?.Invoke(currentKarma);
    }

    public int GetCurrentKarma()
    {
        return currentKarma;
    }
    
    /// <summary>
    /// Updates the karma UI display
    /// </summary>
    private void UpdateKarmaUI()
    {
        if (karmaText == null) return;
        
        // Update text
        karmaText.text = $"{karmaPrefix}{currentKarma}";
        
        // Update color based on karma value
        if (currentKarma > 0)
        {
            karmaText.color = positiveKarmaColor;
        }
        else if (currentKarma < 0)
        {
            karmaText.color = negativeKarmaColor;
        }
        else
        {
            karmaText.color = neutralKarmaColor;
        }
    }
    
    /// <summary>
    /// Show or hide the karma UI
    /// </summary>
    public void SetKarmaUIVisible(bool visible)
    {
        showKarmaUI = visible;
        
        if (karmaUI != null)
        {
            karmaUI.SetActive(visible);
        }
    }
    
    /// <summary>
    /// Reset karma to zero (for testing or new game)
    /// </summary>
    public void ResetKarma()
    {
        currentKarma = 0;
        Debug.Log("<color=yellow>[KarmaManager]</color> Karma reseteado a 0");
        UpdateKarmaUI();
        OnKarmaChanged?.Invoke(currentKarma);
    }
}