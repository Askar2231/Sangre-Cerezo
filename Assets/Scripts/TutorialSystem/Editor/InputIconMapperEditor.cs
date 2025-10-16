using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Custom Editor for InputIconMapper that auto-generates TMP Sprite Assets
/// </summary>
[CustomEditor(typeof(InputIconMapper))]
public class InputIconMapperEditor : Editor
{
    private InputIconMapper mapper;
    private SerializedProperty debugModeProperty;
    
    // Sprite properties
    private SerializedProperty[] keyboardSprites;
    private SerializedProperty[] xboxSprites;
    private SerializedProperty[] psSprites;

    private void OnEnable()
    {
        mapper = (InputIconMapper)target;
        
        // Get all serialized properties
        debugModeProperty = serializedObject.FindProperty("debugMode");
        
        // Keyboard sprites
        keyboardSprites = new SerializedProperty[]
        {
            serializedObject.FindProperty("keyboardMove"),
            serializedObject.FindProperty("keyboardRun"),
            serializedObject.FindProperty("keyboardInteract"),
            serializedObject.FindProperty("keyboardQTE"),
            serializedObject.FindProperty("keyboardParry"),
            serializedObject.FindProperty("keyboardConfirm"),
            serializedObject.FindProperty("keyboardCancel"),
            serializedObject.FindProperty("keyboardLightAttack"),
            serializedObject.FindProperty("keyboardHeavyAttack"),
            serializedObject.FindProperty("keyboardSkill1"),
            serializedObject.FindProperty("keyboardSkill2"),
            serializedObject.FindProperty("keyboardEndTurn"),
        };
        
        // Xbox sprites
        xboxSprites = new SerializedProperty[]
        {
            serializedObject.FindProperty("xboxMove"),
            serializedObject.FindProperty("xboxRun"),
            serializedObject.FindProperty("xboxInteract"),
            serializedObject.FindProperty("xboxQTE"),
            serializedObject.FindProperty("xboxParry"),
            serializedObject.FindProperty("xboxConfirm"),
            serializedObject.FindProperty("xboxCancel"),
            serializedObject.FindProperty("xboxLightAttack"),
            serializedObject.FindProperty("xboxHeavyAttack"),
            serializedObject.FindProperty("xboxSkill1"),
            serializedObject.FindProperty("xboxSkill2"),
            serializedObject.FindProperty("xboxEndTurn"),
        };
        
        // PlayStation sprites
        psSprites = new SerializedProperty[]
        {
            serializedObject.FindProperty("psMove"),
            serializedObject.FindProperty("psRun"),
            serializedObject.FindProperty("psInteract"),
            serializedObject.FindProperty("psQTE"),
            serializedObject.FindProperty("psParry"),
            serializedObject.FindProperty("psConfirm"),
            serializedObject.FindProperty("psCancel"),
            serializedObject.FindProperty("psLightAttack"),
            serializedObject.FindProperty("psHeavyAttack"),
            serializedObject.FindProperty("psSkill1"),
            serializedObject.FindProperty("psSkill2"),
            serializedObject.FindProperty("psEndTurn"),
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Header
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Input Icon Mapper", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Assign your input icon sprites below, then click 'Generate TMP Sprite Asset' to create a sprite asset for inline text icons.",
            MessageType.Info
        );
        EditorGUILayout.Space(10);
        
        // Generate Asset Button
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Generate TMP Sprite Asset", GUILayout.Height(40)))
        {
            GenerateSpriteAsset();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(5);
        
        // Validation info
        int totalAssigned = CountAssignedSprites();
        int totalSprites = keyboardSprites.Length + xboxSprites.Length + psSprites.Length;
        
        string statusMessage = $"Sprites Assigned: {totalAssigned} / {totalSprites}";
        MessageType statusType = totalAssigned == 0 ? MessageType.Warning : 
                                 totalAssigned < totalSprites / 2 ? MessageType.Warning : 
                                 MessageType.Info;
        EditorGUILayout.HelpBox(statusMessage, statusType);
        
        EditorGUILayout.Space(10);
        
        // Configuration
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(debugModeProperty, new GUIContent("Debug Mode", "Enable console logging for debugging"));
        
        EditorGUILayout.Space(10);
        
        // Sprite assignments with better organization
        DrawSpriteSection("Keyboard Sprites", keyboardSprites);
        DrawSpriteSection("Xbox Sprites", xboxSprites);
        DrawSpriteSection("PlayStation Sprites", psSprites);
        
        EditorGUILayout.Space(10);
        
        // Utility buttons
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Sprites"))
        {
            if (EditorUtility.DisplayDialog("Clear All Sprites", 
                "Are you sure you want to clear all sprite assignments?", "Yes", "Cancel"))
            {
                ClearAllSprites();
            }
        }
        
        if (GUILayout.Button("Find Missing Sprites"))
        {
            ShowMissingSprites();
        }
        EditorGUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawSpriteSection(string title, SerializedProperty[] properties)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        foreach (var prop in properties)
        {
            if (prop != null)
            {
                EditorGUILayout.PropertyField(prop);
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    
    private int CountAssignedSprites()
    {
        int count = 0;
        
        foreach (var prop in keyboardSprites.Concat(xboxSprites).Concat(psSprites))
        {
            if (prop != null && prop.objectReferenceValue != null)
            {
                count++;
            }
        }
        
        return count;
    }
    
    private void GenerateSpriteAsset()
    {
        // Collect all assigned sprites
        List<Sprite> allSprites = new List<Sprite>();
        
        // Add keyboard sprites
        foreach (var prop in keyboardSprites)
        {
            if (prop != null && prop.objectReferenceValue != null)
            {
                Sprite sprite = prop.objectReferenceValue as Sprite;
                if (sprite != null && !allSprites.Contains(sprite))
                {
                    allSprites.Add(sprite);
                }
            }
        }
        
        // Add Xbox sprites
        foreach (var prop in xboxSprites)
        {
            if (prop != null && prop.objectReferenceValue != null)
            {
                Sprite sprite = prop.objectReferenceValue as Sprite;
                if (sprite != null && !allSprites.Contains(sprite))
                {
                    allSprites.Add(sprite);
                }
            }
        }
        
        // Add PlayStation sprites
        foreach (var prop in psSprites)
        {
            if (prop != null && prop.objectReferenceValue != null)
            {
                Sprite sprite = prop.objectReferenceValue as Sprite;
                if (sprite != null && !allSprites.Contains(sprite))
                {
                    allSprites.Add(sprite);
                }
            }
        }
        
        if (allSprites.Count == 0)
        {
            EditorUtility.DisplayDialog("No Sprites Assigned", 
                "Please assign at least one sprite before generating the sprite asset.", "OK");
            return;
        }
        
        // Ask where to save
        string path = EditorUtility.SaveFilePanelInProject(
            "Save TMP Sprite Asset",
            "InputIcons_Generated",
            "asset",
            "Choose where to save the generated TMP Sprite Asset",
            "Assets/TextMesh Pro/Resources/Sprite Assets"
        );
        
        if (string.IsNullOrEmpty(path))
        {
            return; // User cancelled
        }
        
        // Create the sprite asset
        TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        
        // Add sprites to the asset
        for (int i = 0; i < allSprites.Count; i++)
        {
            Sprite sprite = allSprites[i];
            
            // Create sprite glyph
            TMP_SpriteGlyph glyph = new TMP_SpriteGlyph
            {
                index = (uint)i,
                sprite = sprite,
                metrics = new UnityEngine.TextCore.GlyphMetrics(
                    sprite.rect.width,
                    sprite.rect.height,
                    0, // bearing X
                    sprite.rect.height, // bearing Y
                    sprite.rect.width // advance
                ),
                glyphRect = new UnityEngine.TextCore.GlyphRect(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height
                ),
                scale = 1.0f,
                atlasIndex = 0
            };
            
            // Create sprite character
            TMP_SpriteCharacter character = new TMP_SpriteCharacter
            {
                unicode = 0xFFFE, // Private use area
                name = sprite.name,
                glyphIndex = (uint)i,
                scale = 1.0f
            };
            
            // Add to the asset using Add methods (properties are read-only)
            spriteAsset.spriteGlyphTable.Add(glyph);
            spriteAsset.spriteCharacterTable.Add(character);
        }
        
        // Save the asset
        AssetDatabase.CreateAsset(spriteAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the created asset
        EditorGUIUtility.PingObject(spriteAsset);
        Selection.activeObject = spriteAsset;
        
        // Show success message
        EditorUtility.DisplayDialog("Sprite Asset Generated!", 
            $"Successfully created TMP Sprite Asset with {allSprites.Count} sprites!\n\n" +
            $"Location: {path}\n\n" +
            "Next steps:\n" +
            "1. Assign this sprite asset to your TextMeshProUGUI components\n" +
            "2. Use placeholders like {Confirm} in your text\n" +
            "3. The system will replace them with sprite icons!", 
            "OK");
        
        Debug.Log($"[InputIconMapper] Generated TMP Sprite Asset with {allSprites.Count} sprites at: {path}");
    }
    
    private void ClearAllSprites()
    {
        foreach (var prop in keyboardSprites.Concat(xboxSprites).Concat(psSprites))
        {
            if (prop != null)
            {
                prop.objectReferenceValue = null;
            }
        }
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Sprites Cleared", "All sprite assignments have been cleared.", "OK");
    }
    
    private void ShowMissingSprites()
    {
        List<string> missingSprites = new List<string>();
        
        CheckMissing(keyboardSprites, "Keyboard", missingSprites);
        CheckMissing(xboxSprites, "Xbox", missingSprites);
        CheckMissing(psSprites, "PlayStation", missingSprites);
        
        if (missingSprites.Count == 0)
        {
            EditorUtility.DisplayDialog("All Sprites Assigned", 
                "Great! All sprite fields have assignments.", "OK");
        }
        else
        {
            string message = "The following sprite fields are missing assignments:\n\n";
            message += string.Join("\n", missingSprites);
            
            EditorUtility.DisplayDialog("Missing Sprites", message, "OK");
        }
    }
    
    private void CheckMissing(SerializedProperty[] properties, string deviceName, List<string> missing)
    {
        foreach (var prop in properties)
        {
            if (prop != null && prop.objectReferenceValue == null)
            {
                string fieldName = prop.displayName;
                missing.Add($"• {deviceName}: {fieldName}");
            }
        }
    }
}
