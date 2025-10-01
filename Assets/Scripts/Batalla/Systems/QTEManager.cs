using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages Quick Time Events during combat
/// </summary>
public class QTEManager : MonoBehaviour
{
    [Header("QTE Settings")]
    [SerializeField] private float qteWindowDuration = 0.5f;
    [SerializeField] private KeyCode qteInputKey = KeyCode.Space;
    
    [Header("Timing")]
    [Tooltip("Time window before and after perfect timing")]
    [SerializeField] private float perfectTimingWindow = 0.1f;
    
    public event Action OnQTESuccess;
    public event Action OnQTEFail;
    public event Action<bool> OnQTEWindowStart; // bool = is active
    
    private bool isQTEActive = false;
    private float qteStartTime;
    private bool qteCompleted = false;
    
    public bool IsQTEActive => isQTEActive;
    
    private void Update()
    {
        if (!isQTEActive) return;
        
        // Check for input
        if (Input.GetKeyDown(qteInputKey) && !qteCompleted)
        {
            CheckQTETiming();
        }
        
        // Check if window expired
        if (Time.time >= qteStartTime + qteWindowDuration && !qteCompleted)
        {
            FailQTE();
        }
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
        Debug.Log($"QTE Window Started! Press {qteInputKey}");
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
        }
    }
}

