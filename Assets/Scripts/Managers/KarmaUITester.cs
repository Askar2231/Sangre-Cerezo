using UnityEngine;

/// <summary>
/// Helper script for testing Karma UI in the editor
/// Attach to buttons or use ContextMenu to test karma changes
/// </summary>
public class KarmaUITester : MonoBehaviour
{
    [Header("Test Values")]
    [SerializeField] private int positiveKarmaAmount = 10;
    [SerializeField] private int negativeKarmaAmount = -10;
    
    [ContextMenu("Add Positive Karma (+10)")]
    public void TestAddPositiveKarma()
    {
        if (KarmaManager.Instance != null)
        {
            KarmaManager.Instance.AddKarma(positiveKarmaAmount);
            Debug.Log($"<color=green>TEST: Added {positiveKarmaAmount} karma. Current: {KarmaManager.Instance.GetCurrentKarma()}</color>");
        }
        else
        {
            Debug.LogError("KarmaManager.Instance is null! Make sure KarmaManager exists in the scene.");
        }
    }
    
    [ContextMenu("Add Negative Karma (-10)")]
    public void TestAddNegativeKarma()
    {
        if (KarmaManager.Instance != null)
        {
            KarmaManager.Instance.AddKarma(negativeKarmaAmount);
            Debug.Log($"<color=red>TEST: Added {negativeKarmaAmount} karma. Current: {KarmaManager.Instance.GetCurrentKarma()}</color>");
        }
        else
        {
            Debug.LogError("KarmaManager.Instance is null! Make sure KarmaManager exists in the scene.");
        }
    }
    
    [ContextMenu("Reset Karma to 0")]
    public void TestResetKarma()
    {
        if (KarmaManager.Instance != null)
        {
            KarmaManager.Instance.ResetKarma();
            Debug.Log($"<color=yellow>TEST: Karma reset to 0</color>");
        }
        else
        {
            Debug.LogError("KarmaManager.Instance is null!");
        }
    }
    
    [ContextMenu("Show Current Karma")]
    public void TestShowCurrentKarma()
    {
        if (KarmaManager.Instance != null)
        {
            int currentKarma = KarmaManager.Instance.GetCurrentKarma();
            Debug.Log($"<color=cyan>Current Karma: {currentKarma}</color>");
        }
        else
        {
            Debug.LogError("KarmaManager.Instance is null!");
        }
    }
    
    // Public methods that can be called from UI buttons
    public void AddPositiveKarma()
    {
        TestAddPositiveKarma();
    }
    
    public void AddNegativeKarma()
    {
        TestAddNegativeKarma();
    }
    
    public void ResetKarma()
    {
        TestResetKarma();
    }
}
