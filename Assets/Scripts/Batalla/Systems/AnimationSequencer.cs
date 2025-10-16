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
    
    [Header("QTE Warning Settings")]
    [SerializeField] private float qteWarningTime = 0.3f; // Seconds before QTE to show warning
    
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
        
        // Play the animation
        animator.Play(attackData.animationStateName);
        
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
        
        while (isPlayingSequence)
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
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
        
        for (int i = 0; i < currentAttackData.qteNormalizedTimings.Count; i++)
        {
            float qteTime = currentAttackData.qteNormalizedTimings[i];
            
            // Calculate warning time in normalized time
            // warningTime (seconds) -> normalized time = warningTime / animationLength
            float warningOffset = qteWarningTime / animationLength;
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
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == stateName)
            {
                return clip.length;
            }
        }
        
        Debug.LogWarning($"Could not find animation length for state: {stateName}");
        return 1f; // Default fallback
    }
}

