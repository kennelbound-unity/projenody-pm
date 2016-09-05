using System;
using System.Collections.Generic;
using System.IO;
using FullSerializer;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

// TODO: Add configuration for telling projenody plugin where npm is installed
public class ProjenodyPackageInfoEditorWindow : EditorWindow
{
    private static readonly fsSerializer _serializer = new fsSerializer();

    private ProjenodyPackage pkg;
    private ProjenodyNPMPackage npmPackage;
    private ReorderableList dependencyList;
    private ReorderableList devDependencyList;
    private ReorderableList pluginOverridesList;

    private string npmPackageJsonPath;
    private string projenodyJsonPath;

    private T GetObject<T>(string path)
    {
        string json = File.ReadAllText(path);
        fsData data = fsJsonParser.Parse(json);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();

        return (T) deserialized;
    }

    private void WriteObject<T>(string path, object item)
    {
        // serialize the data
        fsData data;
        _serializer.TrySerialize(typeof(T), item, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        File.WriteAllText(path, fsJsonPrinter.PrettyJson(data));
    }

    protected void OnEnable()
    {
        // Setup the paths
        projenodyJsonPath = new Uri(Application.dataPath + "/../projenody.json").LocalPath;
        npmPackageJsonPath = new Uri(Application.dataPath + "/../package.json").LocalPath;

        pkg = GetObject<ProjenodyPackage>(projenodyJsonPath);
        if (pkg == null)
        {
            // TODO: Load from the filesystem, may need to monitor filesystem as well
            pkg = new ProjenodyPackage();
            Debug.Log("No projenody package detected.  Creating new one.");
        }

        npmPackage = GetObject<ProjenodyNPMPackage>(npmPackageJsonPath);
        if (npmPackage == null)
        {
            npmPackage = new ProjenodyNPMPackage();
        }
        foreach (KeyValuePair<string, string> kvp in npmPackage.dependencies)
        {
            npmPackage.deps.Add(new ProjenodyPackageDependency()
            {
                Name = kvp.Key,
                SrcPath = kvp.Value
            });
        }
        foreach (KeyValuePair<string, string> kvp in npmPackage.devDependencies)
        {
            npmPackage.devdeps.Add(new ProjenodyPackageDependency()
            {
                Name = kvp.Key,
                SrcPath = kvp.Value
            });
        }

        setupDependencyList(out dependencyList, npmPackage.deps);
        setupDependencyList(out devDependencyList, npmPackage.devdeps);
    }

    void setupDependencyList(out ReorderableList list, List<ProjenodyPackageDependency> src)
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
        pkg.name = EditorGUILayout.TextField("Package name", pkg.name);
        npmPackage.description = EditorGUILayout.TextField("Description", npmPackage.description);
        npmPackage.homepage = EditorGUILayout.TextField("Homepage (Optional)", npmPackage.homepage);
        npmPackage.version = EditorGUILayout.TextField("Version", npmPackage.version);
        npmPackage.author = EditorGUILayout.TextField("Author", npmPackage.author);
        GUILayout.Space(10f);

        GUILayout.Label("Folders (relative)", EditorStyles.boldLabel);
        pkg.assetsFolder = EditorGUILayout.TextField("Assets", pkg.assetsFolder);
        pkg.targetFolder = EditorGUILayout.TextField("Target", pkg.targetFolder);

        GUILayout.Space(10f);

        pkg.applicationPackage = EditorGUILayout.BeginToggleGroup("Application Package", pkg.applicationPackage);
        pkg.projectSettingsFolder = EditorGUILayout.TextField("Project Settings Folder", pkg.projectSettingsFolder);
        EditorGUILayout.EndToggleGroup();
        pkg.pluginPackage = EditorGUILayout.Toggle("Plugin Package", pkg.pluginPackage);

        GUILayout.Space(10f);
        GUILayout.Label("Dependencies", EditorStyles.boldLabel);
        dependencyList.DoLayoutList();

        GUILayout.Space(10f);
        GUILayout.Label("Dev Dependencies", EditorStyles.boldLabel);
        devDependencyList.DoLayoutList();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Save")))
        {
            Save();
        }
    }

    [MenuItem("Projenody/Package Info")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ProjenodyPackageInfoEditorWindow));
    }

    private void Save()
    {
        npmPackage.name = pkg.name;
        npmPackage.dependencies.Clear();
        foreach (ProjenodyPackageDependency dep in npmPackage.deps)
        {
            npmPackage.dependencies[dep.Name] = dep.SrcPath;
        }

        npmPackage.devDependencies.Clear();
        foreach (ProjenodyPackageDependency dep in npmPackage.devdeps)
        {
            npmPackage.devDependencies[dep.Name] = dep.SrcPath;
        }

        WriteObject<ProjenodyPackage>(projenodyJsonPath, pkg);
        WriteObject<ProjenodyNPMPackage>(npmPackageJsonPath, npmPackage);
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
    private void DrawElement(Rect rect, int index, bool active, bool focused, List<ProjenodyPackageDependency> list)
    {
        ProjenodyPackageDependency item = list[index];
        EditorGUIUtility.labelWidth = ListLabelWidth;
        item.Name = EditorGUI.TextField(new Rect(rect.x, rect.y, NameWidth, rect.height), "Name", item.Name);
        item.SrcPath = EditorGUI.TextField(new Rect(rect.x + NameWidth, rect.y, rect.width - NameWidth, rect.height),
            "Source", item.SrcPath);
        EditorGUIUtility.labelWidth = 0;
//        list[index] = EditorGUI.TextField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height),
//            new GUIContent("Module"),
//            list[index]);

        // If you are using a custom PropertyDrawer, this is probably better
        // EditorGUI.PropertyField(rect, serializedObject.FindProperty("list").GetArrayElementAtIndex(index));
        // Although it is probably smart to cach the list as a private variable ;)
    }

    private void AddItem(ReorderableList rlist, List<ProjenodyPackageDependency> list)
    {
        list.Add(new ProjenodyPackageDependency());
    }

    private void RemoveItem(ReorderableList rlist, List<ProjenodyPackageDependency> list)
    {
        list.RemoveAt(rlist.index);
    }
}