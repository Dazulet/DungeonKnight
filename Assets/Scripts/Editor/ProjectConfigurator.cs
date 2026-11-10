using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public static class ProjectConfigurator
    {
        static readonly (int index, string name)[] Layers =
        {
            (GameLayers.Player, "Player"),
            (GameLayers.Enemy, "Enemy"),
            (GameLayers.Projectile, "Projectile"),
            (GameLayers.Pickup, "Pickup"),
            (GameLayers.Obstacle, "Obstacle"),
        };

        public static void Configure()
        {
            ConfigurePlayer();
            ConfigureEditor();
            EnsureLayers();
            ReimportArt();
            ConfigureFont();
            Physics2D.gravity = Vector2.zero;
            QualitySettings.antiAliasing = 0;
        }

        static void ConfigurePlayer()
        {
            PlayerSettings.companyName = "Narxoz University";
            PlayerSettings.productName = "Dungeon Knight";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.narxoz.dungeonknight");
            PlayerSettings.bundleVersion = "1.0.0";

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.Android.renderOutsideSafeArea = true;
            PlayerSettings.Android.startInFullscreen = true;

            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
        }

        static void ConfigureEditor()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        }

        static void EnsureLayers()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null) return;
            foreach (var (index, name) in Layers)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                if (layer.stringValue == name) continue;
                if (!string.IsNullOrEmpty(layer.stringValue))
                    Debug.LogWarning($"Layer {index} was '{layer.stringValue}', renaming it to '{name}'.");
                layer.stringValue = name;
            }
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ReimportArt()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { BuilderUtil.ArtRoot });
            int changed = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (AssetImporter.GetAtPath(path) is TextureImporter importer && PixelArtImportSettings.Apply(importer, path))
                    {
                        importer.SaveAndReimport();
                        changed++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            if (changed > 0) Debug.Log($"Dungeon Knight: re-imported {changed} textures with pixel-art settings.");
        }

        static void ConfigureFont()
        {
            if (AssetImporter.GetAtPath(BuilderUtil.FontPath) is TrueTypeFontImporter font
                && font.fontRenderingMode != FontRenderingMode.HintedRaster)
            {
                font.fontRenderingMode = FontRenderingMode.HintedRaster;
                font.SaveAndReimport();
            }
        }

        public static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{BuilderUtil.SceneRoot}/{SceneNames.MainMenu}.unity", true),
                new EditorBuildSettingsScene($"{BuilderUtil.SceneRoot}/{SceneNames.Dungeon}.unity", true),
                new EditorBuildSettingsScene($"{BuilderUtil.SceneRoot}/{SceneNames.Endless}.unity", true),
            };
        }
    }
}
