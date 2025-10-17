using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper component for spawning and managing enemies in quests/scenes.
/// Simplifies enemy spawning with proper battle system integration.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = false;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    
    [Header("Battle Settings")]
    [SerializeField] private bool autoEnableBattleTrigger = false;
    [SerializeField] private float battleTriggerDelay = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Tracking
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private GameObject lastSpawnedEnemy;
    
    private void Start()
    {
        if (spawnOnStart && enemyPrefab != null)
        {
            SpawnEnemy();
        }
    }
    
    /// <summary>
    /// Spawn an enemy at the configured spawn point
    /// </summary>
    public GameObject SpawnEnemy()
    {
        return SpawnEnemy(enemyPrefab, spawnPoint);
    }
    
    /// <summary>
    /// Spawn a specific enemy prefab at a specific location
    /// </summary>
    public GameObject SpawnEnemy(GameObject prefab, Transform spawnLocation)
    {
        if (prefab == null)
        {
            Debug.LogError("[EnemySpawner] Enemy prefab is null!");
            return null;
        }
        
        if (spawnLocation == null)
        {
            Debug.LogWarning("[EnemySpawner] Spawn location is null, using spawner position");
            spawnLocation = transform;
        }
        
        // Instantiate enemy
        GameObject spawnedEnemy = Instantiate(prefab, spawnLocation.position, spawnLocation.rotation);
        
        // Validate enemy has required components
        EnemyBattleController enemyController = spawnedEnemy.GetComponent<EnemyBattleController>();
        if (enemyController == null)
        {
            Debug.LogError($"[EnemySpawner] Spawned enemy '{spawnedEnemy.name}' missing EnemyBattleController!");
            Destroy(spawnedEnemy);
            return null;
        }
        
        // Check for BattleTrigger
        BattleTrigger battleTrigger = spawnedEnemy.GetComponentInChildren<BattleTrigger>();
        if (battleTrigger == null)
        {
            Debug.LogWarning($"[EnemySpawner] Enemy '{spawnedEnemy.name}' has no BattleTrigger! " +
                           "Player won't be able to initiate battle automatically.");
        }
        else if (!autoEnableBattleTrigger)
        {
            // Disable trigger initially (quest might want to control when it's active)
            battleTrigger.SetTriggerActive(false);
            
            if (debugMode)
            {
                Debug.Log($"[EnemySpawner] BattleTrigger disabled on spawn. Call EnableLastSpawnedEnemyTrigger() to activate.");
            }
        }
        else
        {
            // Enable trigger after a delay
            if (battleTriggerDelay > 0)
            {
                StartCoroutine(EnableTriggerAfterDelay(battleTrigger, battleTriggerDelay));
            }
        }
        
        // Track spawned enemy
        spawnedEnemies.Add(spawnedEnemy);
        lastSpawnedEnemy = spawnedEnemy;
        
        if (debugMode)
        {
            Debug.Log($"<color=green>[EnemySpawner] Spawned enemy '{spawnedEnemy.name}' at {spawnLocation.position}</color>");
        }
        
        return spawnedEnemy;
    }
    
    /// <summary>
    /// Enable battle trigger on the last spawned enemy
    /// </summary>
    public void EnableLastSpawnedEnemyTrigger()
    {
        if (lastSpawnedEnemy == null)
        {
            Debug.LogWarning("[EnemySpawner] No enemy spawned yet!");
            return;
        }
        
        BattleTrigger trigger = lastSpawnedEnemy.GetComponentInChildren<BattleTrigger>();
        if (trigger != null)
        {
            trigger.SetTriggerActive(true);
            
            if (debugMode)
            {
                Debug.Log($"<color=cyan>[EnemySpawner] Battle trigger enabled for {lastSpawnedEnemy.name}</color>");
            }
        }
    }
    
    /// <summary>
    /// Coroutine to enable trigger after delay
    /// </summary>
    private System.Collections.IEnumerator EnableTriggerAfterDelay(BattleTrigger trigger, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (trigger != null)
        {
            trigger.SetTriggerActive(true);
            
            if (debugMode)
            {
                Debug.Log($"<color=cyan>[EnemySpawner] Battle trigger enabled after {delay}s delay</color>");
            }
        }
    }
    
    /// <summary>
    /// Destroy the last spawned enemy
    /// </summary>
    public void DestroyLastSpawnedEnemy()
    {
        if (lastSpawnedEnemy != null)
        {
            if (debugMode)
            {
                Debug.Log($"<color=red>[EnemySpawner] Destroying {lastSpawnedEnemy.name}</color>");
            }
            
            spawnedEnemies.Remove(lastSpawnedEnemy);
            Destroy(lastSpawnedEnemy);
            lastSpawnedEnemy = null;
        }
    }
    
    /// <summary>
    /// Destroy all spawned enemies
    /// </summary>
    public void DestroyAllSpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        spawnedEnemies.Clear();
        lastSpawnedEnemy = null;
        
        if (debugMode)
        {
            Debug.Log("[EnemySpawner] All spawned enemies destroyed");
        }
    }
    
    /// <summary>
    /// Get the last spawned enemy GameObject
    /// </summary>
    public GameObject GetLastSpawnedEnemy() => lastSpawnedEnemy;
    
    /// <summary>
    /// Get the EnemyBattleController of the last spawned enemy
    /// </summary>
    public EnemyBattleController GetLastSpawnedEnemyController()
    {
        if (lastSpawnedEnemy == null) return null;
        return lastSpawnedEnemy.GetComponent<EnemyBattleController>();
    }
    
    /// <summary>
    /// Get all spawned enemies
    /// </summary>
    public List<GameObject> GetSpawnedEnemies() => new List<GameObject>(spawnedEnemies);
    
    /// <summary>
    /// Get count of currently alive spawned enemies
    /// </summary>
    public int GetAliveEnemyCount()
    {
        // Remove null entries (destroyed enemies)
        spawnedEnemies.RemoveAll(e => e == null);
        return spawnedEnemies.Count;
    }
    
    private void OnDestroy()
    {
        // Clean up references
        spawnedEnemies.Clear();
        lastSpawnedEnemy = null;
    }
}
