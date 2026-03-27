using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class TextureImportOptimizer
    {
        [MenuItem("Tools/Optimization/Fix Texture Compression")]
        public static void FixTextureCompression()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Levels" });
            int fixedCount = 0;
            int skippedCount = 0;

            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    if (!path.Contains("/Animations/") || !path.EndsWith(".png"))
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar("Fixing Texture Compression",
                        $"Processing {path}... ({i + 1}/{guids.Length})",
                        (float)(i + 1) / guids.Length);

                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;

                    TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
                    bool needsReimport = false;

                    // Fix default platform: enable compression
                    if (defaultSettings.textureCompression == TextureImporterCompression.Uncompressed)
                    {
                        defaultSettings.textureCompression = TextureImporterCompression.Compressed;
                        defaultSettings.crunchedCompression = true;
                        defaultSettings.compressionQuality = 75;
                        importer.SetPlatformTextureSettings(defaultSettings);
                        needsReimport = true;
                    }

                    // Add/fix Android override with ASTC
                    TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
                    if (!androidSettings.overridden ||
                        androidSettings.format != TextureImporterFormat.ASTC_6x6 ||
                        !androidSettings.crunchedCompression)
                    {
                        androidSettings.overridden = true;
                        androidSettings.name = "Android";
                        androidSettings.maxTextureSize = defaultSettings.maxTextureSize;
                        androidSettings.format = TextureImporterFormat.ASTC_6x6;
                        androidSettings.textureCompression = TextureImporterCompression.Compressed;
                        androidSettings.crunchedCompression = true;
                        androidSettings.compressionQuality = 75;
                        importer.SetPlatformTextureSettings(androidSettings);
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

            Debug.Log($"[TextureOptimizer] Done. Fixed: {fixedCount}, Already OK: {skippedCount}");
            EditorUtility.DisplayDialog("Texture Optimization Complete",
                $"Fixed: {fixedCount} textures\nAlready OK: {skippedCount} textures", "OK");
        }
    }
}
