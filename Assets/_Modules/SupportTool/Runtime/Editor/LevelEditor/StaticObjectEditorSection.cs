using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StaticObjectEditorSection : EditorSection
{
    public StaticObjectEditorSection(LevelEditor levelEditor) : base(levelEditor)
    {
    }
    
    public override void DrawSection()
    {
        List<GameObject> staticObjects = this.levelEditor.StaticObjects;
        if (staticObjects == null || staticObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < staticObjects.Count; i++)
        {
            GameObject staticObject = staticObjects[i];
            if (staticObject == null) continue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(staticObject.gameObject.name);

            if (!newName.Equals(staticObject.gameObject.name))
            {
                Undo.RecordObject(staticObject.gameObject, "Change Name");
                staticObject.gameObject.name = newName;
                EditorUtility.SetDirty(staticObject.gameObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = staticObject.gameObject;
                EditorGUIUtility.PingObject(staticObject.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            DrawObjectRenderersSection(staticObject);

            EditorGUILayout.Space(10);
        }

        EditorGUI.indentLevel--;
    }
}