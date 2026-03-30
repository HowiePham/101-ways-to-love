using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class FontOptimizer
    {
        [MenuItem("Tools/Optimization/Font Size Report")]
        public static void FontSizeReport()
        {
            Debug.Log("========== FONT SIZE REPORT ==========");

            string[] fontDirs =
            {
                "Assets/GeneratedData/Localization/Fonts",
                "Assets/TextMesh Pro/Resources",
                "Assets/_Modules/_UI/_Shared/Font"
            };

            long grandTotal = 0;
            int totalFiles = 0;

            foreach (string dir in fontDirs)
            {
                if (!Directory.Exists(dir)) continue;

                string[] files = Directory.GetFiles(dir)
                    .Where(f => !f.EndsWith(".meta"))
                    .OrderByDescending(f => new FileInfo(f).Length)
                    .ToArray();

                long dirTotal = files.Sum(f => new FileInfo(f).Length);
                grandTotal += dirTotal;
                totalFiles += files.Length;

                Debug.Log($"<b>{dir}</b> ({FormatSize(dirTotal)} total, {files.Length} files)");

                foreach (string file in files)
                {
                    long size = new FileInfo(file).Length;
                    string name = Path.GetFileName(file);
                    string ext = Path.GetExtension(file).ToLower();
                    string type = (ext == ".ttf" || ext == ".otf") ? "Source Font" : "SDF Atlas";
                    Debug.Log($"  {FormatSize(size),8}  [{type}]  {name}");
                }
            }

            Debug.Log($"=== TOTAL FONT ASSETS: {FormatSize(grandTotal)} ===");

            EditorUtility.DisplayDialog("Font Size Report",
                $"Total: {FormatSize(grandTotal)} across {totalFiles} files\n\nSee Console for details.",
                "OK");
        }

        [MenuItem("Tools/Optimization/Strip Font Source Data")]
        public static void StripFontSourceData()
        {
            // Find all SDF font assets to determine which source fonts they reference
            string[] sdfGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            var referencedFontPaths = new HashSet<string>();

            foreach (string guid in sdfGuids)
            {
                string sdfPath = AssetDatabase.GUIDToAssetPath(guid);
                // Read the SDF asset to find source font reference
                string content = File.ReadAllText(sdfPath);
                // Look for m_SourceFontFile reference
                foreach (string line in content.Split('\n'))
                {
                    if (!line.Contains("m_SourceFontFile")) continue;
                    // Extract guid from {fileID: xxx, guid: xxx, type: x}
                    int guidStart = line.IndexOf("guid: ");
                    if (guidStart < 0) continue;
                    string fontGuid = line.Substring(guidStart + 6, 32);
                    string fontPath = AssetDatabase.GUIDToAssetPath(fontGuid);
                    if (!string.IsNullOrEmpty(fontPath))
                        referencedFontPaths.Add(fontPath);
                }
            }

            // Find all font files
            string[] fontGuids = AssetDatabase.FindAssets("t:Font",
                new[] { "Assets/GeneratedData/Localization/Fonts" });

            int strippedCount = 0;
            int alreadyStripped = 0;
            int keptCount = 0;
            long savedBytes = 0;

            Debug.Log("========== FONT SOURCE DATA STRIPPING ==========");

            foreach (string guid in fontGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                string metaPath = path + ".meta";
                if (!File.Exists(metaPath)) continue;

                string metaContent = File.ReadAllText(metaPath);
                bool hasIncludeFontData = metaContent.Contains("includeFontData:");
                bool currentlyIncluded = !metaContent.Contains("includeFontData: 0");

                string fileName = Path.GetFileName(path);
                long fileSize = new FileInfo(path).Length;

                // Only strip if the font is referenced by an SDF asset (has a pre-built atlas)
                if (referencedFontPaths.Contains(path))
                {
                    if (currentlyIncluded)
                    {
                        // Strip source data — SDF atlas exists, runtime doesn't need the raw font
                        metaContent = hasIncludeFontData
                            ? metaContent.Replace("includeFontData: 1", "includeFontData: 0")
                            : metaContent.Replace("fontSize:", "includeFontData: 0\n    fontSize:");

                        File.WriteAllText(metaPath, metaContent);
                        strippedCount++;
                        savedBytes += fileSize;
                        Debug.Log($"  STRIPPED: {fileName} ({FormatSize(fileSize)}) — SDF atlas exists");
                    }
                    else
                    {
                        alreadyStripped++;
                        Debug.Log($"  Already stripped: {fileName}");
                    }
                }
                else
                {
                    keptCount++;
                    Debug.Log($"  KEPT: {fileName} ({FormatSize(fileSize)}) — no SDF atlas, source needed");
                }
            }

            Debug.Log($"\n=== STRIPPING COMPLETE ===");
            Debug.Log($"Stripped: {strippedCount} fonts");
            Debug.Log($"Already stripped: {alreadyStripped}");
            Debug.Log($"Kept (no SDF atlas): {keptCount}");
            Debug.Log($"Estimated savings: {FormatSize(savedBytes)}");

            if (strippedCount > 0)
            {
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog("Font Stripping Complete",
                $"Stripped: {strippedCount} fonts\n" +
                $"Already stripped: {alreadyStripped}\n" +
                $"Kept: {keptCount}\n" +
                $"Estimated savings: {FormatSize(savedBytes)}\n\n" +
                "IMPORTANT: Test TextMeshPro rendering in Play mode!",
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
