using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(InputIconMapper))]
public class InputIconMapperGUI : Editor
{
    public override void OnInspectorGUI()
    {
        InputIconMapper mapper = (InputIconMapper)target;
        
        // Draw default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // Quick actions section
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Initialize Default Mappings", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Initialize Default Mappings", 
                "This will reset all mappings to default values. Continue?", "Yes", "Cancel"))
            {
                // Use reflection to call the private InitializeDefaultMappings method
                var method = typeof(InputIconMapper).GetMethod("InitializeDefaultMappings", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(mapper, null);
                
                EditorUtility.SetDirty(mapper);
                EditorUtility.DisplayDialog("Success", "Default mappings initialized!", "OK");
            }
        }
        
        if (GUILayout.Button("Clear All Mappings", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Mappings", 
                "Are you sure you want to clear all sprite name mappings?", "Yes", "Cancel"))
            {
                SerializedObject so = new SerializedObject(mapper);
                SerializedProperty mappingsProp = so.FindProperty("spriteNameMappings");
                mappingsProp.ClearArray();
                so.ApplyModifiedProperties();
                
                EditorUtility.DisplayDialog("Success", "All mappings cleared!", "OK");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Validation info
        SerializedObject serializedObj = new SerializedObject(mapper);
        SerializedProperty mappings = serializedObj.FindProperty("spriteNameMappings");
        
        if (mappings.arraySize == 0)
        {
            EditorGUILayout.HelpBox(
                "No mappings defined! Click 'Initialize Default Mappings' to create the standard input action mappings.",
                MessageType.Warning
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                $"Total mappings: {mappings.arraySize}\n" +
                "Sprite names should match exactly with sprites in your TMP Sprite Asset.",
                MessageType.Info
            );
        }
    }
}
#endif
