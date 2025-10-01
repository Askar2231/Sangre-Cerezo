using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Presenta tutoriales como tarjetas emergentes con imágenes y texto
/// </summary>
public class CardPopoverPresenter : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject cardContainer;
    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject confirmPrompt;
    [SerializeField] private TextMeshProUGUI confirmPromptText;

    [Header("Indicador de Progreso")]
    [SerializeField] private TextMeshProUGUI pageIndicator;
    [SerializeField] private Transform dotIndicatorContainer;
    [SerializeField] private GameObject dotIndicatorPrefab;

    [Header("Configuración de Animación")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float cardTransitionDuration = 0.2f;

    [Header("Configuración")]
    [SerializeField] private Color activeDotColor = Color.white;
    [SerializeField] private Color inactiveDotColor = Color.gray;

    private List<TutorialCard> currentCards;
    private int currentCardIndex = 0;
    private System.Action onCardsCompleted;
    private Coroutine currentCardCoroutine;
    private List<Image> dotIndicators = new List<Image>();

    private void Awake()
    {
        if (cardContainer != null)
        {
            cardContainer.SetActive(false);
        }

        if (canvasGroup == null)
        {
            canvasGroup = cardContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = cardContainer.AddComponent<CanvasGroup>();
            }
        }

        canvasGroup.alpha = 0f;

        if (confirmPrompt != null)
        {
            confirmPrompt.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra una secuencia de tarjetas
    /// </summary>
    public void ShowCards(List<TutorialCard> cards, System.Action onCompleted)
    {
        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning("CardPopoverPresenter: No hay tarjetas para mostrar");
            onCompleted?.Invoke();
            return;
        }

        if (currentCardCoroutine != null)
        {
            StopCoroutine(currentCardCoroutine);
        }

        currentCards = cards;
        currentCardIndex = 0;
        onCardsCompleted = onCompleted;

        CreateDotIndicators(cards.Count);
        currentCardCoroutine = StartCoroutine(ShowCardsSequence());
    }

    private IEnumerator ShowCardsSequence()
    {
        // Activar container
        cardContainer.SetActive(true);

        // Fade in inicial
        yield return FadeIn();

        // Mostrar cada tarjeta
        while (currentCardIndex < currentCards.Count)
        {
            yield return ShowCard(currentCards[currentCardIndex]);

            // Si no es la última tarjeta, hacer transición
            if (currentCardIndex < currentCards.Count - 1)
            {
                yield return CardTransition();
            }

            currentCardIndex++;
        }

        // Fade out final
        yield return FadeOut();
        cardContainer.SetActive(false);

        // Callback de completado
        onCardsCompleted?.Invoke();
        currentCardCoroutine = null;
    }

    private IEnumerator ShowCard(TutorialCard card)
    {
        // Actualizar contenido de la tarjeta
        if (cardImage != null)
        {
            if (card.cardImage != null)
            {
                cardImage.sprite = card.cardImage;
                cardImage.gameObject.SetActive(true);
            }
            else
            {
                cardImage.gameObject.SetActive(false);
            }
        }

        // Procesar texto con placeholders
        string processedText = InputIconMapper.Instance.ProcessTextPlaceholders(card.cardText);
        if (cardText != null)
        {
            cardText.text = processedText;
        }

        // Actualizar indicadores de progreso
        UpdateProgressIndicators();

        // Esperar tiempo mínimo de visualización
        if (card.minDisplayTime > 0)
        {
            yield return new WaitForSecondsRealtime(card.minDisplayTime);
        }

        // Mostrar prompt de confirmación si es necesario
        if (card.requireConfirmation)
        {
            if (confirmPrompt != null)
            {
                confirmPrompt.SetActive(true);
                if (confirmPromptText != null)
                {
                    string promptText = "Presiona {Confirm} para continuar";
                    confirmPromptText.text = InputIconMapper.Instance.ProcessTextPlaceholders(promptText);
                }
            }

            // Esperar confirmación del jugador
            yield return WaitForPlayerConfirmation();

            if (confirmPrompt != null)
            {
                confirmPrompt.SetActive(false);
            }
        }
        else
        {
            // Auto-avanzar
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private IEnumerator CardTransition()
    {
        // Fade out rápido
        float elapsed = 0f;
        while (elapsed < cardTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / cardTransitionDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        yield return new WaitForSecondsRealtime(0.1f);

        // Fade in rápido
        elapsed = 0f;
        while (elapsed < cardTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / cardTransitionDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
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
        while (true)
        {
            // Keyboard
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                break;
            }

            // Gamepad
            if (UnityEngine.InputSystem.Gamepad.current != null && 
                UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                break;
            }

            // Permitir saltar todas las tarjetas con Cancel
            if (UnityEngine.InputSystem.Keyboard.current != null && 
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SkipAllCards();
                yield break;
            }

            if (UnityEngine.InputSystem.Gamepad.current != null && 
                UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                SkipAllCards();
                yield break;
            }

            yield return null;
        }
    }

    private void CreateDotIndicators(int count)
    {
        // Limpiar indicadores existentes
        foreach (var dot in dotIndicators)
        {
            if (dot != null)
            {
                Destroy(dot.gameObject);
            }
        }
        dotIndicators.Clear();

        // Crear nuevos indicadores si hay prefab y container
        if (dotIndicatorPrefab != null && dotIndicatorContainer != null)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject dotGO = Instantiate(dotIndicatorPrefab, dotIndicatorContainer);
                Image dotImage = dotGO.GetComponent<Image>();
                if (dotImage != null)
                {
                    dotIndicators.Add(dotImage);
                }
            }
        }

        UpdateProgressIndicators();
    }

    private void UpdateProgressIndicators()
    {
        // Actualizar texto de página
        if (pageIndicator != null && currentCards != null)
        {
            pageIndicator.text = $"{currentCardIndex + 1} / {currentCards.Count}";
        }

        // Actualizar puntos
        for (int i = 0; i < dotIndicators.Count; i++)
        {
            if (dotIndicators[i] != null)
            {
                dotIndicators[i].color = (i == currentCardIndex) ? activeDotColor : inactiveDotColor;
            }
        }
    }

    private void SkipAllCards()
    {
        if (currentCardCoroutine != null)
        {
            StopCoroutine(currentCardCoroutine);
            currentCardCoroutine = null;
        }

        cardContainer.SetActive(false);
        canvasGroup.alpha = 0f;

        onCardsCompleted?.Invoke();
    }

    /// <summary>
    /// Cierra las tarjetas inmediatamente
    /// </summary>
    public void CloseCards()
    {
        SkipAllCards();
    }
}

