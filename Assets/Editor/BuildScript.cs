using UnityEditor;
using System.IO;

public static class BuildScript
{
    [MenuItem("Build/Build Client")]
    public static void BuildClient()
    {
        string path = "Build/Client/ReHaB.exe";
        BuildPlayerOptions buildPlayerOptions = new()
        {
            scenes = new[] { "Assets/Scenes/Init.unity", "Assets/Scenes/Scena_Rehab.unity" },
            locationPathName = path,
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
