using UnityEditor;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    
    public class PixelArtImportSettings : AssetPostprocessor
    {
        public const int PixelsPerUnit = 16;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(BuilderUtil.ArtRoot + "/")) return;
            Apply((TextureImporter)assetImporter, assetPath);
        }

        public static bool Apply(TextureImporter importer, string path)
        {
            string file = System.IO.Path.GetFileNameWithoutExtension(path);
            bool soft = file == "glow" || file == "vignette" || file == "shadow";
            Vector4 border = file == "ui_button" || file == "ui_button_pressed" ? new Vector4(5, 5, 5, 5)
                : file == "ui_panel" ? new Vector4(8, 8, 8, 8)
                : Vector4.zero;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit)) { importer.spritePixelsPerUnit = PixelsPerUnit; changed = true; }
            FilterMode filter = soft ? FilterMode.Bilinear : FilterMode.Point;
            if (importer.filterMode != filter) { importer.filterMode = filter; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
            if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
            if (importer.spriteBorder != border) { importer.spriteBorder = border; changed = true; }

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteMeshType != SpriteMeshType.FullRect)
            {
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                changed = true;
            }
            return changed;
        }
    }
}
