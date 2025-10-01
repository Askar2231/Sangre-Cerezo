#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor personalizado para TutorialData ScriptableObject
/// Proporciona mejor organización y validación en el Inspector
/// </summary>
[CustomEditor(typeof(TutorialData))]
public class TutorialDataEditor : Editor
{
    private SerializedProperty tutorialId;
    private SerializedProperty tutorialType;
    private SerializedProperty showOnlyOnce;
    private SerializedProperty pauseGame;
    
    private SerializedProperty dialogText;
    private SerializedProperty dialogPosition;
    private SerializedProperty displayDuration;
    
    private SerializedProperty cards;

    private GUIStyle headerStyle;
    private GUIStyle warningStyle;
    private bool showPreview = false;

    private void OnEnable()
    {
        // Cargar propiedades serializadas
        tutorialId = serializedObject.FindProperty("tutorialId");
        tutorialType = serializedObject.FindProperty("tutorialType");
        showOnlyOnce = serializedObject.FindProperty("showOnlyOnce");
        pauseGame = serializedObject.FindProperty("pauseGame");
        
        dialogText = serializedObject.FindProperty("dialogText");
        dialogPosition = serializedObject.FindProperty("dialogPosition");
        displayDuration = serializedObject.FindProperty("displayDuration");
        
        cards = serializedObject.FindProperty("cards");
    }

    public override void OnInspectorGUI()
    {
        // Inicializar estilos
        InitializeStyles();

        serializedObject.Update();

        TutorialData tutorialData = (TutorialData)target;

        // === HEADER ===
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("CONFIGURACIÓN DE TUTORIAL", headerStyle);
        EditorGUILayout.Space(5);

        // === IDENTIFICACIÓN ===
        DrawSection("Identificación", () =>
        {
            EditorGUILayout.PropertyField(tutorialId, new GUIContent("Tutorial ID", "ID único del tutorial (ej: 'movement_basic')"));
            
            if (string.IsNullOrEmpty(tutorialId.stringValue))
            {
                EditorGUILayout.HelpBox("⚠️ El Tutorial ID es obligatorio!", MessageType.Warning);
            }
        });

        EditorGUILayout.Space(5);

        // === CONFIGURACIÓN GENERAL ===
        DrawSection("Configuración General", () =>
        {
            EditorGUILayout.PropertyField(tutorialType, new GUIContent("Tipo de Tutorial"));
            EditorGUILayout.PropertyField(showOnlyOnce, new GUIContent("Mostrar Solo Una Vez"));
            EditorGUILayout.PropertyField(pauseGame, new GUIContent("Pausar Juego"));

            // Recomendaciones según el tipo
            if (tutorialData.tutorialType == TutorialType.SimpleDialog && tutorialData.pauseGame)
            {
                EditorGUILayout.HelpBox("ℹ️ Los diálogos simples generalmente no pausan el juego (para tutoriales de movimiento).", MessageType.Info);
            }

            if (tutorialData.tutorialType == TutorialType.CardPopover && !tutorialData.pauseGame)
            {
                EditorGUILayout.HelpBox("ℹ️ Las tarjetas emergentes generalmente pausan el juego.", MessageType.Info);
            }
        });

        EditorGUILayout.Space(5);

        // === CONFIGURACIÓN ESPECÍFICA POR TIPO ===
        if (tutorialData.tutorialType == TutorialType.SimpleDialog)
        {
            DrawSimpleDialogConfig(tutorialData);
        }
        else if (tutorialData.tutorialType == TutorialType.CardPopover)
        {
            DrawCardPopoverConfig(tutorialData);
        }

        EditorGUILayout.Space(10);

        // === PREVIEW ===
        showPreview = EditorGUILayout.Foldout(showPreview, "Vista Previa de Texto", true);
        if (showPreview)
        {
            DrawTextPreview(tutorialData);
        }

        EditorGUILayout.Space(5);

        // === ACCIONES RÁPIDAS ===
        DrawQuickActions(tutorialData);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSimpleDialogConfig(TutorialData tutorialData)
    {
        DrawSection("Configuración de Diálogo Simple", () =>
        {
            EditorGUILayout.PropertyField(dialogText, new GUIContent("Texto del Diálogo"));
            
            if (string.IsNullOrEmpty(dialogText.stringValue))
            {
                EditorGUILayout.HelpBox("⚠️ El texto del diálogo está vacío!", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(dialogPosition, new GUIContent("Posición en Pantalla"));
            EditorGUILayout.PropertyField(displayDuration, new GUIContent("Duración (segundos)", "0 = cerrar manualmente"));

            if (displayDuration.floatValue == 0)
            {
                EditorGUILayout.HelpBox("ℹ️ Duración = 0: El jugador debe presionar Confirmar para cerrar.", MessageType.Info);
            }

            // Contar placeholders
            int placeholderCount = CountPlaceholders(dialogText.stringValue);
            if (placeholderCount > 0)
            {
                EditorGUILayout.HelpBox($"✓ Encontrados {placeholderCount} placeholder(s) de input", MessageType.Info);
            }
        });
    }

    private void DrawCardPopoverConfig(TutorialData tutorialData)
    {
        DrawSection("Configuración de Tarjetas", () =>
        {
            EditorGUILayout.PropertyField(cards, new GUIContent("Tarjetas"), true);

            if (cards.arraySize == 0)
            {
                EditorGUILayout.HelpBox("⚠️ No hay tarjetas configuradas!", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ {cards.arraySize} tarjeta(s) configurada(s)", MessageType.Info);

                // Validar cada tarjeta
                for (int i = 0; i < cards.arraySize; i++)
                {
                    SerializedProperty card = cards.GetArrayElementAtIndex(i);
                    SerializedProperty cardText = card.FindPropertyRelative("cardText");
                    
                    if (string.IsNullOrEmpty(cardText.stringValue))
                    {
                        EditorGUILayout.HelpBox($"⚠️ La tarjeta {i + 1} no tiene texto", MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.Space(5);

            // Botones de utilidad
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Agregar Tarjeta"))
            {
                cards.InsertArrayElementAtIndex(cards.arraySize);
            }
            if (cards.arraySize > 0 && GUILayout.Button("- Eliminar Última"))
            {
                cards.DeleteArrayElementAtIndex(cards.arraySize - 1);
            }
            EditorGUILayout.EndHorizontal();
        });
    }

    private void DrawTextPreview(TutorialData tutorialData)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Texto Procesado (ejemplo):", EditorStyles.boldLabel);
        
        if (tutorialData.tutorialType == TutorialType.SimpleDialog)
        {
            string preview = ProcessPlaceholdersForPreview(tutorialData.dialogText);
            EditorGUILayout.LabelField(preview, EditorStyles.wordWrappedLabel);
        }
        else if (tutorialData.tutorialType == TutorialType.CardPopover)
        {
            for (int i = 0; i < tutorialData.cards.Count; i++)
            {
                EditorGUILayout.LabelField($"Tarjeta {i + 1}:", EditorStyles.boldLabel);
                string preview = ProcessPlaceholdersForPreview(tutorialData.cards[i].cardText);
                EditorGUILayout.LabelField(preview, EditorStyles.wordWrappedLabel);
                
                if (i < tutorialData.cards.Count - 1)
                {
                    EditorGUILayout.Space(5);
                    DrawSeparator();
                }
            }
        }
        
        EditorGUILayout.EndVertical();
    }

    private void DrawQuickActions(TutorialData tutorialData)
    {
        DrawSection("Acciones Rápidas", () =>
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("🧪 Probar Tutorial"))
            {
                TestTutorial(tutorialData);
            }

            if (GUILayout.Button("📋 Copiar ID"))
            {
                EditorGUIUtility.systemCopyBuffer = tutorialData.tutorialId;
                Debug.Log($"ID copiado: {tutorialData.tutorialId}");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (Application.isPlaying)
            {
                if (GUILayout.Button("▶️ Disparar Tutorial Ahora (Play Mode)"))
                {
                    if (TutorialManager.Instance != null)
                    {
                        TutorialManager.Instance.TriggerTutorial(tutorialData);
                        Debug.Log($"Tutorial disparado: {tutorialData.tutorialId}");
                    }
                    else
                    {
                        Debug.LogError("TutorialManager no encontrado en la escena!");
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("▶️ Entra en Play Mode para probar el tutorial en tiempo real", MessageType.Info);
            }
        });
    }

    private void TestTutorial(TutorialData tutorialData)
    {
        Debug.Log("=== TEST DE TUTORIAL ===");
        Debug.Log($"ID: {tutorialData.tutorialId}");
        Debug.Log($"Tipo: {tutorialData.tutorialType}");
        Debug.Log($"Pausar: {tutorialData.pauseGame}");
        
        if (tutorialData.tutorialType == TutorialType.SimpleDialog)
        {
            Debug.Log($"Texto: {tutorialData.dialogText}");
            Debug.Log($"Duración: {tutorialData.displayDuration}s");
        }
        else
        {
            Debug.Log($"Tarjetas: {tutorialData.cards.Count}");
            for (int i = 0; i < tutorialData.cards.Count; i++)
            {
                Debug.Log($"  Tarjeta {i + 1}: {tutorialData.cards[i].cardText.Substring(0, Mathf.Min(50, tutorialData.cards[i].cardText.Length))}...");
            }
        }
        
        Debug.Log("======================");
    }

    private void DrawSection(string title, System.Action content)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(3);
        content?.Invoke();
        EditorGUILayout.EndVertical();
    }

    private void DrawSeparator()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    private void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            headerStyle.alignment = TextAnchor.MiddleCenter;
        }

        if (warningStyle == null)
        {
            warningStyle = new GUIStyle(EditorStyles.helpBox);
            warningStyle.normal.textColor = Color.yellow;
        }
    }

    private int CountPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int count = 0;
        string[] placeholders = { "{Move}", "{Run}", "{Interact}", "{QTE}", "{Parry}", "{Confirm}", "{Cancel}", "{Attack}", "{Skill}", "{EndTurn}" };
        
        foreach (string placeholder in placeholders)
        {
            int index = 0;
            while ((index = text.IndexOf(placeholder, index)) != -1)
            {
                count++;
                index += placeholder.Length;
            }
        }

        return count;
    }

    private string ProcessPlaceholdersForPreview(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";

        string processed = text;
        processed = processed.Replace("{Move}", "[WASD/Stick Izq]");
        processed = processed.Replace("{Run}", "[Shift/LT]");
        processed = processed.Replace("{Interact}", "[E/A]");
        processed = processed.Replace("{QTE}", "[Espacio/A]");
        processed = processed.Replace("{Parry}", "[Espacio/A]");
        processed = processed.Replace("{Confirm}", "[Enter/A]");
        processed = processed.Replace("{Cancel}", "[ESC/B]");
        processed = processed.Replace("{Attack}", "[Clic Izq/X]");
        processed = processed.Replace("{Skill}", "[Clic Der/Y]");
        processed = processed.Replace("{EndTurn}", "[Tab/RB]");

        return processed;
    }
}
#endif

