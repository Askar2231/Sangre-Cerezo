using UnityEngine;

/// <summary>
/// Defines combat positions for player, enemy, and camera
/// Place this component in your scene to set up battle staging
/// </summary>
public class BattlePositionSetup : MonoBehaviour
{
    [Header("Combat Positions")]
    [Tooltip("Where the player should stand during combat")]
    [SerializeField] private Transform playerCombatPosition;
    
    [Tooltip("Where the enemy should stand during combat")]
    [SerializeField] private Transform enemyCombatPosition;
    
    [Tooltip("Where the camera should be positioned for combat")]
    [SerializeField] private Transform cameraCombatPosition;
    
    [Header("Rotation Settings")]
    [Tooltip("Should characters rotate to face each other?")]
    [SerializeField] private bool autoRotateToFaceOpponent = true;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color playerGizmoColor = Color.blue;
    [SerializeField] private Color enemyGizmoColor = Color.red;
    [SerializeField] private Color cameraGizmoColor = Color.yellow;
    [SerializeField] private float gizmoSize = 0.5f;
    
    // Public properties
    public Transform PlayerCombatPosition => playerCombatPosition;
    public Transform EnemyCombatPosition => enemyCombatPosition;
    public Transform CameraCombatPosition => cameraCombatPosition;
    public bool AutoRotateToFaceOpponent => autoRotateToFaceOpponent;
    
    /// <summary>
    /// Validate that all positions are set
    /// </summary>
    public bool IsValid()
    {
        if (playerCombatPosition == null)
        {
            Debug.LogError($"[BattlePositionSetup] Player combat position not assigned on {gameObject.name}");
            return false;
        }
        
        if (enemyCombatPosition == null)
        {
            Debug.LogError($"[BattlePositionSetup] Enemy combat position not assigned on {gameObject.name}");
            return false;
        }
        
        if (cameraCombatPosition == null)
        {
            Debug.LogWarning($"[BattlePositionSetup] Camera combat position not assigned on {gameObject.name}");
            // Camera is optional, so we don't return false
        }
        
        return true;
    }
    
    /// <summary>
    /// Create default positions if they don't exist
    /// </summary>
    [ContextMenu("Create Default Position Markers")]
    private void CreateDefaultPositions()
    {
        if (playerCombatPosition == null)
        {
            GameObject playerPos = new GameObject("PlayerCombatPosition");
            playerPos.transform.SetParent(transform);
            playerPos.transform.localPosition = new Vector3(-2f, 0f, 0f);
            playerPos.transform.localRotation = Quaternion.Euler(0, 90, 0);
            playerCombatPosition = playerPos.transform;
            Debug.Log("Created PlayerCombatPosition");
        }
        
        if (enemyCombatPosition == null)
        {
            GameObject enemyPos = new GameObject("EnemyCombatPosition");
            enemyPos.transform.SetParent(transform);
            enemyPos.transform.localPosition = new Vector3(2f, 0f, 0f);
            enemyPos.transform.localRotation = Quaternion.Euler(0, -90, 0);
            enemyCombatPosition = enemyPos.transform;
            Debug.Log("Created EnemyCombatPosition");
        }
        
        if (cameraCombatPosition == null)
        {
            GameObject cameraPos = new GameObject("CameraCombatPosition");
            cameraPos.transform.SetParent(transform);
            cameraPos.transform.localPosition = new Vector3(0f, 2f, -5f);
            cameraPos.transform.localRotation = Quaternion.Euler(15, 0, 0);
            cameraCombatPosition = cameraPos.transform;
            Debug.Log("Created CameraCombatPosition");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Draw player position
        if (playerCombatPosition != null)
        {
            Gizmos.color = playerGizmoColor;
            Gizmos.DrawWireSphere(playerCombatPosition.position, gizmoSize);
            Gizmos.DrawLine(playerCombatPosition.position, 
                           playerCombatPosition.position + playerCombatPosition.forward * 1f);
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(playerCombatPosition.position + Vector3.up * (gizmoSize + 0.2f), 
                                      "Player", 
                                      new GUIStyle() { normal = new GUIStyleState() { textColor = playerGizmoColor } });
            #endif
        }
        
        // Draw enemy position
        if (enemyCombatPosition != null)
        {
            Gizmos.color = enemyGizmoColor;
            Gizmos.DrawWireSphere(enemyCombatPosition.position, gizmoSize);
            Gizmos.DrawLine(enemyCombatPosition.position, 
                           enemyCombatPosition.position + enemyCombatPosition.forward * 1f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(enemyCombatPosition.position + Vector3.up * (gizmoSize + 0.2f), 
                                      "Enemy", 
                                      new GUIStyle() { normal = new GUIStyleState() { textColor = enemyGizmoColor } });
            #endif
        }
        
        // Draw camera position
        if (cameraCombatPosition != null)
        {
            Gizmos.color = cameraGizmoColor;
            Gizmos.DrawWireCube(cameraCombatPosition.position, Vector3.one * gizmoSize * 1.5f);
            Gizmos.DrawLine(cameraCombatPosition.position, 
                           cameraCombatPosition.position + cameraCombatPosition.forward * 2f);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(cameraCombatPosition.position + Vector3.up * (gizmoSize + 0.2f), 
                                      "Camera", 
                                      new GUIStyle() { normal = new GUIStyleState() { textColor = cameraGizmoColor } });
            #endif
        }
        
        // Draw line between player and enemy
        if (playerCombatPosition != null && enemyCombatPosition != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(playerCombatPosition.position, enemyCombatPosition.position);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw frustum from camera to combat area
        if (cameraCombatPosition != null && playerCombatPosition != null && enemyCombatPosition != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            
            Vector3 center = (playerCombatPosition.position + enemyCombatPosition.position) / 2f;
            
            Gizmos.DrawLine(cameraCombatPosition.position, playerCombatPosition.position);
            Gizmos.DrawLine(cameraCombatPosition.position, enemyCombatPosition.position);
            Gizmos.DrawLine(cameraCombatPosition.position, center);
        }
    }
}
