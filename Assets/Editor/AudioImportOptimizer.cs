using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AudioImportOptimizer
    {
        [MenuItem("Tools/Optimization/Fix Audio Import Settings")]
        public static void FixAudioImportSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Levels/_Shared/Sounds" });
            int fixedCount = 0;
            int skippedCount = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    EditorUtility.DisplayProgressBar("Fixing Audio Import Settings",
                        $"Processing {path}... ({i + 1}/{guids.Length})",
                        (float)(i + 1) / guids.Length);

                    var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                    if (importer == null) continue;

                    bool isBgm = path.Contains("/BGM/");
                    AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                    bool needsReimport = false;

                    AudioClipLoadType targetLoadType = isBgm
                        ? AudioClipLoadType.Streaming
                        : AudioClipLoadType.CompressedInMemory;

                    const float targetQuality = 0.55f;
                    const AudioCompressionFormat targetFormat = AudioCompressionFormat.Vorbis;
                    const AudioSampleRateSetting targetSampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
                    const uint targetSampleRate = 22050;

                    if (settings.loadType != targetLoadType ||
                        settings.compressionFormat != targetFormat ||
                        !Mathf.Approximately(settings.quality, targetQuality) ||
                        settings.sampleRateSetting != targetSampleRateSetting ||
                        settings.sampleRateOverride != targetSampleRate)
                    {
                        settings.loadType = targetLoadType;
                        settings.compressionFormat = targetFormat;
                        settings.quality = targetQuality;
                        settings.sampleRateSetting = targetSampleRateSetting;
                        settings.sampleRateOverride = targetSampleRate;
                        importer.defaultSampleSettings = settings;
                        needsReimport = true;
                    }

                    if (needsReimport)
                    {
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[AudioOptimizer] Done. Fixed: {fixedCount}, Already OK: {skippedCount}");
            EditorUtility.DisplayDialog("Audio Optimization Complete",
                $"Fixed: {fixedCount} audio clips\nAlready OK: {skippedCount} audio clips", "OK");
        }
    }
}
