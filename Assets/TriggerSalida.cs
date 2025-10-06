using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trigger que cambia de escena cuando el jugador entra en contacto
/// </summary>
public class TriggerSalida : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string targetSceneName = "MainMenu"; // Nombre de la escena destino
    [SerializeField] private int targetSceneIndex = -1; // Índice de escena (opcional, -1 para usar nombre)
    
    [Header("Trigger Settings")]
    [SerializeField] private bool usePlayerTag = true; // Usar tag "Player" o componente
    [SerializeField] private float changeDelay = 0.5f; // Delay antes de cambiar escena
    
    private bool sceneChangeTriggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        // Evitar múltiples activaciones
        if (sceneChangeTriggered) return;
        
        bool isPlayer = false;
        
        // Verificar si es el jugador
        if (usePlayerTag)
        {
            isPlayer = other.CompareTag("Player");
        }
        else
        {
            // Verificar por componente PlayerBattleController o MovimientoV2
            isPlayer = other.GetComponent<PlayerBattleController>() != null || 
                      other.GetComponent<MovimientoV2>() != null;
        }
        
        if (isPlayer)
        {
            Debug.Log($"Player contacted exit trigger! Changing to scene: {GetTargetSceneName()}");
            ChangeScene();
        }
    }
    
    /// <summary>
    /// Inicia el cambio de escena
    /// </summary>
    private void ChangeScene()
    {
        sceneChangeTriggered = true;
        
        if (changeDelay > 0f)
        {
            // Cambiar después de un delay
            Invoke(nameof(LoadTargetScene), changeDelay);
        }
        else
        {
            // Cambiar inmediatamente
            LoadTargetScene();
        }
    }
    
    /// <summary>
    /// Carga la escena destino
    /// </summary>
    private void LoadTargetScene()
    {
        try
        {
            if (targetSceneIndex >= 0)
            {
                // Usar índice de escena
                Debug.Log($"Loading scene by index: {targetSceneIndex}");
                SceneManager.LoadScene(targetSceneIndex);
            }
            else if (!string.IsNullOrEmpty(targetSceneName))
            {
                // Usar nombre de escena
                Debug.Log($"Loading scene by name: {targetSceneName}");
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogError("No target scene specified! Set either targetSceneName or targetSceneIndex.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
        }
    }
    
    /// <summary>
    /// Obtiene el nombre de la escena destino para debug
    /// </summary>
    private string GetTargetSceneName()
    {
        if (targetSceneIndex >= 0)
            return $"Index {targetSceneIndex}";
        else
            return targetSceneName;
    }
    
    /// <summary>
    /// Método público para cambiar escena desde otros scripts
    /// </summary>
    public void TriggerSceneChange()
    {
        if (!sceneChangeTriggered)
        {
            ChangeScene();
        }
    }
    
    /// <summary>
    /// Resetea el trigger (útil para testing)
    /// </summary>
    public void ResetTrigger()
    {
        sceneChangeTriggered = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualizar el área del trigger
        Gizmos.color = Color.cyan;
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, collider.bounds.size);
        }
    }
}
