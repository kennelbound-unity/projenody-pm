using System.IO;
using UnityEngine;
using UnityEditor;

namespace Projenody
{
    public class ConfigEditorWindow : EditorWindow
    {
        private Config config;
        private string _path;

        void OnEnable()
        {
            _path = Utilities.GetProjenodyConfigPath();
            if (File.Exists(_path))
            {
                config = Utilities.GetObject<Config>(_path);
            }
            else
            {
                config = new Config();
            }
        }

        void OnGUI()
        {
            titleContent = new GUIContent("Projenody Config");

            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Path", _path);
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
            Utilities.WriteObject<Config>(_path, config);
        }

        [MenuItem("Projenody/Configure Paths")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ConfigEditorWindow));
        }
    }
}