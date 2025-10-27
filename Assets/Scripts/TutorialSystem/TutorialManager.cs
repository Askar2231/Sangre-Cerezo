using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestor central del sistema de tutoriales
/// Maneja el estado de los tutoriales, su visualización y persistencia
/// </summary>
public class TutorialManager : MonoBehaviour
{
    #region Singleton
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TutorialManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    _instance = go.AddComponent<TutorialManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    [Header("Referencias de UI")]
    [SerializeField] private SimpleDialogPresenter simpleDialogPresenter;
    [SerializeField] private CardPopoverPresenter cardPopoverPresenter;

    [Header("Configuración")]
    [SerializeField] private bool tutorialsEnabled = true;
    [SerializeField] private bool debugMode = false;

    [Header("⚠️ TESTING ONLY - Remove for Production ⚠️")]
    [Tooltip("WARNING: Resets ALL tutorials every time the game loads. ONLY use for testing!")]
    [SerializeField] private bool resetTutorialsOnLoad = true;

    [Header("Debug Hotkeys (Testing)")]
    [Tooltip("Modifier key 1 for debug hotkeys (default: LeftControl)")]
    [SerializeField] private KeyCode modifier1 = KeyCode.LeftControl;
    [Tooltip("Modifier key 2 for debug hotkeys (default: LeftShift)")]
    [SerializeField] private KeyCode modifier2 = KeyCode.LeftShift;
    [Tooltip("Reset all tutorials (default: R)")]
    [SerializeField] private KeyCode resetTutorialsKey = KeyCode.R;
    [Tooltip("Toggle tutorials on/off (default: T)")]
    [SerializeField] private KeyCode toggleTutorialsKey = KeyCode.T;
    [Tooltip("Debug log tutorial status (default: D)")]
    [SerializeField] private KeyCode debugStatusKey = KeyCode.D;

    // Estado interno
    private Dictionary<string, bool> completedTutorials = new Dictionary<string, bool>();
    private Queue<TutorialData> tutorialQueue = new Queue<TutorialData>();
    private TutorialData currentActiveTutorial = null;
    private bool isTutorialActive = false;
    private float originalTimeScale = 1f;

    // Eventos
    public UnityEvent<string> OnTutorialStarted = new UnityEvent<string>();
    public UnityEvent<string> OnTutorialCompleted = new UnityEvent<string>();

    #region Unity Lifecycle
    private void Awake()
    {
        // Singleton setup
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCompletedTutorials();

        // ⚠️ TESTING ONLY: Auto-reset tutorials on load
        if (resetTutorialsOnLoad)
        {
            Debug.LogWarning("╔════════════════════════════════════════════════════════════╗");
            Debug.LogWarning("║  ⚠️  WARNING: TUTORIALS AUTO-RESET ON LOAD ENABLED  ⚠️   ║");
            Debug.LogWarning("║  This is for TESTING ONLY!                                ║");
            Debug.LogWarning("║  All tutorials will reset every time the game loads.      ║");
            Debug.LogWarning("║  DISABLE 'resetTutorialsOnLoad' before production build!  ║");
            Debug.LogWarning("╚════════════════════════════════════════════════════════════╝");
            
            ResetAllTutorials();
            Debug.Log("<color=yellow>🔄 [TutorialManager] Tutorials automatically reset on load (Testing Mode)</color>");
        }
    }

    private void Start()
    {
        // Validar referencias
        if (simpleDialogPresenter == null)
        {
            Debug.LogWarning("TutorialManager: SimpleDialogPresenter no está asignado!");
        }
        if (cardPopoverPresenter == null)
        {
            Debug.LogWarning("TutorialManager: CardPopoverPresenter no está asignado!");
        }
    }

    private void Update()
    {
        // Procesar cola de tutoriales
        if (!isTutorialActive && tutorialQueue.Count > 0)
        {
            ProcessNextTutorial();
        }

        // Debug hotkeys (configurable in Inspector)
        bool modifiersPressed = Input.GetKey(modifier1) && Input.GetKey(modifier2);

        if (modifiersPressed)
        {
            // Reset all tutorials
            if (Input.GetKeyDown(resetTutorialsKey))
            {
                ResetAllTutorials();
                Debug.Log($"<color=yellow>🔄 [TutorialManager] ALL TUTORIALS RESET! ({modifier1}+{modifier2}+{resetTutorialsKey} pressed)</color>");
            }

            // Toggle tutorials on/off
            if (Input.GetKeyDown(toggleTutorialsKey))
            {
                SetTutorialsEnabled(!tutorialsEnabled);
                Debug.Log($"<color=cyan>🎓 [TutorialManager] Tutorials {(tutorialsEnabled ? "ENABLED" : "DISABLED")} ({modifier1}+{modifier2}+{toggleTutorialsKey} pressed)</color>");
            }

            // Debug log tutorial status
            if (Input.GetKeyDown(debugStatusKey))
            {
                DebugLogTutorialStatus();
            }
        }
    }
    #endregion

    #region Public API
    /// <summary>
    /// Dispara un tutorial por su ID
    /// </summary>
    public void TriggerTutorial(TutorialData tutorialData)
    {
        if (!tutorialsEnabled)
        {
            if (debugMode) Debug.Log($"Tutoriales deshabilitados. Ignorando: {tutorialData.tutorialId}");
            return;
        }

        if (tutorialData == null)
        {
            Debug.LogError("TutorialManager: TutorialData es null!");
            return;
        }

        // Verificar si ya se completó
        if (tutorialData.showOnlyOnce && HasCompletedTutorial(tutorialData.tutorialId))
        {
            if (debugMode) Debug.Log($"Tutorial ya completado: {tutorialData.tutorialId}");
            return;
        }

        // Agregar a la cola
        tutorialQueue.Enqueue(tutorialData);
        if (debugMode) Debug.Log($"Tutorial agregado a la cola: {tutorialData.tutorialId}");
    }

    /// <summary>
    /// Marca un tutorial como completado
    /// </summary>
    public void MarkTutorialComplete(string tutorialId)
    {
        if (!completedTutorials.ContainsKey(tutorialId))
        {
            completedTutorials[tutorialId] = true;
            SaveCompletedTutorials();
            OnTutorialCompleted?.Invoke(tutorialId);
            if (debugMode) Debug.Log($"Tutorial completado: {tutorialId}");
        }
    }

    /// <summary>
    /// Verifica si un tutorial ya fue completado
    /// </summary>
    public bool HasCompletedTutorial(string tutorialId)
    {
        return completedTutorials.ContainsKey(tutorialId) && completedTutorials[tutorialId];
    }

    /// <summary>
    /// Resetea todos los tutoriales (útil para testing)
    /// </summary>
    public void ResetAllTutorials()
    {
        completedTutorials.Clear();
        PlayerPrefs.DeleteKey("CompletedTutorials");
        PlayerPrefs.Save();
        if (debugMode) Debug.Log("Todos los tutoriales reseteados");
    }

    /// <summary>
    /// Habilita o deshabilita el sistema de tutoriales
    /// </summary>
    public void SetTutorialsEnabled(bool enabled)
    {
        tutorialsEnabled = enabled;
        PlayerPrefs.SetInt("TutorialsEnabled", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Obtiene si los tutoriales están habilitados
    /// </summary>
    public bool AreTutorialsEnabled()
    {
        return tutorialsEnabled;
    }
    #endregion

    #region Private Methods
    private void ProcessNextTutorial()
    {
        if (tutorialQueue.Count == 0) return;

        currentActiveTutorial = tutorialQueue.Dequeue();
        isTutorialActive = true;

        OnTutorialStarted?.Invoke(currentActiveTutorial.tutorialId);

        // Pausar juego si es necesario
        if (currentActiveTutorial.pauseGame)
        {
            PauseGame();
        }

        // Mostrar tutorial según su tipo
        switch (currentActiveTutorial.tutorialType)
        {
            case TutorialType.SimpleDialog:
                ShowSimpleDialog();
                break;
            case TutorialType.CardPopover:
                ShowCardPopover();
                break;
        }
    }

    private void ShowSimpleDialog()
    {
        if (simpleDialogPresenter == null)
        {
            Debug.LogError("TutorialManager: SimpleDialogPresenter no está asignado!");
            FinishCurrentTutorial();
            return;
        }

        simpleDialogPresenter.ShowDialog(
            currentActiveTutorial.dialogText,
            currentActiveTutorial.dialogPosition,
            currentActiveTutorial.displayDuration,
            OnSimpleDialogClosed
        );
    }

    private void ShowCardPopover()
    {
        if (cardPopoverPresenter == null)
        {
            Debug.LogError("TutorialManager: CardPopoverPresenter no está asignado!");
            FinishCurrentTutorial();
            return;
        }

        cardPopoverPresenter.ShowCards(
            currentActiveTutorial.cards,
            OnCardPopoverClosed
        );
    }

    private void OnSimpleDialogClosed()
    {
        FinishCurrentTutorial();
    }

    private void OnCardPopoverClosed()
    {
        FinishCurrentTutorial();
    }

    private void FinishCurrentTutorial()
    {
        if (currentActiveTutorial == null) return;

        // Marcar como completado
        MarkTutorialComplete(currentActiveTutorial.tutorialId);

        // Reanudar juego si fue pausado
        if (currentActiveTutorial.pauseGame)
        {
            ResumeGame();
        }

        currentActiveTutorial = null;
        isTutorialActive = false;
    }

    private void PauseGame()
    {
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (debugMode) Debug.Log("Juego pausado por tutorial");
    }

    private void ResumeGame()
    {
        Time.timeScale = originalTimeScale;
        if (debugMode) Debug.Log("Juego reanudado");
    }

    private void SaveCompletedTutorials()
    {
        List<string> completedList = new List<string>();
        foreach (var kvp in completedTutorials)
        {
            if (kvp.Value)
            {
                completedList.Add(kvp.Key);
            }
        }

        string serialized = string.Join(",", completedList);
        PlayerPrefs.SetString("CompletedTutorials", serialized);
        PlayerPrefs.Save();
    }

    private void LoadCompletedTutorials()
    {
        completedTutorials.Clear();

        // Cargar estado de tutoriales habilitados
        if (PlayerPrefs.HasKey("TutorialsEnabled"))
        {
            tutorialsEnabled = PlayerPrefs.GetInt("TutorialsEnabled") == 1;
        }

        // Cargar tutoriales completados
        if (PlayerPrefs.HasKey("CompletedTutorials"))
        {
            string serialized = PlayerPrefs.GetString("CompletedTutorials");
            if (!string.IsNullOrEmpty(serialized))
            {
                string[] tutorialIds = serialized.Split(',');
                foreach (string id in tutorialIds)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        completedTutorials[id] = true;
                    }
                }
            }
        }

        if (debugMode) Debug.Log($"Cargados {completedTutorials.Count} tutoriales completados");
    }
    #endregion

    #region Debug Methods
    public void DebugLogTutorialStatus()
    {
        Debug.Log("=== ESTADO DE TUTORIALES ===");
        Debug.Log($"Habilitados: {tutorialsEnabled}");
        Debug.Log($"Tutorial activo: {(isTutorialActive ? currentActiveTutorial?.tutorialId : "Ninguno")}");
        Debug.Log($"Tutoriales en cola: {tutorialQueue.Count}");
        Debug.Log($"Tutoriales completados: {completedTutorials.Count}");
        foreach (var kvp in completedTutorials)
        {
            Debug.Log($"  - {kvp.Key}");
        }
    }
    #endregion
}
