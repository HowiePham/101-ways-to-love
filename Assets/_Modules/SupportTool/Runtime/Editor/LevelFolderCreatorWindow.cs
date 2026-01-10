using System.IO;
using UnityEditor;
using UnityEngine;

namespace _Modules.SupportTool.Runtime.Editor
{
    public class LevelFolderCreatorWindow : EditorWindow
    {
        private const string LevelBasePath = "Assets/_Levels";
        private string levelName;

        public static void ShowWindow()
        {
            var window = GetWindow<LevelFolderCreatorWindow>(true, "Create New Level Folder", true);
            window.minSize = new Vector2(300, 300);
            window.maxSize = new Vector2(300, 300);
            window.ShowPopup();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("LevelName (Ex: L_001, L_002,...)", EditorStyles.boldLabel);
            this.levelName = EditorGUILayout.TextField(this.levelName);

            EditorGUILayout.Space();
            DrawButton("Create", OnMenuItemClicked);
        }

        private void OnMenuItemClicked()
        {
            CreateLevelFolderStructure();
        }

        private void CreateLevelFolderStructure()
        {
            if (string.IsNullOrEmpty(this.levelName))
            {
                Debug.LogError($"You have to name this Level");
                return;
            }

            string rootPath = Path.Combine(LevelBasePath, this.levelName);

            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                AssetDatabase.CreateFolder(LevelBasePath, this.levelName);
            }

            CreateSub(rootPath, "Textures");
            CreateSub(rootPath, "Prefabs");
            CreateSub(rootPath, "Animations");

            AssetDatabase.Refresh();
            Debug.Log("Folder structure created!");
        }

        private static void CreateSub(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(Path.Combine(parent, name)))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private void DrawButton(string label, System.Action onClick)
        {
            bool clicked = GUILayout.Button(label);

            if (clicked)
            {
                onClick?.Invoke();
                Close();
            }
        }
    }
}