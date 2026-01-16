using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditor))]
public class LevelEditorCustomEditor : Editor
{
    private LevelEditorWindow levelEditorWindow;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var levelEditor = (LevelEditor)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Open Level Editor", GUILayout.Height(25)))
        {
            levelEditor.PrepareData();

            if (this.levelEditorWindow == null)
            {
                this.levelEditorWindow = CreateInstance<LevelEditorWindow>();
            }

            this.levelEditorWindow.Initialize(levelEditor);
            this.levelEditorWindow.ShowWindow();
        }
    }
}