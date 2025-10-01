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
    
    private AttackAnimationData currentAttackData;
    private HashSet<int> triggeredQTEIndices = new HashSet<int>();
    private bool hitTriggered = false;
    
    private Action onAnimationComplete;
    private Action onHitFrame;
    private QTEManager qteManager;
    
    private bool isPlayingSequence = false;
    private string currentAnimationState;
    
    public void Initialize(Animator anim, QTEManager qteManager)
    {
        this.animator = anim;
        this.qteManager = qteManager;
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
        hitTriggered = false;
        isPlayingSequence = true;
        currentAnimationState = attackData.animationStateName;
        
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
}

