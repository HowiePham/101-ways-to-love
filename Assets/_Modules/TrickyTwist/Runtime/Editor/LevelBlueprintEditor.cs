using UnityEditor;

public class LevelBlueprintEditor
{
    [MenuItem("GameObject/Visual Actions/101WaysToLove/LevelBlueprint")]
    public static void CreateLevelBlueprint(MenuCommand menuCommand)
    {
        LevelBlueprintEditorPopup.ShowWindow();
    }
}