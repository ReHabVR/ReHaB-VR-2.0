using UnityEditor;
using System.IO;
using UnityEngine;

public static class BuildScript
{
    [MenuItem("Build/Build Client")]
    public static void BuildClient()
    {
        string buildFolder = "Build/Client";
        if (!Directory.Exists(buildFolder))
            Directory.CreateDirectory(buildFolder);

        string templatePath = "Assets/StreamingAssets/ConnectionConfig.json";
        string targetPath = Path.Combine(buildFolder, "ConnectionConfig.json");

        if (File.Exists(templatePath) && !File.Exists(targetPath))
        {
            File.Copy(templatePath, targetPath);
        }

        string exePath = Path.Combine(buildFolder, "ReHaB.exe");
        BuildPlayerOptions buildPlayerOptions = new()
        {
            scenes = new[] { "Assets/Scenes/Init.unity", "Assets/Scenes/Scena_Rehab.unity" },
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }

    [MenuItem("Build/Build Server")]
    public static void BuildServer()
    {
        string path = "Build/DedicatedServer/Server.exe";
        if (Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.Delete(Path.GetDirectoryName(path), true);
        }

        BuildPlayerOptions buildPlayerOptions = new()
        {
            scenes = new[] { "Assets/Scenes/Init.unity", "Assets/Scenes/Scena_Rehab.unity" },
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            //options = BuildOptions.Development
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
