using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Editor{
    [CreateAssetMenu(menuName = "Mimi/VisualActions/Generate/Editor Config")]
    public class ConfigGenerateSO : ScriptableObject
    {
        private static ConfigGenerateSO _instance;
        public static ConfigGenerateSO Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    // Tìm trong Editor (Assets, Packages,…)
                    string[] guids = AssetDatabase.FindAssets($"t:{typeof(ConfigGenerateSO).Name}");
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        _instance = AssetDatabase.LoadAssetAtPath<ConfigGenerateSO>(path);
                    }
#endif
                    if (_instance == null)
                        Debug.LogError($"Singleton ScriptableObject of type {typeof(ConfigGenerateSO).Name} not found!");
                }

                return _instance;
            }
        }
        public string rootPath;
        public string defaultRootPath = "Packages/VisualActions.Generate";

        // Nút trong Inspector + Menu Item
        [Button]
        public void GenerateAllPrefabVariants()
        {
            if (this != Instance)
            {
                Debug.LogError("Chỉ chạy từ Instance hợp lệ!");
                return;
            }

            if (string.IsNullOrEmpty(defaultRootPath) || string.IsNullOrEmpty(rootPath))
            {
                Debug.LogError("Vui lòng thiết lập defaultRootPath và rootPath!");
                return;
            }

            /*if (!AssetDatabase.IsValidFolder(defaultRootPath))
            {
                Debug.LogError($"Thư mục nguồn không tồn tại: {defaultRootPath}");
                return;
            }*/

            CreateDirectoryIfNotExists(rootPath);

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { defaultRootPath });
            int count = 0;

            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                string relativePath = assetPath.Substring(defaultRootPath.Length + 1);
                string variantPath = Path.Combine(rootPath, relativePath).Replace("\\", "/");
                string variantDir = Path.GetDirectoryName(variantPath).Replace("\\", "/");

                CreateDirectoryIfNotExists(variantDir);

                if (AssetDatabase.LoadAssetAtPath<GameObject>(variantPath) != null)
                {
                    Debug.Log($"[SKIP] Đã tồn tại: {variantPath}");
                    continue;
                }

                // 1. Instantiate prefab gốc
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    Debug.LogError($"[FAILED] Không thể instantiate: {assetPath}");
                    continue;
                }

// 2. (Tùy chọn) Thêm override nếu muốn
// Ví dụ: Đổi tên để phân biệt
                instance.name = prefab.name + " (Variant)";

// 3. Lưu thành Prefab Variant
                GameObject variant = PrefabUtility.SaveAsPrefabAsset(instance, variantPath, out bool success);

// 4. Xóa instance khỏi scene
                Object.DestroyImmediate(instance);
                if (variant != null && success)
                {
                    count++;
                    Debug.Log($"[CREATED] {variantPath}");
                }
                else
                {
                    Debug.LogError($"[FAILED] Không thể tạo variant: {variantPath}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Hoàn tất! Đã tạo {count} Prefab Variant.");
            EditorUtility.DisplayDialog("Thành công", $"Đã tạo {count} Prefab Variant!", "OK");
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                CreateDirectoryIfNotExists(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
        
    }
}