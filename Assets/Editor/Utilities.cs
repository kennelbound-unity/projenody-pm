﻿using System;
using System.Diagnostics;
using System.IO;
using FullSerializer;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Projenody
{
    public class Utilities
    {
        [MenuItem("Projenody/Update Project Links")]
        public static void UpdateProjectLinks()
        {
            if (Config == null)
            {
                UnityEditor.EditorWindow.GetWindow(typeof(ConfigEditorWindow));
                return;
            }
            InstallDependencies();

            string path = GetPackageRoot();
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Failed to find the package.json file");
                return;
            }

            string output =
                RunCommand(Config.nodePath, NormalizedPath(path),
                    "node_modules" + Path.DirectorySeparatorChar + "projenody" + Path.DirectorySeparatorChar +
                    "projenody.js");
            if (string.IsNullOrEmpty(output))
            {
                Debug.Log(output);
            }
        }

        [MenuItem("Projenody/Install Dependencies")]
        public static void InstallDependencies()
        {
            if (Config == null)
            {
                EditorWindow.GetWindow(typeof(ConfigEditorWindow));
                return;
            }

            string path = GetPackageRoot();
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Failed to find the package.json file");
                return;
            }

            RunCommand(Config.npmPath, path, "install");
            RunCommand(Config.npmPath, path, "prune");
        }

        public static string GetPackageRoot()
        {
            string path = GetFilePath(Application.dataPath + "/../", "package.json");
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("Failed to find the package.json file");
                return null;
            }

            return Path.GetDirectoryName(path);
        }

        public static string RunCommand(string executable, string directory, string args)
        {
            Process p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = executable,
                    WorkingDirectory = directory,
                    Arguments = args
                }
            };
            // Redirect the output stream of the child process.
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            if (p.ExitCode != 0)
            {
                string err = "Error running cmd: " + executable + " (in " + directory + ") and args " + args +
                             "\nOutput: " + output;
                Debug.LogError(err);
                throw new Exception(err);
            }
            Debug.Log("Output: " + output);
            return output;
        }

        public static string RunCommand(string cmd)
        {
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = cmd;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                Debug.LogError("Error running cmd: " + cmd + "\nOutput: " + output);
                throw new Exception("Couldn't execute cmd " + cmd);
            }
            return output;
        }

        private static string _projenodyConfigPath;

        public static string GetProjenodyConfigPath()
        {
            if (string.IsNullOrEmpty(_projenodyConfigPath))
            {
                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

                _projenodyConfigPath = NormalizedPath(homePath + "/.projenody-cfg.json");
            }

            return _projenodyConfigPath;
        }

        private static readonly fsSerializer Serializer = new fsSerializer();

        public static T GetObject<T>(string path)
        {
            string json = File.ReadAllText(path);
            fsData data = fsJsonParser.Parse(json);

            // step 2: deserialize the data
            object deserialized = null;
            Serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();

            return (T) deserialized;
        }

        public static void WriteObject<T>(string path, object item)
        {
            // serialize the data
            fsData data;
            Serializer.TrySerialize(typeof(T), item, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            File.WriteAllText(path, fsJsonPrinter.PrettyJson(data));
        }

        public static string NormalizedPath(string path)
        {
            return new Uri(path).LocalPath;
        }

        public static bool IsRootFolder(string path)
        {
            return new DirectoryInfo(path).Parent == null;
        }

        public static string GetFilePath(string directory, string filename)
        {
            while (true)
            {
                string path = NormalizedPath(directory + "/" + filename);
                if (File.Exists(path))
                {
                    return path;
                }
                if ((new DirectoryInfo(directory).Name.Equals("node_modules") || IsRootFolder(directory))) return null;
                directory = directory + "../";
            }
        }

        private static Config _config;

        public static Config Config
        {
            get
            {
                if (_config == null)
                {
                    _config = GetObject<Config>(GetProjenodyConfigPath());
                }
                return _config;
            }
        }
    }
}