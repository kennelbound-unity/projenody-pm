using System.Collections.Generic;

namespace Projenody
{
    [System.Serializable]
    public class NPMPackage
    {
        public string name;
        public string description;
        public string version = "1.0.0";
        public string homepage;
        public string author;

        [System.NonSerialized] public List<PackageDependency> deps = new List<PackageDependency>();

        [System.NonSerialized] public List<PackageDependency> devdeps =
            new List<PackageDependency>();

        public Dictionary<string, string> dependencies = new Dictionary<string, string>();
        public Dictionary<string, string> devDependencies = new Dictionary<string, string>();
    }
}