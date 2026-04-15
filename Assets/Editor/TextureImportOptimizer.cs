using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class TextureImportOptimizer
    {
        [MenuItem("Tools/Optimization/Fix Texture Compression (Animations Only)")]
        public static void FixAnimationTextureCompression()
        {
            OptimizeTextures(new[] { "Assets/_Levels" }, true, "Animations");
        }

        [MenuItem("Tools/Optimization/Fix ALL Texture Compression")]
        public static void FixAllTextureCompression()
        {
            OptimizeTextures(new[] { "Assets/_Levels", "Assets/_Modules" }, false, "All Textures");
        }

        private static void OptimizeTextures(string[] searchPaths, bool animationsOnly, string label)
        {
            int fixedCount = 0;
            int skippedCount = 0;
            int totalProcessed = 0;

            try
            {
                foreach (string searchPath in searchPaths)
                {
                    string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { searchPath });

                    for (int i = 0; i < guids.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                        if (animationsOnly && !path.Contains("/Animations/"))
                            continue;

                        if (!path.EndsWith(".png") && !path.EndsWith(".jpg") && !path.EndsWith(".jpeg"))
                            continue;

                        totalProcessed++;
                        EditorUtility.DisplayProgressBar($"Fixing {label} Compression",
                            $"Processing {path}... ({totalProcessed})",
                            (float)(i + 1) / guids.Length);

                        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                        if (importer == null) continue;

                        TextureImporterPlatformSettings defaultSettings =
                            importer.GetDefaultPlatformTextureSettings();
                        bool needsReimport = false;

                        // Fix default platform: enable compression if uncompressed
                        if (defaultSettings.textureCompression == TextureImporterCompression.Uncompressed)
                        {
                            defaultSettings.textureCompression = TextureImporterCompression.Compressed;
                            defaultSettings.crunchedCompression = true;
                            defaultSettings.compressionQuality = 75;
                            importer.SetPlatformTextureSettings(defaultSettings);
                            needsReimport = true;
                        }

                        // Add/fix Android override with ASTC_6x6
                        TextureImporterPlatformSettings androidSettings =
                            importer.GetPlatformTextureSettings("Android");
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
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[TextureOptimizer] {label} done. Fixed: {fixedCount}, Already OK: {skippedCount}");
            EditorUtility.DisplayDialog($"{label} Optimization Complete",
                $"Fixed: {fixedCount} textures\nAlready OK: {skippedCount} textures", "OK");
        }
    }
}
