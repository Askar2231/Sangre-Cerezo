using UnityEngine;

public class LakeAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private float volumeMultiplier = 1.0f;
    [SerializeField] private bool use3DAudio = true; // Si quieres sonido espacial o ambiente
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private float originalVolume;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            Debug.LogError("❌ [LakeAudio] No se encontró AudioSource en el lago!");
            return;
        }
        
        originalVolume = audioSource.volume;
        
        // Configurar el audio según el tipo deseado
        if (use3DAudio)
        {
            // Audio 3D espacial (suena desde el centro del lago)
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
        }
        else
        {
            // Audio 2D ambiente (suena igual en toda el área)
            audioSource.spatialBlend = 0f;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"✅ [LakeAudio] AudioSource configurado en {gameObject.name}");
            Debug.Log($"🎵 Clip: {audioSource.clip?.name ?? "ninguno"}");
            Debug.Log($"🔊 Volumen original: {originalVolume}");
            Debug.Log($"🎚️ Tipo: {(use3DAudio ? "3D Espacial" : "2D Ambiente")}");
        }
        
        // Verificar Box Collider
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol == null)
        {
            Debug.LogError("❌ [LakeAudio] Se necesita un Box Collider marcado como Trigger!");
        }
        else
        {
            Debug.Log($"✅ [LakeAudio] Box Collider encontrado - Is Trigger: {boxCol.isTrigger}");
            Debug.Log($"📦 Tamaño del área: {boxCol.size}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🚶 [LakeAudio] Entró al área del lago: {other.gameObject.name}");
        }
        
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.volume = originalVolume * volumeMultiplier;
                audioSource.Play();
                Debug.Log($"🌊 [LakeAudio] Sonido del lago ACTIVADO (volumen: {audioSource.volume})");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🚶 [LakeAudio] Salió del área del lago: {other.gameObject.name}");
        }
        
        if (other.CompareTag("Player"))
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("🔇 [LakeAudio] Sonido del lago DESACTIVADO");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar el área de activación en el editor
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Gizmos.color = new Color(0, 0.5f, 1f, 0.3f); // Azul semi-transparente
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCol.center, boxCol.size);
            
            Gizmos.color = new Color(0, 0.5f, 1f, 1f); // Azul sólido para bordes
            Gizmos.DrawWireCube(boxCol.center, boxCol.size);
        }
    }
}