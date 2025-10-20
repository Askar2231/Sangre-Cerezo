using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages animation playback using normalized time tracking (Option B)
/// Triggers events at specific normalized times without relying on animation events
/// </summary>
public class AnimationSequencer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    // Note: QTE warning time is now configured in QTEManager.cs
    // AnimationSequencer queries it from QTEManager.WarningTime
    
    private AttackAnimationData currentAttackData;
    private HashSet<int> triggeredQTEIndices = new HashSet<int>();
    private HashSet<int> triggeredWarningIndices = new HashSet<int>();
    private bool hitTriggered = false;
    
    private Action onAnimationComplete;
    private Action onHitFrame;
    private QTEManager qteManager;
    private QTEVisualFeedback qteVisualFeedback;
    
    private bool isPlayingSequence = false;
    private string currentAnimationState;
    private float animationLength; // Store animation length for timing calculations
    
    public void Initialize(Animator anim, QTEManager qteManager, QTEVisualFeedback visualFeedback = null)
    {
        this.animator = anim;
        this.qteManager = qteManager;
        this.qteVisualFeedback = visualFeedback;
    }
    
    /// <summary>
    /// Play an animation sequence with QTE timings
    /// </summary>
    public void PlayAnimationSequence(AttackAnimationData attackData, 
                                       Action onComplete, 
                                       Action onHit)
    {
        if (isPlayingSequence)
        {
            Debug.LogWarning("Animation sequence already playing!");
            return;
        }
        
        currentAttackData = attackData;
        onAnimationComplete = onComplete;
        onHitFrame = onHit;
        
        // Reset tracking
        triggeredQTEIndices.Clear();
        triggeredWarningIndices.Clear();
        hitTriggered = false;
        isPlayingSequence = true;
        currentAnimationState = attackData.animationStateName;
        
        // Get animation length
        animationLength = GetAnimationLength(attackData.animationStateName);
        
        // Play the animation on layer 0
        animator.Play(attackData.animationStateName, 0, 0f);
        
        // Start tracking
        StartCoroutine(TrackAnimationProgress());
    }
    
    /// <summary>
    /// Tracks animation progress using normalized time
    /// </summary>
    private IEnumerator TrackAnimationProgress()
    {
        // Wait one frame to ensure animation has started
        yield return null;
        
        AnimatorStateInfo stateInfo;
        float previousNormalizedTime = 0f;
        float timeoutTimer = 0f; // AGREGAR timer de timeout
        const float TIMEOUT_DURATION = 4f; // 4 segundos máximo
        
        while (isPlayingSequence)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            timeoutTimer += Time.deltaTime; // INCREMENTAR timer
            
            // FORZAR fin si pasan 4 segundos
            if (timeoutTimer >= TIMEOUT_DURATION)
            {
                Debug.LogWarning($"Animation sequence timed out after {TIMEOUT_DURATION} seconds! Force completing.");
                CompleteSequence();
                yield break;
            }
            
            // Verify we're in the correct animation state
            if (!stateInfo.IsName(currentAnimationState))
            {
                yield return null;
                continue;
            }
            
            // Get normalized time (0-1), handle looping
            float normalizedTime = stateInfo.normalizedTime % 1f;
            
            // Check for QTE timings
            CheckQTETriggers(normalizedTime, previousNormalizedTime);
            
            // Check for QTE warnings (pre-warnings before QTE starts)
            CheckQTEWarnings(normalizedTime, previousNormalizedTime);
            
            // Check for hit frame
            CheckHitTrigger(normalizedTime, previousNormalizedTime);
            
            // Check if animation completed (normalizedTime >= 1.0)
            if (stateInfo.normalizedTime >= 1f)
            {
                Debug.Log($"Animation completed normally after {timeoutTimer:F2} seconds");
                CompleteSequence();
                yield break;
            }
            
            previousNormalizedTime = normalizedTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// Check if any QTE windows should be triggered
    /// </summary>
    private void CheckQTETriggers(float currentTime, float previousTime)
    {
        if (currentAttackData.qteNormalizedTimings == null) return;
        
        for (int i = 0; i < currentAttackData.qteNormalizedTimings.Count; i++)
        {
            float qteTime = currentAttackData.qteNormalizedTimings[i];
            
            // Check if we've crossed this QTE timing
            if (!triggeredQTEIndices.Contains(i) && 
                currentTime >= qteTime && 
                previousTime < qteTime)
            {
                TriggerQTE(i);
                triggeredQTEIndices.Add(i);
            }
        }
    }
    
    /// <summary>
    /// Check if any QTE warnings should be triggered (before QTE starts)
    /// </summary>
    private void CheckQTEWarnings(float currentTime, float previousTime)
    {
        if (currentAttackData.qteNormalizedTimings == null || qteVisualFeedback == null) return;
        if (animationLength <= 0) return; // Need animation length for warning calculation
        if (qteManager == null) return; // Need QTEManager for warning time
        
        for (int i = 0; i < currentAttackData.qteNormalizedTimings.Count; i++)
        {
            float qteTime = currentAttackData.qteNormalizedTimings[i];
            
            // Calculate warning time in normalized time using QTEManager's warning time
            // warningTime (seconds) -> normalized time = warningTime / animationLength
            float warningOffset = qteManager.WarningTime / animationLength;
            float warningTime = Mathf.Max(0, qteTime - warningOffset);
            
            // Check if we've crossed the warning timing
            if (!triggeredWarningIndices.Contains(i) && 
                currentTime >= warningTime && 
                previousTime < warningTime)
            {
                qteVisualFeedback.ShowPreWarning();
                triggeredWarningIndices.Add(i);
                Debug.Log($"QTE Warning {i + 1} triggered at {currentTime:F2} (QTE at {qteTime:F2})");
            }
        }
    }
    
    /// <summary>
    /// Check if hit frame should be triggered
    /// </summary>
    private void CheckHitTrigger(float currentTime, float previousTime)
    {
        if (hitTriggered) return;
        
        float hitTime = currentAttackData.hitNormalizedTime;
        
        if (currentTime >= hitTime && previousTime < hitTime)
        {
            hitTriggered = true;
            onHitFrame?.Invoke();
        }
    }
    
    /// <summary>
    /// Trigger a QTE window
    /// </summary>
    private void TriggerQTE(int qteIndex)
    {
        Debug.Log($"QTE Window {qteIndex + 1} triggered!");
        qteManager?.StartQTEWindow();
    }
    
    /// <summary>
    /// Complete the animation sequence
    /// </summary>
    private void CompleteSequence()
    {
        isPlayingSequence = false;
        
        // Reset to default state - since you use a blend tree, just let the animator 
        // transition naturally back to the entry state via transitions
        // No need to force Play() which causes the layer index error
        if (animator != null)
        {
            // Optional: Set Speed to 0 to ensure blend tree shows idle
            animator.SetFloat("Speed", 0f);
            Debug.Log("Animation sequence completed, returning to blend tree");
        }
        
        onAnimationComplete?.Invoke();
        
        // Cleanup
        currentAttackData = null;
        onAnimationComplete = null;
        onHitFrame = null;
    }
    
    /// <summary>
    /// Force stop the current sequence
    /// </summary>
    public void StopSequence()
    {
        if (isPlayingSequence)
        {
            StopAllCoroutines();
            CompleteSequence();
        }
    }
    
    /// <summary>
    /// Get the length of an animation clip by state name
    /// </summary>
    private float GetAnimationLength(string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 0f;

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        Debug.Log(clips.Length + " animation clips found in controller.");
        
        // Strategy 1: Try to find clip by exact state name match
        foreach (AnimationClip clip in clips)
        {
            Debug.Log($"Checking clip '{clip.name}' against state '{stateName}'");
            if (clip.name == stateName)
            {
                Debug.Log($"Found animation clip '{clip.name}' with length {clip.length}s");
                return clip.length;
            }
        }
        
        // Strategy 2: Try to find clip with similar name (case insensitive, contains)
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.IndexOf(stateName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Debug.Log($"Found similar animation clip '{clip.name}' for state '{stateName}' with length {clip.length}s");
                return clip.length;
            }
        }
        
        Debug.LogWarning($"Could not find animation length for state: {stateName}");
        return 1f; // Default fallback
    }
}

