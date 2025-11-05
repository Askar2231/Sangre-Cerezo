using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Simple victory and defeat screen manager
/// Shows appropriate screen based on BattleManagerV2 battle result
/// </summary>
public class BattleResultManager : MonoBehaviour
{
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    
    [Header("Defeat Screen")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private TextMeshProUGUI defeatMessageText;
    
    [Header("Settings")]
    [SerializeField] private float screenDelay = 2f;
    [SerializeField] private float victoryAutoCloseDelay = 3f; // Tiempo antes de cerrar automáticamente la victoria
    
    private BattleManagerV2 battleManager;
    private bool isShowingDefeat = false;
    private bool canAcceptInput = false;
    
    private void Awake()
    {
        battleManager = FindObjectOfType<BattleManagerV2>();
        
        if (battleManager == null)
        {
            Debug.LogError("BattleManagerV2 not found! BattleResultManager will not work.");
            return;
        }
        
        HideAllPanels();
    }
    
    private void OnEnable()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleEnded += HandleBattleEnded;
            Debug.Log("<color=lime>[BattleResult]</color> ✅ Subscribed to battle end events");
        }
    }
    
    private void OnDisable()
    {
        if (battleManager != null)
        {
            battleManager.OnBattleEnded -= HandleBattleEnded;
        }
    }
    
    private void Update()
    {
        // Solo check input para pantalla de derrota
        if (canAcceptInput && isShowingDefeat)
        {
            if (AnyInputPressed())
            {
                HandleAnyKeyPressed();
            }
        }
    }
    
    /// <summary>
    /// Check if any key or gamepad button was pressed
    /// </summary>
    private bool AnyInputPressed()
    {
        // Check keyboard
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }
        
        // Check gamepad
        if (Gamepad.current != null)
        {
            // Check all gamepad buttons
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.leftShoulder.wasPressedThisFrame ||
                Gamepad.current.rightShoulder.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame)
            {
                return true;
            }
        }
        
        // Fallback: Check old input system for compatibility
        if (Input.anyKeyDown)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Handle any key pressed while showing defeat screen
    /// </summary>
    private void HandleAnyKeyPressed()
    {
        Debug.Log("<color=cyan>[BattleResult]</color> Any key pressed on defeat screen!");
        
        canAcceptInput = false;
        OnDefeatRetry();
    }
    
    private void HandleBattleEnded(BattleResult result)
    {
        Debug.Log($"<color=cyan>[BattleResult]</color> Battle ended: {result}");
        StartCoroutine(ShowBattleResultWithDelay(result, screenDelay));
    }
    
    private IEnumerator ShowBattleResultWithDelay(BattleResult result, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        switch (result)
        {
            case BattleResult.PlayerVictory:
                ShowVictoryScreen();
                break;
                
            case BattleResult.PlayerDefeated:
                ShowDefeatScreen();
                break;
        }
    }
    
    private void ShowVictoryScreen()
    {
        Debug.Log("<color=lime>[BattleResult]</color> 🎉 Showing VICTORY screen");
        
        HideAllPanels();
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            if (victoryMessageText != null)
                victoryMessageText.text = "¡Has derrotado al enemigo!";
        }
        
        // Iniciar auto-close de victoria
        StartCoroutine(AutoCloseVictoryScreen());
    }
    
    /// <summary>
    /// Auto-close victory screen after delay
    /// </summary>
    private IEnumerator AutoCloseVictoryScreen()
    {
        Debug.Log($"<color=lime>[BattleResult]</color> Victory will auto-close in {victoryAutoCloseDelay} seconds...");
        
        yield return new WaitForSeconds(victoryAutoCloseDelay);
        
        Debug.Log("<color=lime>[BattleResult]</color> Auto-closing victory screen");
        OnVictoryNext();
    }
    
    private void ShowDefeatScreen()
    {
        Debug.Log("<color=red>[BattleResult]</color> 💀 Showing DEFEAT screen");
        
        HideAllPanels();
        isShowingDefeat = true;
        
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            
            if (defeatMessageText != null)
                defeatMessageText.text = "Has sido derrotado...";
        }
        
        // Enable input after a short delay to prevent accidental skips
        StartCoroutine(EnableDefeatInput());
    }
    
    /// <summary>
    /// Enable input for defeat screen after delay
    /// </summary>
    private IEnumerator EnableDefeatInput()
    {
        yield return new WaitForSeconds(0.5f);
        canAcceptInput = true;
        Debug.Log("<color=red>[BattleResult]</color> Input now accepted on defeat screen - press any key to retry!");
    }
    
    private void HideAllPanels()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);
            
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
        
        isShowingDefeat = false;
        canAcceptInput = false;
    }
    
    private void OnVictoryNext()
    {
        Debug.Log("<color=lime>[BattleResult]</color> Victory - Continuing game");
        HideAllPanels();
        // Solo desaparecer la UI, continuar el juego normalmente
    }
    
    private void OnDefeatRetry()
    {
        Debug.Log("<color=orange>[BattleResult]</color> Defeat - Restarting scene");
        HideAllPanels();
        
        // Reiniciar la escena actual
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }
    
    // Testing
    [ContextMenu("Test Victory")]
    public void TestVictory() => ShowVictoryScreen();
    
    [ContextMenu("Test Defeat")]
    public void TestDefeat() => ShowDefeatScreen();
}
