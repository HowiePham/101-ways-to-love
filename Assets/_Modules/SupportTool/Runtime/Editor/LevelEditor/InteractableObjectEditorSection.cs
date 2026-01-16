using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class InteractableObjectEditorSection : EditorSection
{
    public InteractableObjectEditorSection(LevelEditor levelEditor) : base(levelEditor)
    {
    }

    public override void DrawSection()
    {
        List<GameObject> interactableObjects = this.levelEditor.InteractableObjects;

        if (interactableObjects == null || interactableObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("No objects found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (int i = 0; i < interactableObjects.Count; i++)
        {
            GameObject interactableObject = interactableObjects[i];
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"{i + 1}. ", EditorStyles.boldLabel, GUILayout.Width(25));
            string newName = EditorGUILayout.TextField(interactableObject.name);

            if (!newName.Equals(interactableObject.name))
            {
                Undo.RecordObject(interactableObject, "Change Name");
                interactableObject.name = newName;
                EditorUtility.SetDirty(interactableObject);
            }

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = interactableObject;
                EditorGUIUtility.PingObject(interactableObject);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            var objectHighlight = interactableObject.GetComponent<ScaleObjectHighlight>();
            var highlightToggle = EditorGUILayout.Toggle($"Highlighted Object ", objectHighlight.EnableHighlight);

            if (highlightToggle != objectHighlight.EnableHighlight)
            {
                Undo.RecordObject(objectHighlight, "Change Highlight toggle");
                objectHighlight.EnableHighlight = highlightToggle;
                EditorUtility.SetDirty(objectHighlight);
            }

            EditorGUILayout.EndHorizontal();

            var boxArea = interactableObject.GetComponent<BoxArea>();
            if (boxArea != null)
            {
                EditorGUILayout.BeginHorizontal();
                Vector2 newSize = EditorGUILayout.Vector2Field("Box Size:", boxArea.Size);

                if (newSize != boxArea.Size)
                {
                    Undo.RecordObject(boxArea, "Change Box Size");
                    boxArea.SetSizeEditor(newSize);
                    EditorUtility.SetDirty(boxArea);
                }

                EditorGUILayout.EndHorizontal();
            }

            DrawObjectRenderersSection(interactableObject);

            EditorGUILayout.Space(10);
        }

        EditorGUI.indentLevel--;
    }
}