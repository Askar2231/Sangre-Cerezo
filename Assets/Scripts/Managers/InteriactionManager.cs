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

            string keyHint = ""; 
            if (i == 0) keyHint = "[Q] "; 
            else if (i == 1) keyHint = "[R] "; 

            buttonText.text = keyHint + choice.choiceText;
            buttonText.color = Color.black;  

            button.onClick.AddListener(() => SelectChoice(choice));
            currentChoiceButtons.Add(button);
        }
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
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (currentChoiceButtons.Count > 0)
                {
                    currentChoiceButtons[0].onClick.Invoke(); 
                }
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                if (currentChoiceButtons.Count > 1)
                {
                    currentChoiceButtons[1].onClick.Invoke(); 
                }
            }
        }
    }
}