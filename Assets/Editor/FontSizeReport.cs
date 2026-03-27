using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class FontSizeReport
    {
        [MenuItem("Tools/Optimization/Font Size Report")]
        public static void GenerateReport()
        {
            string[] fontPaths =
            {
                "Assets/GeneratedData/Localization/Fonts",
                "Assets/TextMesh Pro/Resources",
                "Assets/TextMesh Pro/Examples & Extras",
                "Assets/_Modules/_UI/_Shared/Font"
            };

            long totalSize = 0;

            Debug.Log("========== FONT SIZE REPORT ==========");

            foreach (string basePath in fontPaths)
            {
                if (!Directory.Exists(basePath)) continue;

                string[] files = Directory.GetFiles(basePath, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta"))
                    .OrderByDescending(f => new FileInfo(f).Length)
                    .ToArray();

                long folderSize = files.Sum(f => new FileInfo(f).Length);
                totalSize += folderSize;

                Debug.Log($"\n<b>{basePath}</b> ({FormatSize(folderSize)} total, {files.Length} files)");

                foreach (string file in files)
                {
                    long size = new FileInfo(file).Length;
                    if (size < 50 * 1024) continue; // Skip files < 50KB

                    string relativePath = file.Replace("\\", "/");
                    string fileName = Path.GetFileName(file);
                    string ext = Path.GetExtension(file).ToLower();

                    string type = ext switch
                    {
                        ".ttf" or ".otf" => "Source Font",
                        ".asset" => "SDF Atlas",
                        ".png" or ".jpg" => "Texture",
                        ".mat" => "Material",
                        _ => ext
                    };

                    Debug.Log($"  {FormatSize(size),8}  [{type}]  {fileName}");
                }
            }

            Debug.Log($"\n=== TOTAL FONT ASSETS: {FormatSize(totalSize)} ===");

            // Check TMP Examples
            string tmpExamples = "Assets/TextMesh Pro/Examples & Extras";
            if (Directory.Exists(tmpExamples))
            {
                long examplesSize = Directory.GetFiles(tmpExamples, "*.*", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".meta"))
                    .Sum(f => new FileInfo(f).Length);

                Debug.LogWarning($"[FontReport] 'TextMesh Pro/Examples & Extras' is {FormatSize(examplesSize)} and likely unused. " +
                                 "Consider deleting it to save space.");
            }

            // CJK font warning
            string[] cjkFonts = { "NotoSansSC", "NotoSansTC", "NotoSansJP", "NotoSansKR" };
            string locPath = "Assets/GeneratedData/Localization/Fonts";
            if (Directory.Exists(locPath))
            {
                long cjkSize = Directory.GetFiles(locPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => !f.EndsWith(".meta") && cjkFonts.Any(cjk => f.Contains(cjk)))
                    .Sum(f => new FileInfo(f).Length);

                if (cjkSize > 0)
                {
                    Debug.LogWarning($"[FontReport] CJK fonts total {FormatSize(cjkSize)}. " +
                                     "Consider loading these on-demand via Addressables per user language.");
                }
            }

            EditorUtility.DisplayDialog("Font Size Report",
                $"Total font assets: {FormatSize(totalSize)}\n\nSee Console for detailed breakdown.",
                "OK");
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1048576) return $"{bytes / 1048576f:F1}MB";
            if (bytes >= 1024) return $"{bytes / 1024f:F0}KB";
            return $"{bytes}B";
        }
    }
}
