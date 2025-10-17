using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Custom Editor for InputIconMapper that displays sprite name mappings
/// </summary>
[CustomEditor(typeof(InputIconMapper))]
public class InputIconMapperEditor : Editor
{
    private InputIconMapper mapper;
    private SerializedProperty debugModeProperty;
    private SerializedProperty spriteNameMappingsProperty;

    private void OnEnable()
    {
        mapper = (InputIconMapper)target;
        
        // Get all serialized properties
        debugModeProperty = serializedObject.FindProperty("debugMode");
        spriteNameMappingsProperty = serializedObject.FindProperty("spriteNameMappings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Header
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Input Icon Mapper", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Map input actions to sprite names for different device types. These sprite names should match the names in your TMP Sprite Asset.",
            MessageType.Info
        );
        EditorGUILayout.Space(10);
        
        // Configuration
        EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(debugModeProperty, new GUIContent("Debug Mode", "Enable console logging for debugging"));
        
        EditorGUILayout.Space(10);
        
        // Sprite Name Mappings
        EditorGUILayout.LabelField("Sprite Name Mappings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Define sprite names for each input action. The sprite names must match exactly with sprites in your TMP Sprite Asset.\n\n" +
            "Example sprite names:\n" +
            "• Keyboard: 'WASD', 'Shift', 'Space', 'E', 'Mouse_Left'\n" +
            "• Xbox: 'Xbox_A', 'Xbox_B', 'Xbox_X', 'Xbox_Y', 'Xbox_LT'\n" +
            "• PlayStation: 'PS_Cross', 'PS_Circle', 'PS_Square', 'PS_Triangle', 'PS_L2'",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        // Draw the list with proper array controls
        EditorGUILayout.PropertyField(spriteNameMappingsProperty, new GUIContent("Action Sprite Maps"), true);
        
        EditorGUILayout.Space(10);
        
        // Quick actions
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Initialize Default Mappings"))
        {
            if (EditorUtility.DisplayDialog("Initialize Default Mappings", 
                "This will reset all mappings to default values. Continue?", "Yes", "Cancel"))
            {
                InitializeDefaultMappings();
            }
        }
        
        if (GUILayout.Button("Clear All Mappings"))
        {
            if (EditorUtility.DisplayDialog("Clear All Mappings", 
                "Are you sure you want to clear all sprite name mappings?", "Yes", "Cancel"))
            {
                ClearAllMappings();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Validation
        if (spriteNameMappingsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox(
                "No mappings defined! Click 'Initialize Default Mappings' to create the standard input action mappings.",
                MessageType.Warning
            );
        }
        else
        {
            int emptyMappings = CountEmptyMappings();
            if (emptyMappings > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{emptyMappings} mapping(s) have empty sprite names. Fill them in or they will use text fallbacks.",
                    MessageType.Warning
                );
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private int CountEmptyMappings()
    {
        int count = 0;
        
        for (int i = 0; i < spriteNameMappingsProperty.arraySize; i++)
        {
            SerializedProperty element = spriteNameMappingsProperty.GetArrayElementAtIndex(i);
            SerializedProperty keyboardName = element.FindPropertyRelative("keyboardSpriteName");
            SerializedProperty xboxName = element.FindPropertyRelative("xboxSpriteName");
            SerializedProperty psName = element.FindPropertyRelative("playStationSpriteName");
            
            bool allEmpty = string.IsNullOrEmpty(keyboardName.stringValue) &&
                           string.IsNullOrEmpty(xboxName.stringValue) &&
                           string.IsNullOrEmpty(psName.stringValue);
            
            if (allEmpty)
            {
                count++;
            }
        }
        
        return count;
    }
    
    private void InitializeDefaultMappings()
    {
        // Clear existing
        spriteNameMappingsProperty.ClearArray();
        
        // Add default mappings (matching the defaults in InputIconMapper.cs)
        AddMapping(InputAction.Move, "WASD", "Xbox_LS", "PS_LS");
        AddMapping(InputAction.Run, "Shift", "Xbox_R3", "PS_R3");
        AddMapping(InputAction.Interact, "E", "Xbox_A", "PS_Cross");
        AddMapping(InputAction.QTE, "Space", "Xbox_A", "PS_Cross");
        AddMapping(InputAction.Parry, "Space", "Xbox_A", "PS_Cross");
        AddMapping(InputAction.Confirm, "Enter", "Xbox_A", "PS_Cross");
        AddMapping(InputAction.Cancel, "ESC", "Xbox_B", "PS_Circle");
        AddMapping(InputAction.LightAttack, "Mouse_Left", "Xbox_X", "PS_Square");
        AddMapping(InputAction.HeavyAttack, "Mouse_Right", "Xbox_Y", "PS_Triangle");
        AddMapping(InputAction.Skill1, "Q", "Xbox_LT", "PS_L2");
        AddMapping(InputAction.Skill2, "R", "Xbox_RT", "PS_R2");
        AddMapping(InputAction.EndTurn, "Tab", "Xbox_RB", "PS_R1");
        
        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Initialized", "Default sprite name mappings have been created!", "OK");
    }
    
    private void AddMapping(InputAction action, string keyboardName, string xboxName, string psName)
    {
        int index = spriteNameMappingsProperty.arraySize;
        spriteNameMappingsProperty.InsertArrayElementAtIndex(index);
        
        SerializedProperty element = spriteNameMappingsProperty.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("action").enumValueIndex = (int)action;
        element.FindPropertyRelative("keyboardSpriteName").stringValue = keyboardName;
        element.FindPropertyRelative("xboxSpriteName").stringValue = xboxName;
        element.FindPropertyRelative("playStationSpriteName").stringValue = psName;
    }
    
    private void ClearAllMappings()
    {
        spriteNameMappingsProperty.ClearArray();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Cleared", "All sprite name mappings have been cleared.", "OK");
    }
}
