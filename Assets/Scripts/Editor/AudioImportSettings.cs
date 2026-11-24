using UnityEditor;
using UnityEngine;

namespace DungeonKnight.EditorTools
{
    public class AudioImportSettings : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(AudioBuilder.AudioRoot + "/")) return;
            Apply((AudioImporter)assetImporter, assetPath);
        }

        public static void Apply(AudioImporter importer, string path)
        {
            bool isMusic = path.StartsWith(AudioBuilder.MusicRoot + "/");
            importer.forceToMono = true;
            importer.loadInBackground = isMusic;
            importer.ambisonic = false;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = isMusic ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = isMusic ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            settings.quality = isMusic ? 0.6f : 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
        }
    }
}
