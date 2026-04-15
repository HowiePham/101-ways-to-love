using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Batch-applies optimal Android ASTC compression to level and UI textures.
    ///
    /// Strategy:
    ///   - Full-screen background PNGs  → ASTC 8x8, maxSize 1024  (~75% smaller than ETC2)
    ///   - All other textures (Spine atlas, drag, static elements, UI) → ASTC 6x6, maxSize 2048  (~55% smaller)
    ///
    /// Important: ASTC does NOT support Crunch compression. crunchedCompression is always false for ASTC.
    /// Min SDK 23 + ARM64-only target means all devices have hardware ASTC support — no fallback needed.
    /// </summary>
    public static class TextureImportOptimizer
    {
        [MenuItem("Tools/Optimization/Fix Texture Compression (Animations Only)")]
        public static void FixAnimationTextureCompression()
        {
            RunOptimizer(new[] { "Assets/_Levels" }, animationsOnly: true);
        }

        [MenuItem("Tools/Optimization/Fix ALL Texture Compression")]
        public static void FixAllTextureCompression()
        {
            RunOptimizer(new[] { "Assets/_Levels", "Assets/_Modules/_UI" }, animationsOnly: false);
        }

        private static void RunOptimizer(string[] searchPaths, bool animationsOnly)
        {
            int fixedCount = 0;
            int skippedCount = 0;
            int total = 0;

            try
            {
                foreach (string searchPath in searchPaths)
                {
                    string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { searchPath });

                    for (int i = 0; i < guids.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                        if (!path.EndsWith(".png") && !path.EndsWith(".jpg") && !path.EndsWith(".jpeg"))
                            continue;

                        if (animationsOnly && !path.Contains("/Animations/"))
                            continue;

                        total++;
                        EditorUtility.DisplayProgressBar(
                            "Fixing Texture Compression",
                            $"({total}) {path}",
                            (float)(i + 1) / guids.Length);

                        if (ApplyOptimalAndroidSettings(path))
                            fixedCount++;
                        else
                            skippedCount++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[TextureOptimizer] Done. Total: {total} | Fixed: {fixedCount} | Already OK: {skippedCount}");
            EditorUtility.DisplayDialog("Texture Optimization Complete",
                $"Total processed: {total}\nFixed: {fixedCount}\nAlready OK: {skippedCount}", "OK");
        }

        /// <summary>
        /// Applies the correct ASTC Android override for this texture.
        /// Returns true if the importer was changed and re-imported.
        /// </summary>
        private static bool ApplyOptimalAndroidSettings(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            string fileName = Path.GetFileName(path);
            bool isBackground = IsFullScreenBackground(fileName);

            // Backgrounds get maximum compression + halved max size (saves the most space).
            // All other level/UI textures get ASTC 6x6 at full resolution.
            TextureImporterFormat targetFormat = isBackground
                ? TextureImporterFormat.ASTC_8x8
                : TextureImporterFormat.ASTC_6x6;
            int targetMaxSize = isBackground ? 1024 : 2048;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");

            // Already correct — skip to avoid unnecessary reimport
            bool alreadyOk = android.overridden
                             && android.format == targetFormat
                             && !android.crunchedCompression   // ASTC never uses crunch
                             && android.maxTextureSize == targetMaxSize;
            if (alreadyOk) return false;

            android.overridden = true;
            android.name = "Android";
            android.format = targetFormat;
            android.textureCompression = TextureImporterCompression.Compressed;
            android.crunchedCompression = false;  // ASTC does not support Crunch — must be false
            android.compressionQuality = 50;
            android.maxTextureSize = targetMaxSize;

            importer.SetPlatformTextureSettings(android);
            importer.SaveAndReimport();
            return true;
        }

        /// <summary>
        /// Returns true if the file is a full-screen or large background that can safely
        /// use ASTC 8x8 (max compression) with maxTextureSize 1024.
        ///
        /// Patterns matched:
        ///   "Static_bg.png"         — canonical full-screen level background
        ///   "BG 1.png", "BG 2.png"  — numbered background variants
        ///   "static_*_bg.png"       — named backgrounds (static_down_bg, static_top_bg, static_wal_bg, etc.)
        /// </summary>
        private static bool IsFullScreenBackground(string fileName)
        {
            string lower = fileName.ToLower();

            // Exact canonical background name
            if (lower == "static_bg.png") return true;

            // "BG 1.png", "BG 2.png", "bg_*.png" style
            if (lower.StartsWith("bg ") || lower.StartsWith("bg_")) return true;

            // "static_*_bg.png" — e.g. static_down_bg.png, static_top_bg.png, static_wal_bg.png
            if (lower.StartsWith("static_") && lower.EndsWith("_bg.png")) return true;

            return false;
        }
    }
}
