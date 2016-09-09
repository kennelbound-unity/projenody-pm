using UnityEngine;

namespace Projenody
{
    [System.Serializable]
    public class Config
    {
        public string npmPath;
        public string nodePath;

        private static string _path;
    }
}