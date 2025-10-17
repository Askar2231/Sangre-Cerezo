using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;

/// <summary>
/// Manages Quick Time Events during combat
/// </summary>
public class QTEManager : MonoBehaviour
{
    [Header("QTE Settings")]
    [SerializeField] private float qteWindowDuration = 0.5f;
    [SerializeField] private InputActionReference qteInputAction;
    
    [Header("Timing")]
    [Tooltip("Time window before and after perfect timing")]
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
    
    // NOTE: Input checking removed - now handled by BattleInputManager
    // BattleInputManager will call ProcessQTEInput() when QTE button is pressed
    
    private void Update()
    {
        if (!isQTEActive) return;
        
        // Check if window expired (time-based expiration still handled here)
        if (Time.time >= qteStartTime + qteWindowDuration && !qteCompleted)
        {
            FailQTE();
        }
    }
    
    /// <summary>
    /// Process QTE input from BattleInputManager
    /// Called when player presses QTE button during QTE window
    /// </summary>
    public void ProcessQTEInput()
    {
        if (!isQTEActive)
        {
            Debug.LogWarning("[QTEManager] ProcessQTEInput called but QTE is not active!");
            return;
        }
        
        if (qteCompleted)
        {
            Debug.LogWarning("[QTEManager] QTE already completed!");
            return;
        }
        
        CheckQTETiming();
    }
    
    /// <summary>
    /// Start a QTE window
    /// </summary>
    public void StartQTEWindow()
    {
        if (isQTEActive)
        {
            Debug.LogWarning("QTE already active!");
            return;
        }
        
        isQTEActive = true;
        qteStartTime = Time.time;
        qteCompleted = false;
        
        OnQTEWindowStart?.Invoke(true);
        
        string actionName = qteInputAction != null ? qteInputAction.action.name : "QTE Button";
        Debug.Log($"QTE Window Started! Press {actionName}");
    }
    
    /// <summary>
    /// Check the timing of the QTE input
    /// </summary>
    private void CheckQTETiming()
    {
        float timeSinceStart = Time.time - qteStartTime;
        float perfectTiming = qteWindowDuration * 0.5f; // Middle of window is perfect
        
        float deviation = Mathf.Abs(timeSinceStart - perfectTiming);
        
        if (deviation <= perfectTimingWindow)
        {
            // Perfect timing
            SuccessQTE(true);
        }
        else if (timeSinceStart <= qteWindowDuration)
        {
            // Good timing, but not perfect
            SuccessQTE(false);
        }
        else
        {
            FailQTE();
        }
    }
    
    /// <summary>
    /// QTE was successful
    /// </summary>
    private void SuccessQTE(bool wasPerfect)
    {
        qteCompleted = true;
        isQTEActive = false;
        
        Debug.Log(wasPerfect ? "PERFECT QTE!" : "QTE Success!");
        OnQTESuccess?.Invoke();
        OnQTEWindowStart?.Invoke(false);
        OnQTEWindowEnd?.Invoke();
    }
    
    /// <summary>
    /// QTE failed
    /// </summary>
    private void FailQTE()
    {
        qteCompleted = true;
        isQTEActive = false;
        
        Debug.Log("QTE Failed!");
        OnQTEFail?.Invoke();
        OnQTEWindowStart?.Invoke(false);
        OnQTEWindowEnd?.Invoke();
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

