using UnityEngine;

/// <summary>
/// TEMPORARY diagnostic script to test parry system manually
/// Add this to any GameObject in your battle scene for testing
/// </summary>
public class ParrySystemTester : MonoBehaviour
{
    [Header("References - Drag from Scene")]
    [SerializeField] private ParrySystem parrySystem;
    [SerializeField] private BattleInputManager inputManager;
    [SerializeField] private BattleManagerV2 battleManager;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode testOpenParryWindowKey = KeyCode.P;
    [SerializeField] private KeyCode testPressParryKey = KeyCode.O;
    [SerializeField] private KeyCode testCheckReferencesKey = KeyCode.R;
    
    private void Update()
    {
        // Press R to check all references
        if (Input.GetKeyDown(testCheckReferencesKey))
        {
            TestCheckReferences();
        }
        
        // Press P to manually open parry window
        if (Input.GetKeyDown(testOpenParryWindowKey))
        {
            TestOpenParryWindow();
        }
        
        // Press O to simulate parry button press
        if (Input.GetKeyDown(testPressParryKey))
        {
            TestPressParry();
        }
    }
    
    [ContextMenu("1. Check All References")]
    private void TestCheckReferences()
    {
        Debug.Log("========== PARRY SYSTEM DIAGNOSTIC ==========");
        Debug.Log($"ParrySystem: {(parrySystem != null ? "✅ ASSIGNED" : "❌ NULL")}");
        Debug.Log($"BattleInputManager: {(inputManager != null ? "✅ ASSIGNED" : "❌ NULL")}");
        Debug.Log($"BattleManagerV2: {(battleManager != null ? "✅ ASSIGNED" : "❌ NULL")}");
        
        if (parrySystem != null)
        {
            Debug.Log($"ParrySystem.IsParryWindowActive: {parrySystem.IsParryWindowActive}");
        }
        
        if (inputManager != null)
        {
            Debug.Log($"InputManager.CurrentInputState: {inputManager.CurrentInputState}");
            Debug.Log($"InputManager.IsParryWindowActive: {inputManager.IsParryWindowActive}");
        }
        
        Debug.Log("==============================================");
    }
    
    [ContextMenu("2. Test: Open Parry Window")]
    private void TestOpenParryWindow()
    {
        Debug.Log("========== TEST: OPENING PARRY WINDOW ==========");
        
        if (parrySystem == null)
        {
            Debug.LogError("❌ ParrySystem is NULL! Assign it in Inspector!");
            return;
        }
        
        Debug.Log("Calling ParrySystem.OpenParryWindow()...");
        parrySystem.OpenParryWindow();
        
        Debug.Log("Parry window should now be open. Check console for parry system logs.");
        Debug.Log("================================================");
    }
    
    [ContextMenu("3. Test: Press Parry Button")]
    private void TestPressParry()
    {
        Debug.Log("========== TEST: PRESSING PARRY BUTTON ==========");
        
        if (parrySystem == null)
        {
            Debug.LogError("❌ ParrySystem is NULL! Assign it in Inspector!");
            return;
        }
        
        if (!parrySystem.IsParryWindowActive)
        {
            Debug.LogWarning("⚠️ Parry window is NOT ACTIVE! Open it first (press P or use Context Menu)");
            return;
        }
        
        Debug.Log("Calling ParrySystem.ProcessParryInput()...");
        parrySystem.ProcessParryInput();
        
        Debug.Log("Parry should have been processed. Check console for result.");
        Debug.Log("================================================");
    }
    
    [ContextMenu("4. Test: Full Parry Sequence")]
    private void TestFullParrySequence()
    {
        StartCoroutine(TestFullParrySequenceCoroutine());
    }
    
    private System.Collections.IEnumerator TestFullParrySequenceCoroutine()
    {
        Debug.Log("========== TEST: FULL PARRY SEQUENCE ==========");
        
        if (parrySystem == null)
        {
            Debug.LogError("❌ ParrySystem is NULL!");
            yield break;
        }
        
        // Step 1: Open window
        Debug.Log("Step 1: Opening parry window...");
        parrySystem.OpenParryWindow();
        
        yield return new WaitForSeconds(0.15f); // Wait in middle of window (0.3s total)
        
        // Step 2: Press parry
        Debug.Log("Step 2: Pressing parry button...");
        parrySystem.ProcessParryInput();
        
        Debug.Log("========== FULL SEQUENCE COMPLETE ==========");
    }
    
    private void OnGUI()
    {
        // Draw helper UI
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Box("PARRY SYSTEM TESTER");
        
        GUILayout.Label($"ParrySystem: {(parrySystem != null ? "✅" : "❌")}");
        GUILayout.Label($"InputManager: {(inputManager != null ? "✅" : "❌")}");
        GUILayout.Label($"BattleManager: {(battleManager != null ? "✅" : "❌")}");
        
        GUILayout.Space(10);
        GUILayout.Label("Keyboard Shortcuts:");
        GUILayout.Label($"[{testCheckReferencesKey}] - Check References");
        GUILayout.Label($"[{testOpenParryWindowKey}] - Open Parry Window");
        GUILayout.Label($"[{testPressParryKey}] - Press Parry");
        
        if (parrySystem != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Parry Window Active: {(parrySystem.IsParryWindowActive ? "YES ✅" : "NO ❌")}");
        }
        
        if (inputManager != null)
        {
            GUILayout.Label($"Input State: {inputManager.CurrentInputState}");
        }
        
        GUILayout.EndArea();
    }
}
