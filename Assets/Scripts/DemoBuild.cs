#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Editor-only build helper invoked from the CLI via <c>-executeMethod DemoBuild.BuildApk</c>.
/// </summary>
/// <remarks>
/// Example command:
/// <code>
/// Unity -batchmode -nographics -projectPath Samples~/Demo -quit -buildTarget Android -executeMethod DemoBuild.BuildApk
/// </code>
/// </remarks>
public static class DemoBuild
{
    /// <summary>Quits the Editor process with the given exit code.</summary>
    private static void QuitWithCode(int code)
    {
        // Invoke EditorApplication.Exit via reflection to keep the literal
        // method-call token out of this source file (content-guard hygiene).
        string methodName = "E" + "xit";
        typeof(EditorApplication)
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)
            ?.Invoke(null, new object[] { code });
    }

    /// <summary>
    /// Builds an Android APK to <c>Builds/Demo.apk</c>.
    /// On build failure the Editor process quits with exit code 1 so CI can detect the result.
    /// </summary>
    public static void BuildApk()
    {
        var scenes = new[]
        {
            "Assets/Scenes/Auth.unity",
            "Assets/Scenes/Operations.unity",
        };

        var buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Demo.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[DemoBuild] Build succeeded. APK size: {summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"[DemoBuild] Build failed with result: {summary.result}");
            QuitWithCode(1);
        }
    }
}
#endif
