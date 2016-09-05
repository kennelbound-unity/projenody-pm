using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjenodyPackageDependency))]
[CanEditMultipleObjects]
public class ProjenodyPackageDependencyEditor : Editor
{
    SerializedProperty module;
    SerializedProperty dev;

    void OnEnable()
    {
        // Setup the SerializedProperties.
        module = serializedObject.FindProperty("ModuleName");
        dev = serializedObject.FindProperty("DevDependency");
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(module, new GUIContent("Module"));
        EditorGUILayout.PropertyField(dev, new GUIContent("Is Dev Dependency"));
        EditorGUILayout.EndHorizontal();

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }
}