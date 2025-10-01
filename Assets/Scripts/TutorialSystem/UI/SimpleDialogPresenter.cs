using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Presenta tutoriales simples como diálogos en la parte superior de la pantalla
/// </summary>
public class SimpleDialogPresenter : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject dialogContainer;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Configuración de Animación")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Configuración de Posición")]
    [SerializeField] private RectTransform topLeftAnchor;
    [SerializeField] private RectTransform topCenterAnchor;
    [SerializeField] private RectTransform topRightAnchor;

    private System.Action onDialogClosed;
    private Coroutine currentDialogCoroutine;

    private void Awake()
    {
        if (dialogContainer != null)
        {
            dialogContainer.SetActive(false);
        }

        if (canvasGroup == null)
        {
            canvasGroup = dialogContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = dialogContainer.AddComponent<CanvasGroup>();
            }
        }

        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Muestra un diálogo simple
    /// </summary>
    public void ShowDialog(string text, DialogPosition position, float duration, System.Action onClosed)
    {
        if (currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
        }

        onDialogClosed = onClosed;
        currentDialogCoroutine = StartCoroutine(ShowDialogCoroutine(text, position, duration));
    }

    private IEnumerator ShowDialogCoroutine(string text, DialogPosition position, float duration)
    {
        // Configurar posición
        SetDialogPosition(position);

        // Procesar texto con placeholders de input
        string processedText = InputIconMapper.Instance.ProcessTextPlaceholders(text);
        dialogText.text = processedText;

        // Activar y hacer fade in
        dialogContainer.SetActive(true);
        yield return FadeIn();

        // Esperar duración especificada (0 = infinito hasta que se cierre manualmente)
        if (duration > 0)
        {
            yield return new WaitForSecondsRealtime(duration);
        }
        else
        {
            // Esperar confirmación del jugador
            yield return WaitForPlayerConfirmation();
        }

        // Fade out y cerrar
        yield return FadeOut();
        dialogContainer.SetActive(false);

        onDialogClosed?.Invoke();
        currentDialogCoroutine = null;
    }

    private void SetDialogPosition(DialogPosition position)
    {
        RectTransform targetAnchor = null;

        switch (position)
        {
            case DialogPosition.TopLeft:
                targetAnchor = topLeftAnchor;
                break;
            case DialogPosition.TopCenter:
                targetAnchor = topCenterAnchor;
                break;
            case DialogPosition.TopRight:
                targetAnchor = topRightAnchor;
                break;
        }

        if (targetAnchor != null && dialogContainer != null)
        {
            RectTransform dialogRect = dialogContainer.GetComponent<RectTransform>();
            dialogRect.anchorMin = targetAnchor.anchorMin;
            dialogRect.anchorMax = targetAnchor.anchorMax;
            dialogRect.anchoredPosition = targetAnchor.anchoredPosition;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    private IEnumerator WaitForPlayerConfirmation()
    {
        // Esperar hasta que el jugador presione Confirmar
        while (true)
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                break;
            }

            if (UnityEngine.InputSystem.Gamepad.current != null && 
                UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Cierra el diálogo inmediatamente
    /// </summary>
    public void CloseDialog()
    {
        if (currentDialogCoroutine != null)
        {
            StopCoroutine(currentDialogCoroutine);
            currentDialogCoroutine = null;
        }

        dialogContainer.SetActive(false);
        canvasGroup.alpha = 0f;
        onDialogClosed?.Invoke();
    }
}

