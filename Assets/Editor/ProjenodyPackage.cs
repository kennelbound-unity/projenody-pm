using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjenodyPackage
{
    public string name;
    public string assetsFolder = "Assets";
    public string targetFolder;
    public string projectSettingsFolder;
    public bool applicationPackage = false;
    public bool pluginPackage = false;
    public List<string> pluginOverrides;
}