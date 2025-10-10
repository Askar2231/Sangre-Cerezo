using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Manages gamepad vibration with different patterns for different game events
/// Supports both Xbox and PlayStation controllers
/// </summary>
public class GamepadVibrationManager : MonoBehaviour
{
    #region Singleton
    public static GamepadVibrationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("Vibration Settings")]
    [SerializeField] private bool vibrationsEnabled = true;
    [SerializeField] [Range(0f, 1f)] private float globalIntensity = 1f;

    private Gamepad currentGamepad;
    private Coroutine currentVibrationCoroutine;

    private void Start()
    {
        // Get the current gamepad
        currentGamepad = Gamepad.current;
        
        if (currentGamepad == null)
        {
            Debug.LogWarning("No gamepad detected. Vibration will not work.");
        }
    }

    private void Update()
    {
        // Update gamepad reference if it changes
        if (Gamepad.current != currentGamepad)
        {
            currentGamepad = Gamepad.current;
        }
    }

    #region Public Vibration Methods

    /// <summary>
    /// Light attack vibration - quick and sharp
    /// </summary>
    public void VibrateOnLightAttack()
    {
        if (!vibrationsEnabled) return;
        StartVibrationPattern(new VibrationPattern
        {
            duration = 0.1f,
            lowFrequency = 0.3f,
            highFrequency = 0.8f
        });
    }

    /// <summary>
    /// Heavy attack vibration - stronger and longer
    /// </summary>
    public void VibrateOnHeavyAttack()
    {
        if (!vibrationsEnabled) return;
        StartVibrationPattern(new VibrationPattern
        {
            duration = 0.25f,
            lowFrequency = 0.6f,
            highFrequency = 1f
        });
    }

    /// <summary>
    /// Parry vibration - double pulse pattern
    /// </summary>
    public void VibrateOnParry()
    {
        if (!vibrationsEnabled) return;
        StartCoroutine(ParryVibrationPattern());
    }

    /// <summary>
    /// Counterattack vibration - intense burst
    /// </summary>
    public void VibrateOnCounterattack()
    {
        if (!vibrationsEnabled) return;
        StartCoroutine(CounterattackVibrationPattern());
    }

    /// <summary>
    /// Take damage vibration - medium intensity
    /// </summary>
    public void VibrateOnTakeDamage(float damagePercent = 0.5f)
    {
        if (!vibrationsEnabled) return;
        
        // Scale vibration based on damage amount
        float intensity = Mathf.Clamp01(damagePercent);
        
        StartVibrationPattern(new VibrationPattern
        {
            duration = 0.2f,
            lowFrequency = 0.5f * intensity,
            highFrequency = 0.7f * intensity
        });
    }

    /// <summary>
    /// Critical hit vibration - very strong
    /// </summary>
    public void VibrateOnCriticalHit()
    {
        if (!vibrationsEnabled) return;
        StartCoroutine(CriticalHitVibrationPattern());
    }

    /// <summary>
    /// Item use vibration - soft and quick
    /// </summary>
    public void VibrateOnItemUse()
    {
        if (!vibrationsEnabled) return;
        StartVibrationPattern(new VibrationPattern
        {
            duration = 0.15f,
            lowFrequency = 0.2f,
            highFrequency = 0.4f
        });
    }

    /// <summary>
    /// Healing vibration - gentle pulse
    /// </summary>
    public void VibrateOnHeal()
    {
        if (!vibrationsEnabled) return;
        StartCoroutine(HealVibrationPattern());
    }

    /// <summary>
    /// Block vibration - quick response
    /// </summary>
    public void VibrateOnBlock()
    {
        if (!vibrationsEnabled) return;
        StartVibrationPattern(new VibrationPattern
        {
            duration = 0.08f,
            lowFrequency = 0.4f,
            highFrequency = 0.5f
        });
    }

    /// <summary>
    /// Death vibration - long decay
    /// </summary>
    public void VibrateOnDeath()
    {
        if (!vibrationsEnabled) return;
        StartCoroutine(DeathVibrationPattern());
    }

    /// <summary>
    /// Custom vibration with specific parameters
    /// </summary>
    public void VibrateCustom(float duration, float lowFreq, float highFreq)
    {
        if (!vibrationsEnabled) return;
        StartVibrationPattern(new VibrationPattern
        {
            duration = duration,
            lowFrequency = lowFreq,
            highFrequency = highFreq
        });
    }

    #endregion

    #region Vibration Patterns (Coroutines)

    private IEnumerator ParryVibrationPattern()
    {
        // Double pulse for parry
        Vibrate(0.7f, 0.9f);
        yield return new WaitForSeconds(0.08f);
        StopVibration();
        
        yield return new WaitForSeconds(0.05f);
        
        Vibrate(0.7f, 0.9f);
        yield return new WaitForSeconds(0.08f);
        StopVibration();
    }

    private IEnumerator CounterattackVibrationPattern()
    {
        // Intense burst with fadeout
        float duration = 0.35f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float intensity = Mathf.Lerp(1f, 0.2f, t);
            
            Vibrate(0.8f * intensity, 1f * intensity);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        StopVibration();
    }

    private IEnumerator CriticalHitVibrationPattern()
    {
        // Strong initial hit with echo
        Vibrate(1f, 1f);
        yield return new WaitForSeconds(0.1f);
        StopVibration();
        
        yield return new WaitForSeconds(0.08f);
        
        Vibrate(0.5f, 0.6f);
        yield return new WaitForSeconds(0.12f);
        StopVibration();
    }

    private IEnumerator HealVibrationPattern()
    {
        // Gentle pulsing
        for (int i = 0; i < 3; i++)
        {
            Vibrate(0.2f, 0.3f);
            yield return new WaitForSeconds(0.08f);
            StopVibration();
            yield return new WaitForSeconds(0.08f);
        }
    }

    private IEnumerator DeathVibrationPattern()
    {
        // Long decay from strong to weak
        float duration = 0.8f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float intensity = Mathf.Lerp(1f, 0f, t);
            
            Vibrate(0.6f * intensity, 0.8f * intensity);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        StopVibration();
    }

    #endregion

    #region Core Vibration Functions

    private void StartVibrationPattern(VibrationPattern pattern)
    {
        // Stop any current vibration
        if (currentVibrationCoroutine != null)
        {
            StopCoroutine(currentVibrationCoroutine);
        }

        currentVibrationCoroutine = StartCoroutine(ExecuteVibrationPattern(pattern));
    }

    private IEnumerator ExecuteVibrationPattern(VibrationPattern pattern)
    {
        Vibrate(pattern.lowFrequency, pattern.highFrequency);
        yield return new WaitForSeconds(pattern.duration);
        StopVibration();
        currentVibrationCoroutine = null;
    }

    private void Vibrate(float lowFrequency, float highFrequency)
    {
        if (currentGamepad == null || !vibrationsEnabled) return;

        // Apply global intensity multiplier
        lowFrequency *= globalIntensity;
        highFrequency *= globalIntensity;

        // Clamp values
        lowFrequency = Mathf.Clamp01(lowFrequency);
        highFrequency = Mathf.Clamp01(highFrequency);

        // Set motor speeds
        currentGamepad.SetMotorSpeeds(lowFrequency, highFrequency);
    }

    private void StopVibration()
    {
        if (currentGamepad == null) return;
        currentGamepad.SetMotorSpeeds(0f, 0f);
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Enable or disable all vibrations
    /// </summary>
    public void SetVibrationsEnabled(bool enabled)
    {
        vibrationsEnabled = enabled;
        if (!enabled)
        {
            StopVibration();
        }
    }

    /// <summary>
    /// Set global vibration intensity (0-1)
    /// </summary>
    public void SetGlobalIntensity(float intensity)
    {
        globalIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>
    /// Immediately stop any ongoing vibration
    /// </summary>
    public void ForceStopVibration()
    {
        if (currentVibrationCoroutine != null)
        {
            StopCoroutine(currentVibrationCoroutine);
            currentVibrationCoroutine = null;
        }
        StopVibration();
    }

    #endregion

    private void OnDestroy()
    {
        // Clean up - stop vibration when destroyed
        StopVibration();
    }

    private void OnApplicationQuit()
    {
        // Clean up - stop vibration when quitting
        StopVibration();
    }

    #region Helper Struct
    private struct VibrationPattern
    {
        public float duration;
        public float lowFrequency;  // Left motor (low rumble)
        public float highFrequency; // Right motor (high rumble)
    }
    #endregion
}