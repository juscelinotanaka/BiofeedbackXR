using System;
using System.Linq;
using Unity.Build;
using Unity.Build.Common;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BuildSettings
{
    public static class BuildEditorMenu
    {
        private const string CurrentConfigKey = "CurrentConfigurationKey";

        #region Paths

        private const string BuilderSetDefaultConfigurationPath = "Builder/Set as Default Configuration";
        private const string BuilderSelectConfigurationPath = "Builder/Select Configuration";
        private const string BuilderBuildAndRunPath = "Builder/Build and Run";
        private const string BuilderRunLatestPath = "Builder/Run Latest";
        private const string BuilderLaunchOnAndroidPath = "Builder/Launch on Android";

        #endregion

        private static BuildConfiguration CurrentConfiguration
        {
            get
            {
                string path = EditorPrefs.GetString(CurrentConfigKey);
                return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<BuildConfiguration>(path);
            }

            set
            {
                string path = AssetDatabase.GetAssetPath(value);
                EditorPrefs.SetString(CurrentConfigKey, path);
            }
        }

        [MenuItem(BuilderSetDefaultConfigurationPath, false, 1000)]
        public static void SetDefaultConfiguration()
        {
            Object current = Selection.activeObject;

            CurrentConfiguration = current as BuildConfiguration;
        }

        [MenuItem(BuilderSetDefaultConfigurationPath, true, 1000)]
        public static bool SetDefaultConfiguration_Validate()
        {
            return Selection.activeObject != null && Selection.activeObject is BuildConfiguration;
        }

        [MenuItem(BuilderSelectConfigurationPath, false, 1001)]
        public static void BuilderSelectConfiguration()
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = CurrentConfiguration;
        }

        [MenuItem(BuilderSelectConfigurationPath, true, 1001)]
        public static bool BuilderSelectConfiguration_Validate()
        {
            return CurrentConfiguration != null;
        }

        [MenuItem(BuilderBuildAndRunPath, false, 100)]
        public static void BuildAndRun()
        {
            BuildResult result = CurrentConfiguration.Build();
            if (result.Succeeded)
            {
                CurrentConfiguration.Run();
            }
        }

        [MenuItem(BuilderBuildAndRunPath, true, 100)]
        public static bool BuildAndRun_Validate()
        {
            return CurrentConfiguration != null;
        }

        [MenuItem(BuilderRunLatestPath, false, 101)]
        public static void RunLatest()
        {
            if (CurrentConfiguration.GetLastBuildResult().Succeeded)
            {
                CurrentConfiguration.Run();
            }
        }

        [MenuItem(BuilderRunLatestPath, true, 101)]
        public static bool RunLatest_Validate()
        {
            return CurrentConfiguration != null;
        }

        [MenuItem(BuilderLaunchOnAndroidPath, false, 102)]
        public static void LaunchOnAndroid()
        {
#if UNITY_2020_1_OR_NEWER
            ApplicationIdentifier applicationIdentifier = CurrentConfiguration.GetComponentOrDefault<ApplicationIdentifier>();
            string packageName = applicationIdentifier.PackageName;
#else
            //BUG it may not be valid since the application identifier on SBP may be different from the on on PlayerSets
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#endif

            string runTarget = $"\"{packageName}/com.unity3d.player.UnityPlayerActivity\"";
            ADB adb = ADB.GetInstance();
            
            try
            {
                EditorUtility.DisplayProgressBar("Launching", $"Launching {runTarget}", 0.6f);
                adb.Run(new[]
                {
                    "shell", "am", "start",
                    "-a", "android.intent.action.MAIN",
                    "-c", "android.intent.category.LAUNCHER",
                    "-f", "0x10200000",
                    "-S",
                    "-n", runTarget
                }, $"Failed to launch {runTarget}");
            }
            catch (Exception ex)
            {
                Debug.Log($"Error when launching: {ex}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

        }

        [MenuItem(BuilderLaunchOnAndroidPath, true, 102)]
        public static bool LaunchOnAndroid_Validate()
        {
            return CurrentConfiguration != null &&
                   CurrentConfiguration.GetBuildPipeline().UsedComponents
                       .Any(type => type.Name.Contains("ApplicationIdentifier"));
        }
    }
}