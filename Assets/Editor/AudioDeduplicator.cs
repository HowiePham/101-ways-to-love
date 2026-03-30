using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Safely deduplicates audio files across level folders.
    ///
    /// The problem:
    /// - Same audio file is copied into multiple L_xxx/ folders (e.g., SFX_ThaTim.mp3 x14)
    /// - Each copy has its own GUID
    /// - GUIDs are referenced from: level prefabs (direct AudioClip fields), Addressable groups,
    ///   MasterAudio prefab, scenes, and other .asset files
    /// - Some copies are heavily referenced (one GUID used by 50+ prefabs), others only in Addressables
    /// - Addressable _Shared group has duplicate entries (14 entries with address "SFX_ThaTim")
    ///
    /// The solution:
    /// 1. Group duplicate files by MD5 hash (handles mixed-content sets)
    /// 2. For each group of 2+ identical files, find the most-referenced GUID
    /// 3. Remap ALL references from other GUIDs to the primary GUID across ALL project files
    /// 4. Remove duplicate Addressable entries (keep only one per address)
    /// 5. Delete the unused copies
    /// 6. Move the primary copy to Common/ folder
    /// </summary>
    public static class AudioDeduplicator
    {
        // Search the entire Assets folder for GUID references
        private const string SearchRoot = "Assets";

        [MenuItem("Tools/Optimization/1. Find Duplicate Audio (Report Only)")]
        public static void FindDuplicateAudio()
        {
            var duplicates = CollectDuplicates();
            if (duplicates.Count == 0)
            {
                Debug.Log("[AudioDeduplicator] No duplicate audio files found.");
                EditorUtility.DisplayDialog("Audio Deduplication", "No duplicates found.", "OK");
                return;
            }

            // Build file index once for performance
            string[] allProjectFiles = GetAllSerializedFiles();

            int totalDuplicateFiles = duplicates.Sum(d => d.Value.Count - 1);
            long totalWastedBytes = 0;
            int consolidatableSetCount = 0;
            int uniqueOnlyCount = 0;

            Debug.Log("========== DUPLICATE AUDIO REPORT ==========");

            foreach (var kvp in duplicates)
            {
                List<string> copies = kvp.Value;
                long fileSize = new FileInfo(copies[0]).Length;
                long wastedBytes = fileSize * (copies.Count - 1);
                totalWastedBytes += wastedBytes;

                // Group by content hash
                var hashGroups = GroupByContent(copies);
                bool hasConsolidatable = hashGroups.Any(g => g.Value.Count > 1);
                if (hasConsolidatable) consolidatableSetCount++;
                else uniqueOnlyCount++;

                string groupLabel = hashGroups.Count == 1
                    ? "ALL IDENTICAL"
                    : $"{hashGroups.Count} variants";

                Debug.Log($"<b>{kvp.Key}</b> ({copies.Count} copies, {FormatSize(fileSize)} each, " +
                          $"{FormatSize(wastedBytes)} wasted) [{groupLabel}]");

                foreach (var hashGroup in hashGroups)
                {
                    if (hashGroups.Count > 1)
                    {
                        string hashLabel = hashGroup.Value.Count > 1
                            ? $"IDENTICAL x{hashGroup.Value.Count} (will consolidate)"
                            : "UNIQUE (will keep)";
                        Debug.Log($"  Hash {hashGroup.Key.Substring(0, 8)}: {hashLabel}");
                    }

                    foreach (string path in hashGroup.Value)
                    {
                        string guid = AssetDatabase.AssetPathToGUID(path);
                        int refCount = CountGuidReferences(guid, allProjectFiles);
                        string refLabel = refCount > 0 ? $"REFERENCED x{refCount}" : "unreferenced";
                        Debug.Log($"    - {path}  [{refLabel}]  guid:{guid}");
                    }
                }
            }

            Debug.Log($"\n=== SUMMARY ===");
            Debug.Log($"Duplicated filenames: {duplicates.Count}");
            Debug.Log($"  With consolidatable groups: {consolidatableSetCount}");
            Debug.Log($"  All unique content (no consolidation possible): {uniqueOnlyCount}");
            Debug.Log($"Extra copies: {totalDuplicateFiles}");
            Debug.Log($"Wasted space: {FormatSize(totalWastedBytes)}");

            // Check Addressable duplicate entries
            string sharedAsset = "Assets/AddressableAssetsData/AssetGroups/_Shared.asset";
            if (File.Exists(sharedAsset))
            {
                string content = File.ReadAllText(sharedAsset);
                var addressLines = content.Split('\n')
                    .Where(l => l.Contains("m_Address:"))
                    .Select(l => l.Trim())
                    .ToArray();
                int totalEntries = addressLines.Length;
                int uniqueEntries = addressLines.Distinct().Count();
                int duplicateEntries = totalEntries - uniqueEntries;
                Debug.Log($"Addressable _Shared group: {totalEntries} entries, {uniqueEntries} unique, " +
                          $"{duplicateEntries} duplicates to clean");
            }

            EditorUtility.DisplayDialog("Audio Deduplication Report",
                $"Found {duplicates.Count} duplicated audio files\n" +
                $"With consolidatable groups: {consolidatableSetCount}\n" +
                $"All unique (skip): {uniqueOnlyCount}\n" +
                $"Extra copies: {totalDuplicateFiles}\n" +
                $"Wasted: {FormatSize(totalWastedBytes)}\n\n" +
                "See Console for full report.\n" +
                "Use 'Consolidate Duplicates' to fix.",
                "OK");
        }

        [MenuItem("Tools/Optimization/2. Consolidate Duplicate Audio")]
        public static void ConsolidateDuplicateAudio()
        {
            string commonPath = "Assets/_Levels/_Shared/Sounds/Common";
            var duplicates = CollectDuplicates();

            if (duplicates.Count == 0)
            {
                Debug.Log("[AudioDeduplicator] No duplicates to consolidate.");
                return;
            }

            // Build consolidation work items: for each filename, find groups of identical files with 2+ copies
            var workItems = new List<ConsolidationItem>();
            int skippedUniqueCount = 0;

            foreach (var kvp in duplicates)
            {
                var hashGroups = GroupByContent(kvp.Value);
                foreach (var hashGroup in hashGroups)
                {
                    if (hashGroup.Value.Count > 1)
                    {
                        workItems.Add(new ConsolidationItem
                        {
                            FileName = kvp.Key,
                            Copies = hashGroup.Value,
                            Hash = hashGroup.Key
                        });
                    }
                    else
                    {
                        skippedUniqueCount++;
                    }
                }
            }

            if (workItems.Count == 0)
            {
                Debug.Log("[AudioDeduplicator] No identical duplicate groups found to consolidate.");
                EditorUtility.DisplayDialog("No Consolidation Needed",
                    "All duplicate filenames have different content — nothing to consolidate.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Consolidate Duplicates",
                    $"Found {workItems.Count} identical groups to consolidate.\n" +
                    $"Unique copies to keep in place: {skippedUniqueCount}\n\n" +
                    "For each identical group:\n" +
                    "1. Finds the most-referenced copy across ALL project files\n" +
                    "2. Remaps ALL GUIDs from other copies to the kept copy\n" +
                    "3. Removes duplicate Addressable entries\n" +
                    "4. Deletes extra copies\n" +
                    "5. Moves kept copy to Common/\n\n" +
                    "IMPORTANT: Commit your changes first!",
                    "Proceed", "Cancel"))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(commonPath))
            {
                AssetDatabase.CreateFolder("Assets/_Levels/_Shared/Sounds", "Common");
            }

            // Build file index once
            string[] allProjectFiles = GetAllSerializedFiles();

            int movedCount = 0;
            int remappedFileCount = 0;
            int deletedCount = 0;
            int addressableEntriesCleaned = 0;

            try
            {
                for (int i = 0; i < workItems.Count; i++)
                {
                    var item = workItems[i];
                    string fileName = item.FileName;
                    List<string> copies = item.Copies;

                    EditorUtility.DisplayProgressBar("Consolidating Audio",
                        $"Processing {fileName} ({i + 1}/{workItems.Count})...",
                        (float)(i + 1) / workItems.Count);

                    // Step 1: Find the most-referenced copy
                    string keepPath = null;
                    string keepGuid = null;
                    int maxRefs = -1;

                    foreach (string path in copies)
                    {
                        string guid = AssetDatabase.AssetPathToGUID(path);
                        int refs = CountGuidReferences(guid, allProjectFiles);
                        if (refs > maxRefs)
                        {
                            maxRefs = refs;
                            keepPath = path;
                            keepGuid = guid;
                        }
                    }

                    // Step 2: Remap all other GUIDs to the kept GUID
                    var otherGuids = copies
                        .Where(p => p != keepPath)
                        .Select(p => AssetDatabase.AssetPathToGUID(p))
                        .Where(g => !string.IsNullOrEmpty(g))
                        .ToList();

                    foreach (string projectFile in allProjectFiles)
                    {
                        string content = File.ReadAllText(projectFile);
                        bool modified = false;

                        foreach (string oldGuid in otherGuids)
                        {
                            if (content.Contains(oldGuid))
                            {
                                content = content.Replace(oldGuid, keepGuid);
                                modified = true;
                            }
                        }

                        if (modified)
                        {
                            File.WriteAllText(projectFile, content);
                            remappedFileCount++;
                        }
                    }

                    // Step 3: Move the kept copy to Common/
                    string targetPath = $"{commonPath}/{fileName}";
                    // If a file with the same name already exists in Common (from a different hash group),
                    // keep it in place — the first group already claimed that filename
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

                    // Step 4: Delete extra copies
                    foreach (string copyPath in copies)
                    {
                        if (copyPath == keepPath) continue;
                        if (AssetDatabase.DeleteAsset(copyPath))
                        {
                            deletedCount++;
                        }
                    }

                    Debug.Log($"[AudioDeduplicator] {fileName} (hash:{item.Hash.Substring(0, 8)}): " +
                              $"kept {keepPath} (refs:{maxRefs}), " +
                              $"remapped {otherGuids.Count} GUIDs, deleted {copies.Count - 1} copies");
                }

                // Step 5: Clean duplicate Addressable entries in _Shared.asset
                EditorUtility.DisplayProgressBar("Consolidating Audio",
                    "Cleaning Addressable group entries...", 1f);
                addressableEntriesCleaned = CleanAddressableEntries();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"\n=== CONSOLIDATION COMPLETE ===");
            Debug.Log($"Moved to Common/: {movedCount}");
            Debug.Log($"Files with remapped GUIDs: {remappedFileCount}");
            Debug.Log($"Deleted copies: {deletedCount}");
            Debug.Log($"Addressable entries cleaned: {addressableEntriesCleaned}");
            if (skippedUniqueCount > 0)
                Debug.Log($"Unique copies kept in place: {skippedUniqueCount}");

            EditorUtility.DisplayDialog("Consolidation Complete",
                $"Moved to Common/: {movedCount}\n" +
                $"Files with remapped GUIDs: {remappedFileCount}\n" +
                $"Deleted copies: {deletedCount}\n" +
                $"Addressable entries cleaned: {addressableEntriesCleaned}\n" +
                (skippedUniqueCount > 0 ? $"\nUnique copies kept in place: {skippedUniqueCount}" : ""),
                "OK");
        }

        /// <summary>
        /// Removes duplicate entries from the _Shared Addressable group.
        /// After GUID remapping, entries that had different GUIDs but same address
        /// now have the SAME GUID and same address — pure duplicates.
        /// </summary>
        private static int CleanAddressableEntries()
        {
            string sharedAssetPath = "Assets/AddressableAssetsData/AssetGroups/_Shared.asset";
            if (!File.Exists(sharedAssetPath)) return 0;

            string content = File.ReadAllText(sharedAssetPath);
            string[] lines = content.Split('\n');

            // Parse entries: each entry is "  - m_GUID: xxx\n    m_Address: yyy\n    ..."
            // We need to find blocks starting with "  - m_GUID:" and remove duplicate GUID entries
            var seenGuids = new HashSet<string>();
            var outputLines = new List<string>();
            int removedCount = 0;
            bool skipCurrentEntry = false;
            var entryBuffer = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // Detect start of a new entry in the m_SerializedEntries list
                if (line.TrimStart().StartsWith("- m_GUID:"))
                {
                    // Flush previous entry if not skipped
                    if (entryBuffer.Count > 0)
                    {
                        if (!skipCurrentEntry)
                        {
                            outputLines.AddRange(entryBuffer);
                        }
                        else
                        {
                            removedCount++;
                        }
                    }

                    // Start new entry
                    entryBuffer = new List<string> { line };
                    string guid = line.Split(':').Last().Trim();
                    skipCurrentEntry = seenGuids.Contains(guid);
                    seenGuids.Add(guid);
                }
                else if (entryBuffer.Count > 0 && !line.TrimStart().StartsWith("- m_GUID:") &&
                         (line.StartsWith("    ") || line.Trim().Length == 0) &&
                         !line.TrimStart().StartsWith("m_") && !line.Contains("m_SchemaSet:"))
                {
                    // Continuation of current entry
                    entryBuffer.Add(line);
                }
                else
                {
                    // Not part of an entry — flush and output directly
                    if (entryBuffer.Count > 0)
                    {
                        if (!skipCurrentEntry)
                        {
                            outputLines.AddRange(entryBuffer);
                        }
                        else
                        {
                            removedCount++;
                        }
                        entryBuffer.Clear();
                        skipCurrentEntry = false;
                    }
                    outputLines.Add(line);
                }
            }

            // Flush last entry
            if (entryBuffer.Count > 0 && !skipCurrentEntry)
            {
                outputLines.AddRange(entryBuffer);
            }
            else if (entryBuffer.Count > 0)
            {
                removedCount++;
            }

            if (removedCount > 0)
            {
                File.WriteAllText(sharedAssetPath, string.Join("\n", outputLines));
                Debug.Log($"[AudioDeduplicator] Removed {removedCount} duplicate entries from _Shared.asset");
            }

            return removedCount;
        }

        private static Dictionary<string, List<string>> CollectDuplicates()
        {
            string basePath = "Assets/_Levels/_Shared/Sounds";
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { basePath });

            var fileGroups = new Dictionary<string, List<string>>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/L_")) continue;

                string fileName = Path.GetFileName(path);
                if (!fileGroups.ContainsKey(fileName))
                    fileGroups[fileName] = new List<string>();
                fileGroups[fileName].Add(path);
            }

            return fileGroups
                .Where(kvp => kvp.Value.Count > 1)
                .OrderByDescending(kvp => kvp.Value.Count)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Groups files by their MD5 content hash.
        /// Returns a dictionary of hash → list of paths with that hash.
        /// </summary>
        private static Dictionary<string, List<string>> GroupByContent(List<string> paths)
        {
            var groups = new Dictionary<string, List<string>>();
            foreach (string path in paths)
            {
                string hash = ComputeFileHash(path);
                if (!groups.ContainsKey(hash))
                    groups[hash] = new List<string>();
                groups[hash].Add(path);
            }
            return groups;
        }

        private static string ComputeFileHash(string path)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(path))
            {
                byte[] hash = md5.ComputeHash(stream);
                return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Gets all serialized files (.prefab, .unity, .asset) across the entire project.
        /// Cached for performance when processing multiple duplicates.
        /// </summary>
        private static string[] GetAllSerializedFiles()
        {
            return Directory.GetFiles(SearchRoot, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".prefab") || f.EndsWith(".unity") || f.EndsWith(".asset"))
                .Where(f => !f.Contains("Library"))
                .ToArray();
        }

        private static int CountGuidReferences(string guid, string[] files)
        {
            if (string.IsNullOrEmpty(guid)) return 0;
            return files.Count(f => File.ReadAllText(f).Contains(guid));
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1048576) return $"{bytes / 1048576f:F1}MB";
            if (bytes >= 1024) return $"{bytes / 1024f:F0}KB";
            return $"{bytes}B";
        }

        private struct ConsolidationItem
        {
            public string FileName;
            public List<string> Copies;
            public string Hash;
        }
    }
}
