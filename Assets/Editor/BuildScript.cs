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
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            }

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

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Dedicated Server build ({summary.totalSize / 1024 / 1024} MB) succeeded in {summary.totalTime}.");
            }
            else
            {
                Debug.LogError($"Dedicated Server build failed! ({summary.result})");
            }
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
                Debug.Log($"Android VR build ({summary.totalSize / 1024 / 1024} MB) succeeded in {summary.totalTime}.");
            }
            else
            {
                Debug.LogError($"Android VR build failed! ({summary.result})");
            }
        }

        [MenuItem("ReHaB/Build/Client (Windows)")]
        public static void BuildWindowsClient()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
            }

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

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Windows Client build ({summary.totalSize / 1024 / 1024} MB) succeeded in {summary.totalTime}.");
            }
            else
            {
                Debug.LogError($"Windows Client build failed! ({summary.result})");
            }
        }
    }
}
