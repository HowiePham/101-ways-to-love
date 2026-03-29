using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AddressableOptimizer
    {
        [MenuItem("Tools/Optimization/Clean Addressable Duplicates")]
        public static void CleanAddressableDuplicates()
        {
            string sharedAssetPath = "Assets/AddressableAssetsData/AssetGroups/_Shared.asset";
            if (!File.Exists(sharedAssetPath))
            {
                Debug.LogWarning("[AddressableOptimizer] _Shared.asset not found.");
                return;
            }

            string content = File.ReadAllText(sharedAssetPath);
            string[] lines = content.Split('\n');

            // Track seen addresses (not just GUIDs) to catch address-level duplicates
            var seenAddresses = new HashSet<string>();
            var outputLines = new List<string>();
            int removedCount = 0;
            bool skipCurrentEntry = false;
            var entryBuffer = new List<string>();
            string currentAddress = null;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.TrimStart().StartsWith("- m_GUID:"))
                {
                    // Flush previous entry
                    if (entryBuffer.Count > 0)
                    {
                        if (!skipCurrentEntry)
                            outputLines.AddRange(entryBuffer);
                        else
                            removedCount++;
                    }

                    entryBuffer = new List<string> { line };
                    currentAddress = null;
                    skipCurrentEntry = false;

                    // Also check GUID-level duplicates
                    string guid = line.Split(':').Last().Trim();
                    // We'll determine skip based on address (checked when we see m_Address line)
                }
                else if (entryBuffer.Count > 0 && line.Contains("m_Address:"))
                {
                    entryBuffer.Add(line);
                    currentAddress = line.Split(new[] { "m_Address:" }, System.StringSplitOptions.None)
                        .Last().Trim();

                    if (!string.IsNullOrEmpty(currentAddress))
                    {
                        if (seenAddresses.Contains(currentAddress))
                        {
                            skipCurrentEntry = true;
                        }
                        else
                        {
                            seenAddresses.Add(currentAddress);
                        }
                    }
                }
                else if (entryBuffer.Count > 0 &&
                         !line.TrimStart().StartsWith("- m_GUID:") &&
                         (line.StartsWith("    ") || line.Trim().Length == 0) &&
                         !line.TrimStart().StartsWith("m_") && !line.Contains("m_SchemaSet:"))
                {
                    entryBuffer.Add(line);
                }
                else
                {
                    if (entryBuffer.Count > 0)
                    {
                        if (!skipCurrentEntry)
                            outputLines.AddRange(entryBuffer);
                        else
                            removedCount++;
                        entryBuffer.Clear();
                        skipCurrentEntry = false;
                    }
                    outputLines.Add(line);
                }
            }

            // Flush last entry
            if (entryBuffer.Count > 0)
            {
                if (!skipCurrentEntry)
                    outputLines.AddRange(entryBuffer);
                else
                    removedCount++;
            }

            if (removedCount > 0)
            {
                File.WriteAllText(sharedAssetPath, string.Join("\n", outputLines));
                AssetDatabase.Refresh();
            }

            int finalCount = seenAddresses.Count;
            Debug.Log($"[AddressableOptimizer] Removed {removedCount} duplicate entries. " +
                      $"Remaining: {finalCount} unique entries.");

            EditorUtility.DisplayDialog("Addressable Cleanup",
                $"Removed: {removedCount} duplicate entries\n" +
                $"Remaining: {finalCount} unique entries",
                "OK");
        }

        [MenuItem("Tools/Optimization/Disable Empty Dev Groups")]
        public static void DisableEmptyDevGroups()
        {
            string[] devGroupNames =
            {
                "L_Demo", "L_DemoNewEditor", "Level_DemoNewEditor",
                "L_Dev_001", "L_Dev_002", "L_Dev_003", "L_Dev_004",
                "L_71"
            };

            string schemasDir = "Assets/AddressableAssetsData/AssetGroups/Schemas";
            int disabledCount = 0;

            foreach (string groupName in devGroupNames)
            {
                string schemaPath = $"{schemasDir}/{groupName}_BundledAssetGroupSchema.asset";
                if (!File.Exists(schemaPath)) continue;

                string content = File.ReadAllText(schemaPath);
                if (content.Contains("m_IncludeInBuild: 1"))
                {
                    content = content.Replace("m_IncludeInBuild: 1", "m_IncludeInBuild: 0");
                    File.WriteAllText(schemaPath, content);
                    disabledCount++;
                    Debug.Log($"[AddressableOptimizer] Disabled: {groupName}");
                }
            }

            if (disabledCount > 0)
                AssetDatabase.Refresh();

            Debug.Log($"[AddressableOptimizer] Disabled {disabledCount} empty dev/demo groups from builds.");
            EditorUtility.DisplayDialog("Dev Groups Disabled",
                $"Disabled {disabledCount} groups from builds.\n\n" +
                "These empty groups will no longer be included in Addressable builds.",
                "OK");
        }
    }
}
