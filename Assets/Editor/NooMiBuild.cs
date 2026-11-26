#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NooMiBetter.Editor
{
    public static class NooMiBuild
    {
        [MenuItem("NooMiBetter/Build Android APK")]
        public static void BuildAndroid() {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "NooMiBetter";
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/NooMiBetter.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/NooMiBetter.unity", true) };
            PlayerSettings.companyName = "NooMiBetter";
            PlayerSettings.productName = "NooMiBetter";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.noomibetter.game");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/Scenes/NooMiBetter.unity" },
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
