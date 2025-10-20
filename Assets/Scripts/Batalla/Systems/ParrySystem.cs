using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Manages parry mechanics during enemy attacks
/// 
/// PARRY TUNING GUIDE:
/// - parryWindowDuration: Controls how long the parry window stays open
///   • Easy: 0.5s - 0.8s (generous timing)
///   • Normal: 0.3s - 0.4s (skill-based)
///   • Hard: 0.15s - 0.25s (requires prediction)
///   • Souls-like: 0.1s - 0.15s (frame-perfect)
///
/// - parryWindowOpenDelay: When to open parry window during enemy attack
///   • Earlier (0.2s - 0.3s): Opens sooner, easier to react
///   • Normal (0.4s - 0.5s): Balanced timing
///   • Later (0.6s - 0.8s): Opens closer to hit, requires prediction
///
/// - perfectParryWindow: Tolerance for perfect parry bonus
///   • Easy: 0.15s - 0.2s (wide perfect window)
///   • Normal: 0.1s (centered timing)
///   • Hard: 0.05s - 0.08s (precise timing required)
///
/// - staminaRewardOnSuccessfulParry: Stamina awarded on success
///   • Low: 15-20 (small bonus)
///   • Normal: 25-35 (meaningful reward)
///   • High: 40-50 (significant advantage)
/// </summary>
public class ParrySystem : MonoBehaviour
{
    [Header("Parry Settings")]
    [Tooltip("Duration of parry window in seconds - INCREASE this to make parrying easier")]
    [SerializeField] private float parryWindowDuration = 0.3f;
    
    [Tooltip("Delay before opening parry window during enemy attack (seconds)")]
    [SerializeField] private float parryWindowOpenDelay = 0.4f;
    
    [Tooltip("Stamina reward given to player on successful parry")]
    [SerializeField] private float staminaRewardOnSuccessfulParry = 30f;

    [Header("Visual Feedback")]
    [Tooltip("Perfect parry window for bonus timing - DECREASE to make perfect parry harder")]
    [SerializeField] private float perfectParryWindow = 0.1f;

    public event Action OnParrySuccess;
    public event Action<bool> OnParrySuccessWithTiming; // bool = wasPerfect
    public event Action OnParryFail;
    public event Action<bool> OnParryWindowActive; // bool = is active

    private bool isParryWindowActive = false;
    private float parryWindowStartTime;
    private bool parryAttempted = false;

    public bool IsParryWindowActive => isParryWindowActive;
    public float StaminaReward => staminaRewardOnSuccessfulParry;
    public float WindowOpenDelay => parryWindowOpenDelay; // Expose delay for EnemyBattleController

    // NOTE: Input checking removed - now handled by BattleInputManager
    // BattleInputManager will call ProcessParryInput() when parry button is pressed

    private void Update()
    {
        if (!isParryWindowActive) return;

        // Close window after duration (time-based expiration still handled here)
        if (Time.time >= parryWindowStartTime + parryWindowDuration)
        {
            CloseParryWindow();
        }
    }
    
    /// <summary>
    /// Process parry input from BattleInputManager
    /// Called when player presses parry button during parry window
    /// </summary>
    public void ProcessParryInput()
    {
        Debug.Log($"<color=cyan>[ParrySystem]</color> 🛡️ ProcessParryInput() CALLED! | isParryWindowActive: {isParryWindowActive}, parryAttempted: {parryAttempted}");
        
        if (!isParryWindowActive)
        {
            Debug.LogWarning("<color=red>[ParrySystem]</color> ❌ ProcessParryInput called but window is NOT ACTIVE!");
            return;
        }
        
        if (parryAttempted)
        {
            Debug.LogWarning("<color=yellow>[ParrySystem]</color> ⚠️ Parry already attempted in this window!");
            return;
        }
        
        Debug.Log($"<color=lime>[ParrySystem]</color> ✅ Parry input validated, checking timing...");
        CheckParryTiming();
    }

    /// <summary>
    /// Open parry window (called right before enemy attack lands)
    /// </summary>
    public void OpenParryWindow()
    {
        Debug.Log($"<color=cyan>[ParrySystem]</color> 🎯 OpenParryWindow() CALLED!");
        
        if (isParryWindowActive)
        {
            Debug.LogWarning("<color=yellow>[ParrySystem]</color> ⚠️ Parry window already active!");
            return;
        }

        isParryWindowActive = true;
        parryWindowStartTime = Time.time;
        parryAttempted = false;

        Debug.Log($"<color=lime>[ParrySystem]</color> 🛡️ PARRY WINDOW OPENED! Duration: {parryWindowDuration}s, StartTime: {parryWindowStartTime}");

        OnParryWindowActive?.Invoke(true);
        Debug.Log($"<color=lime>[ParrySystem]</color> 📢 OnParryWindowActive(true) event fired!");
        
        Debug.Log($"<color=yellow>⚔️⚔️⚔️ PARRY WINDOW ACTIVE - PRESS PARRY BUTTON NOW! ⚔️⚔️⚔️</color>");
    }

    /// <summary>
    /// Check the timing of the parry attempt
    /// </summary>
    private void CheckParryTiming()
    {
        parryAttempted = true;

        float timeSinceWindowStart = Time.time - parryWindowStartTime;
        float perfectTiming = parryWindowDuration * 0.5f;
        float deviation = Mathf.Abs(timeSinceWindowStart - perfectTiming);

        bool isPerfect = deviation <= perfectParryWindow;
        
        Debug.Log($"<color=cyan>[ParrySystem]</color> 🎯 CheckParryTiming() | timeSinceStart: {timeSinceWindowStart:F3}s, perfectTiming: {perfectTiming:F3}s, deviation: {deviation:F3}s, windowDuration: {parryWindowDuration}s");
        Debug.Log($"<color=cyan>[ParrySystem]</color> isPerfect: {isPerfect} (deviation {deviation:F3}s {(isPerfect ? "<=" : ">")} {perfectParryWindow}s)");

        // Any parry within the window is successful
        SuccessfulParry(isPerfect);
    }

    /// <summary>
    /// Parry was successful
    /// </summary>
    private void SuccessfulParry(bool wasPerfect)
    {
        isParryWindowActive = false;

        Debug.Log($"<color=lime>[ParrySystem]</color> ✨ {(wasPerfect ? "PERFECT PARRY!" : "Parry Success!")}");
        
        OnParrySuccess?.Invoke();
        Debug.Log($"<color=lime>[ParrySystem]</color> 📢 OnParrySuccess event fired!");
        
        OnParrySuccessWithTiming?.Invoke(wasPerfect); // Event with perfect timing info
        Debug.Log($"<color=lime>[ParrySystem]</color> 📢 OnParrySuccessWithTiming({wasPerfect}) event fired!");
        
        OnParryWindowActive?.Invoke(false);
        Debug.Log($"<color=lime>[ParrySystem]</color> 📢 OnParryWindowActive(false) event fired!");
    }

    /// <summary>
    /// Close parry window (player didn't parry)
    /// </summary>
    private void CloseParryWindow()
    {
        if (!parryAttempted)
        {
            // Window expired without input
            Debug.Log("<color=red>[ParrySystem]</color> ❌ Parry window missed!");
            OnParryFail?.Invoke();
            Debug.Log($"<color=red>[ParrySystem]</color> 📢 OnParryFail event fired!");
        }

        isParryWindowActive = false;
        OnParryWindowActive?.Invoke(false);
        Debug.Log($"<color=cyan>[ParrySystem]</color> 📢 OnParryWindowActive(false) event fired!");
    }

    /// <summary>
    /// Force cancel parry window
    /// </summary>
    public void CancelParryWindow()
    {
        if (isParryWindowActive)
        {
            isParryWindowActive = false;
            OnParryWindowActive?.Invoke(false);
        }
    }
}

