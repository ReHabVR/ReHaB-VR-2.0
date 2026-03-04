using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEditor.Build.Reporting;

namespace Editor
{
    public static class BuildScript
    {
        static readonly string[] Scenes = {
            "Assets/Scenes/Init.unity",
            "Assets/Scenes/Scena_Rehab.unity"
        };

        [MenuItem("ReHaB/Build/Dedicated Server")]
        public static void BuildServer()
        {
            string path = "Build/DedicatedServer/Server.exe";
            if (Directory.Exists(Path.GetDirectoryName(path)))
            {
                // Ensure clean build
                Directory.Delete(Path.GetDirectoryName(path), true);
            }

            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = Scenes,
                locationPathName = path,
                target = BuildTarget.StandaloneWindows64,
                subtarget = (int)StandaloneBuildSubtarget.Server,
                //options = BuildOptions.Development
            };
            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        [MenuItem("ReHaB/Build/Client (Android VR)")]
        public static void BuildAndroidClient()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            string buildFolder = "Build/VR";
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string path = Path.Combine(buildFolder, "ReHaB_VR.apk");
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = Scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AutoRunPlayer
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Android build succeeded: {summary.totalSize / 1024 / 1024} MB");
            }
            else
            {
                Debug.LogError("Android build failed.");
            }
        }

        [MenuItem("ReHaB/Build/Client (Windows)")]
        public static void BuildWindowsClient()
        {
            string buildFolder = "Build/Client";
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string templatePath = "Assets/StreamingAssets/ConnectionConfig.json";
            string targetPath = Path.Combine(buildFolder, "ConnectionConfig.json");

            if (File.Exists(templatePath) && !File.Exists(targetPath))
            {
                File.Copy(templatePath, targetPath);
            }

            string exePath = Path.Combine(buildFolder, "ReHaB.exe");
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = Scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
    }
}