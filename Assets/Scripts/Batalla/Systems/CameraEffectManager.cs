using UnityEngine;
using System.Collections;
using DG.Tweening;

/// <summary>
/// Manages all camera effects and combat sounds during combat (shake, zoom, slow motion, audio feedback, etc.)
/// Subscribes to combat events and triggers appropriate visual and audio feedback
/// </summary>
public class CameraEffectManager : MonoBehaviour
{
    [Header("Audio Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swordClashSound; // Para parrear - choque de espadas
    [SerializeField] private AudioClip swordCutSound;   // Para golpear/ser golpeado - corte
    [SerializeField] private float audioVolume = 1f;
    [SerializeField] private bool enableAudioEffects = true;
    
    [Header("Parry Effects")]
    [SerializeField] private float parryShakeIntensity = 0.8f;
    [SerializeField] private float parryShakeDuration = 0.3f;
    [SerializeField] private float perfectParryShakeIntensity = 1.2f;
    [SerializeField] private float perfectParryShakeDuration = 0.4f;
    
    [Header("Hit Effects")]
    [SerializeField] private float playerHitShakeIntensity = 0.5f;
    [SerializeField] private float playerHitShakeDuration = 0.2f;
    [SerializeField] private float enemyHitShakeIntensity = 0.4f;
    [SerializeField] private float enemyHitShakeDuration = 0.15f;
    
    [Header("QTE Effects")]
    [SerializeField] private float qteSuccessShakeIntensity = 0.6f;
    [SerializeField] private float qteSuccessShakeDuration = 0.25f;
    [SerializeField] private float qteFailShakeIntensity = 0.3f;
    [SerializeField] private float qteFailShakeDuration = 0.2f;
    
    [Header("Attack Effects")]
    [SerializeField] private float lightAttackShakeIntensity = 0.3f;
    [SerializeField] private float lightAttackShakeDuration = 0.1f;
    [SerializeField] private float heavyAttackShakeIntensity = 0.7f;
    [SerializeField] private float heavyAttackShakeDuration = 0.3f;
    
    [Header("Slow Motion Effects")]
    [SerializeField] private bool enableSlowMotionOnPerfectParry = true;
    [SerializeField] private float slowMotionScale = 0.3f;
    [SerializeField] private float slowMotionDuration = 0.5f;
    
    [Header("Camera References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ThirdPersonJRPGCamera battleCamera;
    
    // Internal state
    private Vector3 originalCameraPosition;
    private bool isShaking = false;
    private Coroutine currentShakeCoroutine;
    
    private void Awake()
    {
        // Auto-find camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (battleCamera == null)
            battleCamera = FindObjectOfType<ThirdPersonJRPGCamera>();
        
        if (mainCamera != null)
            originalCameraPosition = mainCamera.transform.localPosition;
        
        // Auto-find or create AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Configure AudioSource for combat effects
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.volume = audioVolume;
            Debug.Log("<color=cyan>[CameraEffects]</color> 🔊 AudioSource configured for combat effects");
        }
    }
    
    private void OnEnable()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> 📹 Subscribing to combat events...");
        
        // Subscribe to ParrySystem events (NECESITA INSTANCIA)
        var parrySystem = FindObjectOfType<ParrySystem>();
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccessWithTiming += OnParrySuccess;
            parrySystem.OnParryFail += OnParryFail;
            Debug.Log("<color=lime>[CameraEffects]</color> ✅ ParrySystem events subscribed");
        }
        
        // Subscribe to BattleManagerV2 events 
        var battleManager = FindObjectOfType<BattleManagerV2>();
        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged += OnBattleStateChanged;
            Debug.Log("<color=lime>[CameraEffects]</color> ✅ BattleManager events subscribed");
        }
        
        // Subscribe to QTEManager events (NECESITA INSTANCIA)
        var qteManager = FindObjectOfType<QTEManager>();
        if (qteManager != null)
        {
            qteManager.OnQTESuccess += OnQTESuccess;
            qteManager.OnQTEFail += OnQTEFail;
            Debug.Log("<color=lime>[CameraEffects]</color> ✅ QTEManager events subscribed");
        }
        
        // Subscribe to character damage events
        SubscribeToCharacterEvents();
    }
    
    private void OnDisable()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> 📹 Unsubscribing from combat events...");
        
        // Unsubscribe from ParrySystem events (NECESITA INSTANCIA)
        var parrySystem = FindObjectOfType<ParrySystem>();
        if (parrySystem != null)
        {
            parrySystem.OnParrySuccessWithTiming -= OnParrySuccess;
            parrySystem.OnParryFail -= OnParryFail;
        }
        
        // Unsubscribe from BattleManagerV2 events
        var battleManager = FindObjectOfType<BattleManagerV2>();
        if (battleManager != null)
        {
            battleManager.OnBattleStateChanged -= OnBattleStateChanged;
        }
        
        // Unsubscribe from QTEManager events (NECESITA INSTANCIA)
        var qteManager = FindObjectOfType<QTEManager>();
        if (qteManager != null)
        {
            qteManager.OnQTESuccess -= OnQTESuccess;
            qteManager.OnQTEFail -= OnQTEFail;
        }
        
        // Unsubscribe from character events
        UnsubscribeFromCharacterEvents();
        
        // Stop any ongoing effects
        StopAllEffects();
    }
    
    private void SubscribeToCharacterEvents()
    {
        // Find and subscribe to player/enemy damage events
        var playerController = FindObjectOfType<PlayerBattleController>();
        var enemyController = FindObjectOfType<EnemyBattleController>();
        
        if (playerController?.Character != null)
        {
            playerController.Character.OnDamageTaken += OnPlayerDamageTaken;
            playerController.OnActionComplete += OnPlayerActionComplete;
            Debug.Log("<color=lime>[CameraEffects]</color> ✅ Player events subscribed");
        }
        
        if (enemyController?.Character != null)
        {
            enemyController.Character.OnDamageTaken += OnEnemyDamageTaken;
            Debug.Log("<color=lime>[CameraEffects]</color> ✅ Enemy events subscribed");
        }
    }
    
    private void UnsubscribeFromCharacterEvents()
    {
        var playerController = FindObjectOfType<PlayerBattleController>();
        var enemyController = FindObjectOfType<EnemyBattleController>();
        
        if (playerController?.Character != null)
        {
            playerController.Character.OnDamageTaken -= OnPlayerDamageTaken;
            playerController.OnActionComplete -= OnPlayerActionComplete;
        }
        
        if (enemyController?.Character != null)
        {
            enemyController.Character.OnDamageTaken -= OnEnemyDamageTaken;
        }
    }
    
    #region Event Handlers
    
    private void OnParrySuccess(bool wasPerfect)
    {
        Debug.Log($"<color=yellow>[CameraEffects]</color> 🛡️ Parry success! Perfect: {wasPerfect}");
        
        // Play sword clash sound for parry
        PlayAudioEffect(swordClashSound);
        
        if (wasPerfect)
        {
            TriggerPerfectParryEffect();
        }
        else
        {
            TriggerParryEffect();
        }
    }
    
    private void OnParryFail()
    {
        Debug.Log("<color=red>[CameraEffects]</color> ❌ Parry failed!");
        // No special effect for parry fail, damage effect will handle it
    }
    
    private void OnQTESuccess()
    {
        Debug.Log("<color=lime>[CameraEffects]</color> ⚡ QTE Success!");
        // Play cut sound for successful QTE
        PlayAudioEffect(swordCutSound);
        CameraShake(qteSuccessShakeIntensity, qteSuccessShakeDuration);
    }
    
    private void OnQTEFail()
    {
        Debug.Log("<color=red>[CameraEffects]</color> ❌ QTE Failed!");
        CameraShake(qteFailShakeIntensity, qteFailShakeDuration);
    }
    
    private void OnPlayerDamageTaken(float damage)
    {
        Debug.Log($"<color=red>[CameraEffects]</color> 💥 Player took {damage} damage!");
        // Play cut sound when player is hit
        PlayAudioEffect(swordCutSound);
        CameraShake(playerHitShakeIntensity, playerHitShakeDuration);
    }
    
    private void OnEnemyDamageTaken(float damage)
    {
        Debug.Log($"<color=orange>[CameraEffects]</color> 💥 Enemy took {damage} damage!");
        // Play cut sound when enemy is hit
        PlayAudioEffect(swordCutSound);
        CameraShake(enemyHitShakeIntensity, enemyHitShakeDuration);
    }
    
    private void OnPlayerActionComplete()
    {
        // Could add effects when player completes actions
        // For now, individual attack effects are handled elsewhere
    }
    
    private void OnBattleStateChanged(BattleState newState)
    {
        switch (newState)
        {
            case BattleState.BattleEnd:
                StopAllEffects();
                break;
        }
    }
    
    #endregion
    
    #region Effect Methods
    
    private void TriggerParryEffect()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> 🛡️ Triggering parry effect");
        CameraShake(parryShakeIntensity, parryShakeDuration);
        // Audio already played in OnParrySuccess
    }
    
    private void TriggerPerfectParryEffect()
    {
        Debug.Log("<color=yellow>[CameraEffects]</color> ⭐ Triggering PERFECT parry effect");
        CameraShake(perfectParryShakeIntensity, perfectParryShakeDuration);
        // Audio already played in OnParrySuccess
        
        if (enableSlowMotionOnPerfectParry)
        {
            TriggerSlowMotion();
        }
    }
    
    public void TriggerLightAttackEffect()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> ⚔️ Light attack effect");
        // Play cut sound for light attack
        PlayAudioEffect(swordCutSound);
        CameraShake(lightAttackShakeIntensity, lightAttackShakeDuration);
    }
    
    public void TriggerHeavyAttackEffect()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> ⚔️ Heavy attack effect");
        // Play cut sound for heavy attack
        PlayAudioEffect(swordCutSound);
        CameraShake(heavyAttackShakeIntensity, heavyAttackShakeDuration);
    }
    
    /// <summary>
    /// Trigger counter-attack effect (called from BattleManager)
    /// </summary>
    public void TriggerCounterAttackEffect()
    {
        Debug.Log("<color=yellow>[CameraEffects]</color> ⚡ Counter-attack effect (parry retaliation)");
        
        // Play cut sound for counter-attack
        PlayAudioEffect(swordCutSound);
        
        // Use heavy attack shake for more impact
        CameraShake(heavyAttackShakeIntensity, heavyAttackShakeDuration);
    }
    
    /// <summary>
    /// Play audio effect with proper volume and error handling
    /// </summary>
    private void PlayAudioEffect(AudioClip clip)
    {
        if (!enableAudioEffects)
        {
            Debug.Log("<color=gray>[CameraEffects]</color> 🔇 Audio effects disabled");
            return;
        }
        
        if (audioSource == null)
        {
            Debug.LogWarning("<color=yellow>[CameraEffects]</color> ⚠️ AudioSource not found!");
            return;
        }
        
        if (clip == null)
        {
            Debug.LogWarning("<color=yellow>[CameraEffects]</color> ⚠️ Audio clip is null!");
            return;
        }
        
        Debug.Log($"<color=green>[CameraEffects]</color> 🔊 Playing audio: {clip.name}");
        audioSource.PlayOneShot(clip, audioVolume);
    }
    
    private void CameraShake(float intensity, float duration)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("<color=yellow>[CameraEffects]</color> ⚠️ Main camera not found!");
            return;
        }
        
        // Stop any existing shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }
        
        currentShakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
    }
    
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0f;
        
        Debug.Log($"<color=cyan>[CameraEffects]</color> 📳 Camera shake: intensity={intensity}, duration={duration}s");
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            mainCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Return to original position
        mainCamera.transform.localPosition = originalPos;
        isShaking = false;
        currentShakeCoroutine = null;
        
        Debug.Log("<color=cyan>[CameraEffects]</color> ✅ Camera shake completed");
    }
    
    private void TriggerSlowMotion()
    {
        Debug.Log($"<color=magenta>[CameraEffects]</color> ⏰ Slow motion: scale={slowMotionScale}, duration={slowMotionDuration}s");
        
        // Use DOTween for smooth time scale transitions
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, slowMotionScale, 0.1f)
            .SetUpdate(true) // Use unscaled time
            .OnComplete(() =>
            {
                // Hold slow motion
                DOVirtual.DelayedCall(slowMotionDuration, () =>
                {
                    // Return to normal speed
                    DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 0.2f)
                        .SetUpdate(true);
                }, true);
            });
    }
    
    private void StopAllEffects()
    {
        Debug.Log("<color=cyan>[CameraEffects]</color> 🛑 Stopping all camera effects");
        
        // Stop shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }
        
        // Reset camera position
        if (mainCamera != null && !isShaking)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
        
        // Reset time scale
        Time.timeScale = 1f;
        
        isShaking = false;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually trigger camera shake (for external scripts)
    /// </summary>
    public void TriggerShake(float intensity, float duration)
    {
        CameraShake(intensity, duration);
    }
    
    /// <summary>
    /// Manually play sword clash sound (for external scripts)
    /// </summary>
    public void PlaySwordClashSound()
    {
        PlayAudioEffect(swordClashSound);
    }
    
    /// <summary>
    /// Manually play sword cut sound (for external scripts)
    /// </summary>
    public void PlaySwordCutSound()
    {
        PlayAudioEffect(swordCutSound);
    }
    
    /// <summary>
    /// Set audio volume for all combat effects
    /// </summary>
    public void SetAudioVolume(float volume)
    {
        audioVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = audioVolume;
        }
    }
    
    /// <summary>
    /// Toggle audio effects on/off
    /// </summary>
    public void SetAudioEnabled(bool enabled)
    {
        enableAudioEffects = enabled;
        Debug.Log($"<color=cyan>[CameraEffects]</color> 🔊 Audio effects {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Check if camera is currently shaking
    /// </summary>
    public bool IsShaking => isShaking;
    
    #endregion
    
    #region Debug/Testing
    
    [ContextMenu("Test Parry Effect")]
    private void TestParryEffect()
    {
        TriggerParryEffect();
    }
    
    [ContextMenu("Test Perfect Parry Effect")]
    private void TestPerfectParryEffect()
    {
        TriggerPerfectParryEffect();
    }
    
    [ContextMenu("Test Heavy Attack Effect")]
    private void TestHeavyAttackEffect()
    {
        TriggerHeavyAttackEffect();
    }
    
    [ContextMenu("Test Sword Clash Sound")]
    private void TestSwordClashSound()
    {
        PlaySwordClashSound();
    }
    
    [ContextMenu("Test Sword Cut Sound")]
    private void TestSwordCutSound()
    {
        PlaySwordCutSound();
    }
    
    #endregion
}