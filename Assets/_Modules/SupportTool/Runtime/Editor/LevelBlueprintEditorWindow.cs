using UnityEditor;
using UnityEngine;

public class LevelBlueprintEditorWindow : EditorWindow
{
    private const string LevelBlueprintAddress = "Assets/_Modules/Game/_Shared/Prefabs/LevelBlueprint/";
    private string levelName = "";

    public static void ShowWindow()
    {
        var window = GetWindow<LevelBlueprintEditorWindow>(true, "Select Level Blueprint", true);
        window.minSize = new Vector2(500, 300);
        window.maxSize = new Vector2(5000, 3000);
        window.ShowPopup();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("LevelName (Ex: L_001, L_002,...)", EditorStyles.boldLabel);
        this.levelName = EditorGUILayout.TextField(this.levelName);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Choose Level Blueprint:", EditorStyles.boldLabel);
        DrawButton("E1, E2, M2, M3", () => OnMenuItemClicked("E1"));
        DrawButton("M1, H1, H2", () => OnMenuItemClicked("M1"));
        DrawButton("M4, H3", () => OnMenuItemClicked("M4"));
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

    private void OnMenuItemClicked(string menuName)
    {
        if (string.IsNullOrEmpty(this.levelName))
        {
            Debug.LogError($"You have to name this Level");
            return;
        }

        var blueprintTemplate = AssetDatabase.LoadAssetAtPath<GameObject>($"{LevelBlueprintAddress}{menuName}.prefab");

        if (blueprintTemplate == null)
        {
            Debug.LogError($"There is no Blueprint: {menuName}");
            return;
        }

        Debug.Log($"Creating Level Blueprint: {menuName} --- {this.levelName}");
        var blueprintObject = (GameObject)PrefabUtility.InstantiatePrefab(blueprintTemplate);
        blueprintObject.name = $"{this.levelName}";
    }
}