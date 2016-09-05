using System.Collections.Generic;

[System.Serializable]
public class ProjenodyNPMPackage
{
    public string name;
    public string description;
    public string version = "1.0.0";
    public string homepage;
    public string author;

    [System.NonSerialized] public List<ProjenodyPackageDependency> deps = new List<ProjenodyPackageDependency>();

    [System.NonSerialized] public List<ProjenodyPackageDependency> devdeps =
        new List<ProjenodyPackageDependency>();

    public Dictionary<string, string> dependencies = new Dictionary<string, string>();
    public Dictionary<string, string> devDependencies = new Dictionary<string, string>();
}