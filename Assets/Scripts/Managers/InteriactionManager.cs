using UnityEngine;
using TMPro; 
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI sentenceText;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Input Actions")]
    [Tooltip("Input action for Choice 1 (drag from Input Actions asset) !! Will not update text/icons dynamically !!")]
    public InputActionReference choice1Action;
    
    [Tooltip("Input action for Choice 2 (drag from Input Actions asset) !! Will not update text/icons dynamically !!")]
    public InputActionReference choice2Action;

    private Queue<string> sentences;
    private List<Button> currentChoiceButtons;
    private float choiceInputCooldown = 0f;
    private const float CHOICE_INPUT_COOLDOWN_TIME = 0.3f; // Prevent interact button from triggering choices

    private void Awake()
    {
        if (Instance == null) { Instance = this; } 
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        sentences = new Queue<string>();
        currentChoiceButtons = new List<Button>();
        dialoguePanel.SetActive(false);
    }

    public void StartInteraction(ScriptableObject data)
    {
        dialoguePanel.SetActive(true);

        if (data is Conversation conversation)
        {
            DisplayConversation(conversation);
        }
        else if (data is Decision decision)
        {
            DisplayDecision(decision);
        }
    }

    void DisplayConversation(Conversation convo)
    {
        choicesContainer.SetActive(false);
        speakerText.text = convo.lines[0].speakerName;

        sentences.Clear();

        foreach (DialogueLine line in convo.lines)
        {
            sentences.Enqueue(line.sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndInteraction();
            return;
        }

        string sentence = sentences.Dequeue();
        sentenceText.text = sentence;
    }

    void DisplayDecision(Decision dec)
    {
        speakerText.text = dec.introductoryLines[0].speakerName;
        sentenceText.text = dec.introductoryLines[0].sentence;

        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        currentChoiceButtons.Clear();
        choicesContainer.SetActive(true);
        
        // Start cooldown when displaying choices to prevent immediate selection
        choiceInputCooldown = CHOICE_INPUT_COOLDOWN_TIME;
        
        for (int i = 0; i < dec.choices.Count; i++)
        {
            Choice choice = dec.choices[i]; 

            GameObject buttonGO = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();

            // Use InputIconMapper for dynamic button icons
            string keyHint = GetChoiceButtonHint(i);

            buttonText.text = keyHint + choice.choiceText;
            buttonText.color = Color.black;  

            button.onClick.AddListener(() => SelectChoice(choice));
            currentChoiceButtons.Add(button);
        }
    }

    /// <summary>
    /// Gets the button hint for a choice based on current input device.
    /// Uses InputIconMapper to show keyboard/controller specific buttons.
    /// </summary>
    private string GetChoiceButtonHint(int choiceIndex)
    {
        if (InputIconMapper.Instance == null)
        {
            // Fallback if InputIconMapper not available
            if (choiceIndex == 0) return "[Q/X] ";
            else if (choiceIndex == 1) return "[R/B] ";
            return "";
        }

        // Get device-specific button icon/text
        if (choiceIndex == 0)
        {
            // Choice 1: Q on keyboard, X on Xbox controller
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Choice1);
            return iconText + " ";
        }
        else if (choiceIndex == 1)
        {
            // Choice 2: R on keyboard, B on Xbox controller
            string iconText = InputIconMapper.Instance.GetSpriteOrText(InputAction.Choice2);
            return iconText + " ";
        }

        return "";
    }

    /// <summary>
    /// Maneja la selección de una opción por parte del jugador.
    /// ACTUALIZADO para ejecutar el callback ANTES de cerrar la interacción.
    /// </summary>
    void SelectChoice(Choice choice)
    {
        Debug.Log($"<color=yellow>SelectChoice llamado para: {choice.choiceText}</color>");
        Debug.Log($"<color=yellow>Choice ID: {choice.choiceId}</color>");
        Debug.Log($"<color=yellow>Karma Effect: {choice.karmaEffect}</color>");
        Debug.Log($"<color=yellow>End Interaction: {choice.endInteractionOnSelect}</color>");

        // Aplicar efecto de karma
        if (choice.karmaEffect != 0)
        {
            Debug.Log($"<color=cyan>[InteractionManager]</color> 📊 Aplicando karma: {(choice.karmaEffect > 0 ? "+" : "")}{choice.karmaEffect}");
            KarmaManager.Instance.AddKarma(choice.karmaEffect);
        }
        else
        {
            Debug.Log($"<color=gray>[InteractionManager]</color> Esta elección no tiene efecto de karma configurado");
        }

        // PRIMERO ejecutar el callback a través del ChoiceEventSystem
        choice.Execute();

        // DESPUÉS terminar la interacción con un pequeño delay
        // para que el callback tenga tiempo de ejecutarse
        if (choice.endInteractionOnSelect)
        {
            Invoke("EndInteraction", 0.1f);
        }
    }
    
    /// <summary>
/// Muestra un texto simple sin necesidad de crear un ScriptableObject
/// </summary>
    public void ShowSimpleText(string speakerName, string text)
    {
        Conversation tempConvo = ScriptableObject.CreateInstance<Conversation>();
        tempConvo.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName = speakerName,
                sentence = text
            }
        };

        choiceInputCooldown = CHOICE_INPUT_COOLDOWN_TIME;

        StartInteraction(tempConvo);
        Debug.Log($"✅ ShowSimpleText llamado: {speakerName} - {text.Substring(0, Mathf.Min(50, text.Length))}...");
    }

    public void EndInteraction()
    {
        Debug.Log("<color=yellow>Llamando a EndInteraction...</color>");
        dialoguePanel.SetActive(false);
        currentChoiceButtons.Clear();
        Debug.Log("Interacción finalizada.");
    }

    void Update()
    {
        // Update cooldown timer
        if (choiceInputCooldown > 0f)
        {
            choiceInputCooldown -= Time.deltaTime;
        }
        
        if (dialoguePanel.activeSelf && currentChoiceButtons.Count == 0)
        {
            // Solo procesar input si el cooldown ha terminado
            if (choiceInputCooldown <= 0f)
            {
                bool interactPressed = Input.GetKeyDown(KeyCode.E);
                
                if (interactPressed)
                {
                    Debug.Log("<color=cyan>🔑 E presionada - Cerrando diálogo</color>");
                    EndInteraction();
                }
            }
        }
        if (currentChoiceButtons.Count > 0)
        {
            // Don't process choice input if we're on cooldown
            if (choiceInputCooldown > 0f)
            {
                return;
            }
            
        
            
            // Check Choice 1 input using InputActionReference if available, fallback to legacy input
            bool choice1Pressed = false;
            if (choice1Action != null && choice1Action.action != null)
            {
                choice1Pressed = choice1Action.action.WasPressedThisFrame();
            }
            else
            {
                // Fallback to legacy input
                choice1Pressed = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton2);
            }
            
            if (choice1Pressed && currentChoiceButtons.Count > 0)
            {
                Debug.Log("<color=cyan>Opción 1 seleccionada</color>");
                currentChoiceButtons[0].onClick.Invoke(); 
            }
            
            // Check Choice 2 input using InputActionReference if available, fallback to legacy input
            bool choice2Pressed = false;
            if (choice2Action != null && choice2Action.action != null)
            {
                choice2Pressed = choice2Action.action.WasPressedThisFrame();
            }
            else
            {
                // Fallback to legacy input
                choice2Pressed = Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton1);
            }
            
            if (choice2Pressed && currentChoiceButtons.Count > 1)
            {
                Debug.Log("<color=magenta>Opción 2 seleccionada</color>");
                currentChoiceButtons[1].onClick.Invoke(); 
            }
        }
    }
}