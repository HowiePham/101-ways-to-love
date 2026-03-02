using _Modules.SupportTool.Runtime.Editor;
using UnityEditor;

public class GameToolEditor
{
    [MenuItem("101WaysToLove/Level Blueprint")]
    public static void CreateLevelBlueprint(MenuCommand menuCommand)
    {
        LevelBlueprintEditorWindow.ShowWindow();
    }

    [MenuItem("101WaysToLove/Mechanic")]
    public static void CreateMechanicBlueprint(MenuCommand menuCommand)
    {
        MechanicBlueprintEditorWindow.ShowWindow();
    }

    [MenuItem("101WaysToLove/Project Structure/Level Folder")]
    public static void CreateLevelFolder(MenuCommand menuCommand)
    {
        LevelFolderCreatorWindow.ShowWindow();
    }
}