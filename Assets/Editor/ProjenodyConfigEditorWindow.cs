using System.IO;
using UnityEngine;
using UnityEditor;

public class ProjenodyConfigEditorWindow : ProjenodyCustomEditorWindow
{
    private ProjenodyConfig config;
    private string path;

    void OnEnable()
    {
        ProjenodyUtilities.GetProjenodyConfigPath();
        if (File.Exists(path))
        {
            config = ProjenodyUtilities.GetObject<ProjenodyConfig>(path);
        }
        else
        {
            config = new ProjenodyConfig();
        }
    }

    void OnGUI()
    {
        titleContent = new GUIContent("Projenody Config");

        GUILayout.Label("Basic Information", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("Path", path);
        EditorGUI.EndDisabledGroup();

        config.npmPath = EditorGUILayout.TextField("NPM Path", config.npmPath);
        config.nodePath = EditorGUILayout.TextField("Node Path", config.nodePath);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Save")))
        {
            Save();
        }
    }

    void Save()
    {
        ProjenodyUtilities.WriteObject<ProjenodyConfig>(path, config);
    }

    [MenuItem("Projenody/Progenody Config")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ProjenodyConfigEditorWindow));
    }
}