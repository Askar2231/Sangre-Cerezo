using System.Collections;
using UnityEngine;

/// <summary>
/// Clase base abstracta para todos los triggers de tutoriales
/// </summary>
public abstract class TutorialTrigger : MonoBehaviour
{
    [Header("Configuración de Tutorial")]
    [SerializeField] protected TutorialData tutorialData;

    [Header("Configuración de Trigger")]
    [SerializeField] protected bool triggerOnce = true;
    [SerializeField] protected float triggerDelay = 0f;

    [Header("Debug")]
    [SerializeField] protected bool debugMode = false;

    protected bool hasTriggered = false;

    protected virtual void Start()
    {
        if (tutorialData == null)
        {
            Debug.LogWarning($"TutorialTrigger en {gameObject.name}: No tiene TutorialData asignado!");
        }
    }

    /// <summary>
    /// Verifica si el trigger puede activarse
    /// </summary>
    protected virtual bool CanTrigger()
    {
        // No puede activarse si ya se activó y es de una sola vez
        if (triggerOnce && hasTriggered)
        {
            return false;
        }

        // No puede activarse si no hay TutorialData
        if (tutorialData == null)
        {
            return false;
        }

        // No puede activarse si el tutorial ya fue completado y es showOnlyOnce
        if (tutorialData.showOnlyOnce && 
            TutorialManager.Instance != null && 
            TutorialManager.Instance.HasCompletedTutorial(tutorialData.tutorialId))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Dispara el tutorial
    /// </summary>
    protected void FireTrigger()
    {
        if (!CanTrigger())
        {
            if (debugMode) Debug.Log($"{gameObject.name}: No se puede disparar el trigger");
            return;
        }

        hasTriggered = true;

        if (triggerDelay > 0)
        {
            StartCoroutine(FireTriggerDelayed());
        }
        else
        {
            ExecuteTrigger();
        }
    }

    private IEnumerator FireTriggerDelayed()
    {
        yield return new WaitForSeconds(triggerDelay);
        ExecuteTrigger();
    }

    protected virtual void ExecuteTrigger()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.TriggerTutorial(tutorialData);
            if (debugMode) Debug.Log($"{gameObject.name}: Tutorial disparado: {tutorialData.tutorialId}");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: TutorialManager no encontrado!");
        }
    }

    /// <summary>
    /// Resetea el trigger para que pueda activarse de nuevo
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }

    /// <summary>
    /// Deshabilita el trigger permanentemente
    /// </summary>
    public void DisableTrigger()
    {
        hasTriggered = true;
        enabled = false;
    }
}

