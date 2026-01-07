using UnityEditor;

public class GameBlueprintEditor
{
    [MenuItem("GameObject/Visual Actions/101WaysToLove/LevelBlueprint")]
    public static void CreateLevelBlueprint(MenuCommand menuCommand)
    {
        LevelBlueprintEditorPopup.ShowWindow();
    }

    [MenuItem("GameObject/Visual Actions/101WaysToLove/Mechanic")]
    public static void CreateMechanicBlueprint(MenuCommand menuCommand)
    {
        MechanicBlueprintEditorPopup.ShowWindow();
    }
}