using UnityEngine;
using System;

/// <summary>
/// Manages parry mechanics during enemy attacks
/// </summary>
public class ParrySystem : MonoBehaviour
{
    [Header("Parry Settings")]
    [SerializeField] private KeyCode parryInputKey = KeyCode.Space;
    [SerializeField] private float parryWindowDuration = 0.3f;
    [SerializeField] private float staminaRewardOnSuccessfulParry = 30f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float perfectParryWindow = 0.1f;
    
    public event Action OnParrySuccess;
    public event Action OnParryFail;
    public event Action<bool> OnParryWindowActive; // bool = is active
    
    private bool isParryWindowActive = false;
    private float parryWindowStartTime;
    private bool parryAttempted = false;
    
    public bool IsParryWindowActive => isParryWindowActive;
    public float StaminaReward => staminaRewardOnSuccessfulParry;
    
    private void Update()
    {
        if (!isParryWindowActive) return;
        
        // Check for parry input
        if (Input.GetKeyDown(parryInputKey) && !parryAttempted)
        {
            CheckParryTiming();
        }
        
        // Close window after duration
        if (Time.time >= parryWindowStartTime + parryWindowDuration)
        {
            CloseParryWindow();
        }
    }
    
    /// <summary>
    /// Open parry window (called right before enemy attack lands)
    /// </summary>
    public void OpenParryWindow()
    {
        if (isParryWindowActive)
        {
            Debug.LogWarning("Parry window already active!");
            return;
        }
        
        isParryWindowActive = true;
        parryWindowStartTime = Time.time;
        parryAttempted = false;
        
        OnParryWindowActive?.Invoke(true);
        Debug.Log($"PARRY WINDOW! Press {parryInputKey}");
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
        
        // Any parry within the window is successful
        SuccessfulParry(isPerfect);
    }
    
    /// <summary>
    /// Parry was successful
    /// </summary>
    private void SuccessfulParry(bool wasPerfect)
    {
        isParryWindowActive = false;
        
        Debug.Log(wasPerfect ? "PERFECT PARRY!" : "Parry Success!");
        OnParrySuccess?.Invoke();
        OnParryWindowActive?.Invoke(false);
    }
    
    /// <summary>
    /// Close parry window (player didn't parry)
    /// </summary>
    private void CloseParryWindow()
    {
        if (!parryAttempted)
        {
            // Window expired without input
            Debug.Log("Parry window missed!");
            OnParryFail?.Invoke();
        }
        
        isParryWindowActive = false;
        OnParryWindowActive?.Invoke(false);
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

