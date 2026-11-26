#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NooMiBetter.Editor
{
    [InitializeOnLoad]
    public static class NooMiBetterProjectSetup
    {
        static NooMiBetterProjectSetup() { EditorApplication.delayCall += EnsureBuildSettings; }
        public static void EnsureBuildSettings()
        {
            Directory.CreateDirectory("Assets/Scenes");
            const string scenePath = "Assets/Scenes/Main.unity";
            if (!File.Exists(scenePath)) {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                new GameObject("NooMiBetterRoot").AddComponent<NooMiBetter.NooMiBetterApp>();
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
            PlayerSettings.productName = "NooMiBetter";
            PlayerSettings.companyName = "NooMiBetter Studio";
            PlayerSettings.applicationIdentifier = "com.noomibetter.game";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Branding/noomibetter-icon.png");
            if (icon != null) {
                int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Android);
                var icons = new Texture2D[sizes.Length];
                for (int i=0;i<icons.Length;i++) icons[i]=icon;
                if (icons.Length>0) PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android,icons);
            }
        }
    }
}
#endif
