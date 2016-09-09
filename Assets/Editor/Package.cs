using System.Collections.Generic;

namespace Projenody
{
    [System.Serializable]
    public class Package
    {
        public string name;
        public string assetsFolder = "Assets";
        public string targetFolder;
        public string projectSettingsFolder;
        public bool applicationPackage = false;
        public bool pluginPackage = false;
        public List<string> pluginOverrides;
    }
}