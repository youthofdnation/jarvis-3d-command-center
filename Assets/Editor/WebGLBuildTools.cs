#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class WebGLBuildTools
{
    private const string ScenePath = "Assets/Scenes/CommandCenterMockScene.unity";
    private const string OutputPath = "Builds/WebGL";
    private const int DefaultMemorySizeMb = 512;

    [MenuItem("Jarvis3D/WebGL/Build (Development)")]
    public static void BuildDevelopment()
    {
        BuildWebGL(developmentBuild: true);
    }

    [MenuItem("Jarvis3D/WebGL/Build (Release)")]
    public static void BuildRelease()
    {
        BuildWebGL(developmentBuild: false);
    }

    [MenuItem("Jarvis3D/WebGL/Open Build Folder")]
    public static void OpenBuildFolder()
    {
        var absolute = Path.GetFullPath(OutputPath);
        if (!Directory.Exists(absolute))
        {
            Directory.CreateDirectory(absolute);
        }
        EditorUtility.RevealInFinder(absolute);
    }

    // Batch mode entry:
    // Unity -batchmode -quit -projectPath <path> -executeMethod WebGLBuildTools.BuildReleaseFromCommandLine
    public static void BuildReleaseFromCommandLine()
    {
        BuildWebGL(developmentBuild: false);
    }

    // Batch mode entry:
    // Unity -batchmode -quit -projectPath <path> -executeMethod WebGLBuildTools.BuildDevelopmentFromCommandLine
    public static void BuildDevelopmentFromCommandLine()
    {
        BuildWebGL(developmentBuild: true);
    }

    private static void BuildWebGL(bool developmentBuild)
    {
        EnsureSceneReady();
        ConfigurePlayerSettings();

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            throw new Exception("[Jarvis3D] Failed to switch active build target to WebGL.");
        }

        var options = BuildOptions.None;
        if (developmentBuild)
        {
            options |= BuildOptions.Development;
        }

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            target = BuildTarget.WebGL,
            locationPathName = OutputPath,
            options = options,
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;
        var elapsed = $"{summary.totalTime.TotalSeconds:F1}s";

        if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception($"[Jarvis3D] WebGL build failed. result={summary.result}, errors={summary.totalErrors}");
        }

        Debug.Log(
            $"[Jarvis3D] WebGL build succeeded. " +
            $"path={Path.GetFullPath(OutputPath)}, " +
            $"size={summary.totalSize / (1024f * 1024f):F2}MB, " +
            $"time={elapsed}, " +
            $"development={developmentBuild}");
    }

    private static void EnsureSceneReady()
    {
        if (!File.Exists(ScenePath))
        {
            CreateCommandCenterScene.CreateScene();
        }

        if (Application.isBatchMode)
        {
            EditorSceneManager.SaveOpenScenes();
        }
        else
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.companyName = "Jarvis";
        PlayerSettings.productName = "Jarvis3DCommandCenter";
        PlayerSettings.runInBackground = true;

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.nameFilesAsHashes = false;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = DefaultMemorySizeMb;
    }
}
#endif

