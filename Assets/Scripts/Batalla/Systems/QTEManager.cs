using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

/// <summary>
/// Manages Quick Time Events during combat
/// 
/// QTE TUNING GUIDE:
/// - qteWindowDuration: Controls how long the QTE window stays open
///   • Easy: 0.7s - 1.0s (generous timing for all players)
///   • Normal: 0.4s - 0.6s (skill-based timing)
///   • Hard: 0.2s - 0.35s (requires quick reflexes)
///   • Expert: 0.15s - 0.2s (very tight timing)
///
/// - qteWarningTime: How early to show "Get Ready!" warning
///   • Short: 0.2s (minimal warning, harder)
///   • Normal: 0.3s - 0.4s (standard warning time)
///   • Long: 0.5s - 0.7s (more preparation time)
///
/// - perfectTimingWindow: Tolerance for perfect QTE bonus
///   • Easy: 0.15s - 0.2s (wide perfect window)
///   • Normal: 0.1s (centered in QTE window)
///   • Hard: 0.05s - 0.08s (precise timing)
///   • Expert: 0.03s - 0.05s (near frame-perfect)
///
/// DESIGN NOTE: QTE windows should be placed at impactful moments in animations
/// Configure timing points in AttackAnimationData.asset (qteNormalizedTimings)
/// </summary>
public class QTEManager : MonoBehaviour
{
    [Header("QTE Settings")]
    [Tooltip("Duration of QTE window in seconds - INCREASE this to make QTEs easier")]
    [SerializeField] private float qteWindowDuration = 0.5f;
    
    [Tooltip("Warning time before QTE window opens (seconds) - Shows 'Get Ready!' prompt")]
    [SerializeField] private float qteWarningTime = 0.3f;
    
    [Header("Timing")]
    [Tooltip("Perfect timing tolerance - INCREASE to make perfect QTE easier")]
    [SerializeField] private float perfectTimingWindow = 0.1f;
    
    public event Action OnQTESuccess;
    public event Action OnQTEFail;
    public event Action<bool> OnQTEWindowStart; // bool = is active
    public event Action OnQTEWindowEnd; // New event for when window closes
    
    private bool isQTEActive = false;
    private float qteStartTime;
    private bool qteCompleted = false;
    
    public bool IsQTEActive => isQTEActive;
    public float CurrentQTEDuration => qteWindowDuration; // Expose duration for UI
    public float WarningTime => qteWarningTime; // Expose warning time for AnimationSequencer
    
    // NOTE: Input checking removed - now handled by BattleInputManager
    // BattleInputManager will call ProcessQTEInput() when QTE button is pressed
    
    private void Update()
    {
        if (!isQTEActive) return;
        
        // Check if window duration has fully expired
        if (Time.time >= qteStartTime + qteWindowDuration)
        {
            if (!qteCompleted)
            {
                // Player didn't press button - fail the QTE
                FailQTE();
            }
            else
            {
                // Player pressed button earlier - now close the window after full duration
                isQTEActive = false;
                
                Debug.Log($"<color=cyan>[QTEManager]</color> ⏱️ Full window duration expired, closing window now");
                
                OnQTEWindowStart?.Invoke(false);
                Debug.Log($"<color=cyan>[QTEManager]</color> 📢 OnQTEWindowStart(false) event fired!");
                
                OnQTEWindowEnd?.Invoke();
                Debug.Log($"<color=cyan>[QTEManager]</color> 📢 OnQTEWindowEnd event fired!");
            }
        }
    }
    
    /// <summary>
    /// Process QTE input from BattleInputManager
    /// Called when player presses QTE button during QTE window
    /// </summary>
    public void ProcessQTEInput()
    {
        Debug.Log($"<color=cyan>[QTEManager]</color> ⚡ ProcessQTEInput() CALLED! | isQTEActive: {isQTEActive}, qteCompleted: {qteCompleted}");
        
        if (!isQTEActive)
        {
            Debug.LogWarning("<color=red>[QTEManager]</color> ❌ ProcessQTEInput called but QTE is NOT ACTIVE!");
            return;
        }
        
        if (qteCompleted)
        {
            Debug.LogWarning("<color=yellow>[QTEManager]</color> ⚠️ QTE already completed!");
            return;
        }
        
        Debug.Log($"<color=lime>[QTEManager]</color> ✅ QTE input validated, checking timing...");
        CheckQTETiming();
    }
    
    /// <summary>
    /// Start a QTE window
    /// </summary>
    public void StartQTEWindow()
    {
        Debug.Log($"<color=cyan>[QTEManager]</color> 🎯 StartQTEWindow() CALLED!");
        
        if (isQTEActive)
        {
            Debug.LogWarning("<color=yellow>[QTEManager]</color> ⚠️ QTE already active!");
            return;
        }
        
        isQTEActive = true;
        qteStartTime = Time.time;
        qteCompleted = false;
        
        Debug.Log($"<color=lime>[QTEManager]</color> ⚡ QTE WINDOW OPENED! Duration: {qteWindowDuration}s, StartTime: {qteStartTime}");
        
        OnQTEWindowStart?.Invoke(true);
        Debug.Log($"<color=lime>[QTEManager]</color> 📢 OnQTEWindowStart(true) event fired!");
        
        Debug.Log($"<color=yellow>⚡⚡⚡ QTE WINDOW ACTIVE - PRESS QTE BUTTON NOW! ⚡⚡⚡</color>");
    }
    
    /// <summary>
    /// Check the timing of the QTE input
    /// </summary>
    private void CheckQTETiming()
    {
        float timeSinceStart = Time.time - qteStartTime;
        float perfectTiming = qteWindowDuration * 0.5f; // Middle of window is perfect
        
        float deviation = Mathf.Abs(timeSinceStart - perfectTiming);
        
        Debug.Log($"<color=cyan>[QTEManager]</color> 🎯 CheckQTETiming() | timeSinceStart: {timeSinceStart:F3}s, perfectTiming: {perfectTiming:F3}s, deviation: {deviation:F3}s, windowDuration: {qteWindowDuration}s");
        
        if (deviation <= perfectTimingWindow)
        {
            // Perfect timing
            Debug.Log($"<color=lime>[QTEManager]</color> 🌟 PERFECT TIMING! deviation {deviation:F3}s <= {perfectTimingWindow}s");
            SuccessQTE(true);
        }
        else if (timeSinceStart <= qteWindowDuration)
        {
            // Good timing, but not perfect
            Debug.Log($"<color=lime>[QTEManager]</color> ✅ GOOD TIMING! timeSinceStart {timeSinceStart:F3}s <= {qteWindowDuration}s");
            SuccessQTE(false);
        }
        else
        {
            Debug.Log($"<color=red>[QTEManager]</color> ❌ TOO LATE! timeSinceStart {timeSinceStart:F3}s > {qteWindowDuration}s");
            FailQTE();
        }
    }
    
    /// <summary>
    /// QTE was successful
    /// </summary>
    private void SuccessQTE(bool wasPerfect)
    {
        qteCompleted = true;
        // NOTE: Keep isQTEActive = true until full window duration expires
        // This keeps the visual indicator visible for the full window duration
        
        Debug.Log($"<color=lime>[QTEManager]</color> ✨ {(wasPerfect ? "PERFECT QTE!" : "QTE Success!")}");
        
        OnQTESuccess?.Invoke();
        Debug.Log($"<color=lime>[QTEManager]</color> 📢 OnQTESuccess event fired!");
        
        // NOTE: Do NOT fire window end events here
        // Let Update() handle closing the window after full duration
        Debug.Log($"<color=yellow>[QTEManager]</color> ⏱️ QTE successful but keeping window open until full duration expires");
    }
    
    /// <summary>
    /// QTE failed (window expired without input)
    /// </summary>
    private void FailQTE()
    {
        qteCompleted = true;
        isQTEActive = false;
        
        Debug.Log($"<color=red>[QTEManager]</color> ❌ QTE Failed! Window expired.");
        
        OnQTEFail?.Invoke();
        Debug.Log($"<color=red>[QTEManager]</color> 📢 OnQTEFail event fired!");
        
        OnQTEWindowStart?.Invoke(false);
        Debug.Log($"<color=cyan>[QTEManager]</color> 📢 OnQTEWindowStart(false) event fired!");
        
        OnQTEWindowEnd?.Invoke();
        Debug.Log($"<color=cyan>[QTEManager]</color> 📢 OnQTEWindowEnd event fired!");
    }
    
    /// <summary>
    /// Force cancel current QTE
    /// </summary>
    public void CancelQTE()
    {
        if (isQTEActive)
        {
            isQTEActive = false;
            qteCompleted = true;
            OnQTEWindowStart?.Invoke(false);
            OnQTEWindowEnd?.Invoke();
        }
    }
}

