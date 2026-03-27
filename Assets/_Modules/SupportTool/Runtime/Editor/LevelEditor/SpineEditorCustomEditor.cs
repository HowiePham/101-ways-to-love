using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpineAnimationEditorPlayer))]
public class SpineEditorCustomEditor : UnityEditor.Editor
{
    private SpineEditorWindow spineEditorWindow;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var spineEditor = (SpineAnimationEditorPlayer)this.target;

        GUILayout.Space(10);

        if (GUILayout.Button("Open Spine Editor", GUILayout.Height(25)))
        {
            // levelEditor.PrepareData();

            if (this.spineEditorWindow == null)
            {
                this.spineEditorWindow = CreateInstance<SpineEditorWindow>();
            }

            this.spineEditorWindow.Initialize(spineEditor);
            this.spineEditorWindow.ShowWindow();
        }
    }
}