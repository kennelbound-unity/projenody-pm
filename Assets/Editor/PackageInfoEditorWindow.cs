using System;
using System.Collections.Generic;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Projenody
{
    // TODO: Add configuration for telling projenody plugin where npm is installed
    public class PackageInfoEditorWindow : EditorWindow
    {
        private Package _pkg;
        private NPMPackage _npmPackage;
        private ReorderableList _dependencyList;
        private ReorderableList _devDependencyList;
        private ReorderableList _pluginOverridesList;

        private string _npmPackageJsonPath;
        private string _projenodyJsonPath;

        protected void OnEnable()
        {
            // Setup the paths
            _projenodyJsonPath = Utilities.GetFilePath(Application.dataPath + "/../", "projenody.json");
            if (_projenodyJsonPath == null)
            {
                // TODO: Load from the filesystem, may need to monitor filesystem as well
                _pkg = new Package();
                Debug.Log("No projenody package detected.  Creating new one.");
            }
            else
            {
                _pkg = Utilities.GetObject<Package>(_projenodyJsonPath);
            }

            _npmPackageJsonPath = Utilities.GetFilePath(Application.dataPath + "/../", "package.json");
            if (_npmPackageJsonPath == null)
            {
                _npmPackage = new NPMPackage();
            }
            else
            {
                _npmPackage = Utilities.GetObject<NPMPackage>(_npmPackageJsonPath);
                foreach (KeyValuePair<string, string> kvp in _npmPackage.dependencies)
                {
                    _npmPackage.deps.Add(new PackageDependency()
                    {
                        Name = kvp.Key,
                        SrcPath = kvp.Value
                    });
                }
                foreach (KeyValuePair<string, string> kvp in _npmPackage.devDependencies)
                {
                    _npmPackage.devdeps.Add(new PackageDependency()
                    {
                        Name = kvp.Key,
                        SrcPath = kvp.Value
                    });
                }
            }

            setupDependencyList(out _dependencyList, _npmPackage.deps);
            setupDependencyList(out _devDependencyList, _npmPackage.devdeps);
        }

        void setupDependencyList(out ReorderableList list, List<PackageDependency> src)
        {
            ReorderableList rlist = new ReorderableList(src, typeof(string), true, true, true, true);
            rlist.drawElementCallback +=
                delegate(Rect rect, int i, bool b, bool focused) { DrawElement(rect, i, b, focused, src); };
            rlist.onAddCallback += delegate(ReorderableList reorderableList) { AddItem(reorderableList, src); };
            rlist.onRemoveCallback += delegate(ReorderableList reorderableList) { RemoveItem(reorderableList, src); };

            list = rlist;
        }

        void OnGUI()
        {
            titleContent = new GUIContent("Package Info");

            GUILayout.Label("Basic Information", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Path", Utilities.NormalizedPath(_projenodyJsonPath));
            EditorGUI.EndDisabledGroup();

            _pkg.name = EditorGUILayout.TextField("Package name", _pkg.name);
            _npmPackage.description = EditorGUILayout.TextField("Description", _npmPackage.description);
            _npmPackage.homepage = EditorGUILayout.TextField("Homepage (Optional)", _npmPackage.homepage);
            _npmPackage.version = EditorGUILayout.TextField("Version", _npmPackage.version);
            _npmPackage.author = EditorGUILayout.TextField("Author", _npmPackage.author);
            GUILayout.Space(10f);

            GUILayout.Label("Folders (relative)", EditorStyles.boldLabel);
            _pkg.assetsFolder = EditorGUILayout.TextField("Assets", _pkg.assetsFolder);
            _pkg.targetFolder = EditorGUILayout.TextField("Target", _pkg.targetFolder);

            GUILayout.Space(10f);

            _pkg.applicationPackage = EditorGUILayout.BeginToggleGroup("Application Package", _pkg.applicationPackage);
            _pkg.projectSettingsFolder = EditorGUILayout.TextField("Project Settings Folder", _pkg.projectSettingsFolder);
            EditorGUILayout.EndToggleGroup();
            _pkg.pluginPackage = EditorGUILayout.Toggle("Plugin Package", _pkg.pluginPackage);

            GUILayout.Space(10f);
            GUILayout.Label("Dependencies", EditorStyles.boldLabel);
            _dependencyList.DoLayoutList();

            GUILayout.Space(10f);
            GUILayout.Label("Dev Dependencies", EditorStyles.boldLabel);
            _devDependencyList.DoLayoutList();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Save")))
            {
                Save();
            }
        }

        [MenuItem("Projenody/Configure Package")]
        static void ShowWindow()
        {
            UnityEditor.EditorWindow.GetWindow<PackageInfoEditorWindow>();
        }

        private void Save()
        {
            _npmPackage.name = _pkg.name;
            _npmPackage.dependencies.Clear();
            foreach (PackageDependency dep in _npmPackage.deps)
            {
                _npmPackage.dependencies[dep.Name] = dep.SrcPath;
            }

            _npmPackage.devDependencies.Clear();
            foreach (PackageDependency dep in _npmPackage.devdeps)
            {
                _npmPackage.devDependencies[dep.Name] = dep.SrcPath;
            }

            Utilities.WriteObject<Package>(_projenodyJsonPath, _pkg);
            Utilities.WriteObject<NPMPackage>(_npmPackageJsonPath, _npmPackage);
        }

        private void OnDisable()
        {
            // Make sure we don't get memory leaks etc.
//        dependencyList.drawHeaderCallback -= DrawHeader;
//        dependencyList.drawElementCallback -= DrawElement;
//
//        dependencyList.onAddCallback -= AddItem;
//        dependencyList.onRemoveCallback -= RemoveItem;
        }

        /// <summary>
        /// Draws the header of the list
        /// </summary>
        /// <param name="rect"></param>
        private void DrawHeader(Rect rect)
        {
//        GUI.Label(rect, "Our fancy reorderable list");
        }

        private const int ListLabelWidth = 45;
        private const int NameWidth = 150;

        /// <summary>
        /// Draws one element of the list (ListItemExample)
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="active"></param>
        /// <param name="focused"></param>
        private void DrawElement(Rect rect, int index, bool active, bool focused, List<PackageDependency> list)
        {
            PackageDependency item = list[index];
            EditorGUIUtility.labelWidth = ListLabelWidth;
            item.Name = EditorGUI.TextField(new Rect(rect.x, rect.y, NameWidth, rect.height), "Name", item.Name);
            item.SrcPath = EditorGUI.TextField(
                new Rect(rect.x + NameWidth, rect.y, rect.width - NameWidth, rect.height),
                "Source", item.SrcPath);
            EditorGUIUtility.labelWidth = 0;
        }

        private void AddItem(ReorderableList rlist, List<PackageDependency> list)
        {
            list.Add(new PackageDependency());
        }

        private void RemoveItem(ReorderableList rlist, List<PackageDependency> list)
        {
            list.RemoveAt(rlist.index);
        }
    }
}