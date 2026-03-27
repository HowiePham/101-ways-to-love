using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AudioDeduplicator
    {
        [MenuItem("Tools/Optimization/Find Duplicate Audio")]
        public static void FindDuplicateAudio()
        {
            string basePath = "Assets/_Levels/_Shared/Sounds";
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { basePath });

            // Group by filename
            var fileGroups = new Dictionary<string, List<string>>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Only look at per-level folders (L_xxx/)
                if (!path.Contains("/L_")) continue;

                string fileName = Path.GetFileName(path);
                if (!fileGroups.ContainsKey(fileName))
                {
                    fileGroups[fileName] = new List<string>();
                }
                fileGroups[fileName].Add(path);
            }

            // Filter to duplicates only
            var duplicates = fileGroups
                .Where(kvp => kvp.Value.Count > 1)
                .OrderByDescending(kvp => kvp.Value.Count)
                .ToList();

            if (duplicates.Count == 0)
            {
                Debug.Log("[AudioDeduplicator] No duplicate audio files found.");
                EditorUtility.DisplayDialog("Audio Deduplication", "No duplicates found.", "OK");
                return;
            }

            int totalDuplicateFiles = duplicates.Sum(d => d.Value.Count - 1);
            long totalWastedBytes = 0;

            Debug.Log("========== DUPLICATE AUDIO REPORT ==========");
            foreach (var kvp in duplicates)
            {
                long fileSize = new FileInfo(kvp.Value[0]).Length;
                long wastedBytes = fileSize * (kvp.Value.Count - 1);
                totalWastedBytes += wastedBytes;

                Debug.Log($"<b>{kvp.Key}</b> ({kvp.Value.Count} copies, {FormatSize(fileSize)} each, {FormatSize(wastedBytes)} wasted)");
                foreach (string path in kvp.Value)
                {
                    int refCount = FindReferencesCount(path);
                    string refLabel = refCount > 0 ? $" [REFERENCED x{refCount}]" : " [unreferenced]";
                    Debug.Log($"  - {path}{refLabel}");
                }
            }

            Debug.Log($"=== TOTAL: {duplicates.Count} duplicated files, {totalDuplicateFiles} extra copies, {FormatSize(totalWastedBytes)} wasted ===");

            EditorUtility.DisplayDialog("Audio Deduplication Report",
                $"Found {duplicates.Count} duplicated audio files\n" +
                $"{totalDuplicateFiles} extra copies\n" +
                $"{FormatSize(totalWastedBytes)} wasted\n\n" +
                "See Console for full report.\n" +
                "Use 'Consolidate Duplicates' to fix.",
                "OK");
        }

        [MenuItem("Tools/Optimization/Consolidate Duplicate Audio")]
        public static void ConsolidateDuplicateAudio()
        {
            string basePath = "Assets/_Levels/_Shared/Sounds";
            string commonPath = "Assets/_Levels/_Shared/Sounds/Common";

            if (!AssetDatabase.IsValidFolder(commonPath))
            {
                AssetDatabase.CreateFolder("Assets/_Levels/_Shared/Sounds", "Common");
            }

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { basePath });

            var fileGroups = new Dictionary<string, List<string>>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/L_")) continue;

                string fileName = Path.GetFileName(path);
                if (!fileGroups.ContainsKey(fileName))
                {
                    fileGroups[fileName] = new List<string>();
                }
                fileGroups[fileName].Add(path);
            }

            var duplicates = fileGroups.Where(kvp => kvp.Value.Count > 1).ToList();

            if (duplicates.Count == 0)
            {
                Debug.Log("[AudioDeduplicator] No duplicates to consolidate.");
                return;
            }

            if (!EditorUtility.DisplayDialog("Consolidate Duplicates",
                    $"This will consolidate {duplicates.Count} duplicated audio files into {commonPath}/.\n\n" +
                    "For each duplicate set:\n" +
                    "1. The most-referenced copy is kept and moved to Common/\n" +
                    "2. All other copies have their references remapped to the kept copy\n" +
                    "3. Unreferenced copies are deleted\n\n" +
                    "Make sure to commit your changes first!",
                    "Proceed", "Cancel"))
            {
                return;
            }

            int movedCount = 0;
            int remappedCount = 0;
            int deletedCount = 0;

            try
            {
                foreach (var kvp in duplicates)
                {
                    string fileName = kvp.Key;
                    string targetPath = $"{commonPath}/{fileName}";
                    List<string> copies = kvp.Value;

                    EditorUtility.DisplayProgressBar("Consolidating Audio",
                        $"Processing {fileName}...", (float)movedCount / duplicates.Count);

                    // Find which copy has the most references — that's the one to keep
                    string keepPath = null;
                    int maxRefs = -1;
                    foreach (string path in copies)
                    {
                        int refs = FindReferencesCount(path);
                        if (refs > maxRefs)
                        {
                            maxRefs = refs;
                            keepPath = path;
                        }
                    }

                    string keepGuid = AssetDatabase.AssetPathToGUID(keepPath);

                    // Remap all references from other copies to the kept copy's GUID
                    foreach (string copyPath in copies)
                    {
                        if (copyPath == keepPath) continue;

                        string copyGuid = AssetDatabase.AssetPathToGUID(copyPath);
                        int refsRemapped = RemapReferences(copyGuid, keepGuid);
                        remappedCount += refsRemapped;

                        if (refsRemapped > 0)
                        {
                            Debug.Log($"[AudioDeduplicator] Remapped {refsRemapped} references from {copyPath} -> {keepPath}");
                        }
                    }

                    // Move the kept copy to Common/
                    if (!File.Exists(targetPath))
                    {
                        string moveResult = AssetDatabase.MoveAsset(keepPath, targetPath);
                        if (!string.IsNullOrEmpty(moveResult))
                        {
                            Debug.LogWarning($"[AudioDeduplicator] Failed to move {keepPath}: {moveResult}");
                            continue;
                        }
                        movedCount++;
                    }

                    // Delete the other copies
                    foreach (string copyPath in copies)
                    {
                        if (copyPath == keepPath) continue;
                        if (AssetDatabase.DeleteAsset(copyPath))
                        {
                            deletedCount++;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[AudioDeduplicator] Done. Moved: {movedCount}, Remapped: {remappedCount} refs, Deleted: {deletedCount} copies.");
            EditorUtility.DisplayDialog("Consolidation Complete",
                $"Moved: {movedCount} files to Common/\n" +
                $"Remapped: {remappedCount} references\n" +
                $"Deleted: {deletedCount} duplicate copies\n\n" +
                "Review Addressable groups if sounds are addressable.",
                "OK");
        }

        /// <summary>
        /// Counts how many .prefab, .unity, and .asset files reference this asset's GUID.
        /// </summary>
        private static int FindReferencesCount(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) return 0;

            // Search in prefabs, scenes, and addressable group assets
            string[] searchPaths = { "Assets/_Levels", "Assets/AddressableAssetsData" };
            int count = 0;

            foreach (string searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath)) continue;

                string[] files = Directory.GetFiles(searchPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".prefab") || f.EndsWith(".unity") || f.EndsWith(".asset"))
                    .ToArray();

                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    if (content.Contains(guid))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Replaces all occurrences of oldGuid with newGuid in prefabs, scenes, and asset files.
        /// Returns the number of files modified.
        /// </summary>
        private static int RemapReferences(string oldGuid, string newGuid)
        {
            if (oldGuid == newGuid) return 0;

            string[] searchPaths = { "Assets/_Levels", "Assets/AddressableAssetsData" };
            int modifiedCount = 0;

            foreach (string searchPath in searchPaths)
            {
                if (!Directory.Exists(searchPath)) continue;

                string[] files = Directory.GetFiles(searchPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => f.EndsWith(".prefab") || f.EndsWith(".unity") || f.EndsWith(".asset"))
                    .ToArray();

                foreach (string file in files)
                {
                    string content = File.ReadAllText(file);
                    if (!content.Contains(oldGuid)) continue;

                    string newContent = content.Replace(oldGuid, newGuid);
                    File.WriteAllText(file, newContent);
                    modifiedCount++;
                }
            }

            return modifiedCount;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1048576) return $"{bytes / 1048576f:F1}MB";
            if (bytes >= 1024) return $"{bytes / 1024f:F0}KB";
            return $"{bytes}B";
        }
    }
}
