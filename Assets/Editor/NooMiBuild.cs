#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NooMiBetter.Editor
{
    public static class NooMiBuild
    {
        [MenuItem("NooMiBetter/Build Android APK")]
        public static void BuildAndroid()
        {
            NooMiBetterProjectSetup.EnsureBuildSettings();
            Directory.CreateDirectory("Build");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = "Build/NooMiBetter.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Android build failed: " + report.summary.result);
            Debug.Log("NooMiBetter APK: " + report.summary.outputPath);
        }
    }
}
#endif
