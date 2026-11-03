using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
   
    public static class DungeonKnightBuilder
    {
        const string ApkPath = "Builds/DungeonKnight.apk";

        [MenuItem("Dungeon Knight/Build Project", priority = 0)]
        public static void BuildAll()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            try
            {
                EditorUtility.DisplayProgressBar("Dungeon Knight", "Configuring project settings", 0.1f);
                ProjectConfigurator.Configure();
                AssetDatabase.Refresh();

                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                EditorUtility.DisplayProgressBar("Dungeon Knight", "Building prefabs", 0.35f);
                PrefabBuilder.BuildAll();

                EditorUtility.DisplayProgressBar("Dungeon Knight", "Building main menu", 0.6f);
                MainMenuBuilder.Build();

                EditorUtility.DisplayProgressBar("Dungeon Knight", "Building dungeon", 0.8f);
                DungeonSceneBuilder.Build();

                ProjectConfigurator.SetBuildScenes();
                AssetDatabase.SaveAssets();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorSceneManager.OpenScene($"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity");
            Debug.Log("Dungeon Knight: project built. Press Play in the MainMenu scene.");
        }

        [MenuItem("Dungeon Knight/Build Android APK", priority = 20)]
        public static void BuildAndroid()
        {
            if (!File.Exists($"{BuilderUtil.SceneRoot}/{SceneNames.Dungeon}.unity")) BuildAll();

            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    $"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity",
                    $"{BuilderUtil.SceneRoot}/{SceneNames.Dungeon}.unity",
                },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            EditorUserBuildSettings.buildAppBundle = false;
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"Dungeon Knight: APK written to {ApkPath} ({report.summary.totalSize / (1024 * 1024)} MB).");
            else
                Debug.LogError($"Dungeon Knight: Android build {report.summary.result}. Check that the Android Build Support module is installed.");
        }

        [InitializeOnLoadMethod]
        static void OfferFirstBuild()
        {
            if (Application.isBatchMode || SessionState.GetBool("DungeonKnight.Offered", false)) return;
            EditorApplication.delayCall += () =>
            {
                if (File.Exists($"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity")) return;
                SessionState.SetBool("DungeonKnight.Offered", true);
                if (EditorUtility.DisplayDialog("Dungeon Knight",
                        "The game scenes have not been built yet. Build the prefabs and scenes now?\n\n" +
                        "You can also do this later from the menu: Dungeon Knight > Build Project.",
                        "Build now", "Later"))
                    BuildAll();
            };
        }
    }
}
