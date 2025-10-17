using UnityEngine;
using TMPro; 
using System.Collections.Generic;
using UnityEngine.UI;


public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI sentenceText;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;

    private Queue<string> sentences;
    private List<Button> currentChoiceButtons;

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
        Debug.Log($"<color=yellow>End Interaction: {choice.endInteractionOnSelect}</color>");
        
        // Aplicar efecto de karma
        if (choice.karmaEffect != 0)
        {
            KarmaManager.Instance.AddKarma(choice.karmaEffect);
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

    public void EndInteraction()
    {
        Debug.Log("<color=yellow>Llamando a EndInteraction...</color>");
        dialoguePanel.SetActive(false);
        currentChoiceButtons.Clear();
        Debug.Log("Interacción finalizada.");
    }

    void Update()
    {
        if (currentChoiceButtons.Count > 0)
        {
            // Opción 1 - Tecla Q o Botón X del mando
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                if (currentChoiceButtons.Count > 0)
                {
                    Debug.Log("<color=cyan>Opción 1 seleccionada (Q o X mando)</color>");
                    currentChoiceButtons[0].onClick.Invoke(); 
                }
            }
            
            // Opción 2 - Tecla R o Botón B del mando
            else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                if (currentChoiceButtons.Count > 1)
                {
                    Debug.Log("<color=magenta>Opción 2 seleccionada (R o B mando)</color>");
                    currentChoiceButtons[1].onClick.Invoke(); 
                }
            }
        }
    }
}