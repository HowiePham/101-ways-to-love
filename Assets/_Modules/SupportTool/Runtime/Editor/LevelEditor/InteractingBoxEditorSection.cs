using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

public class InteractingBoxEditorSection : EditorSection
{
    public InteractingBoxEditorSection(LevelEditor levelEditor) : base(levelEditor)
    {
    }
    
     public override void DrawSection()
    {
        InteractingBox[] interactingBoxes = this.levelEditor.InteractingBoxes;
        if (interactingBoxes == null || interactingBoxes.Length == 0)
        {
            EditorGUILayout.HelpBox("No boxes found", MessageType.Info);
            return;
        }

        EditorGUI.indentLevel++;

        for (var i = 0; i < interactingBoxes.Length; i++)
        {
            InteractingBox interactingBox = interactingBoxes[i];
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{i + 1}. {interactingBox.gameObject.name}", EditorStyles.boldLabel);

            if (GUILayout.Button("Select in Hierarchy", GUILayout.Width(150)))
            {
                Selection.activeGameObject = interactingBox.gameObject;
                EditorGUIUtility.PingObject(interactingBox.gameObject);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            bool followTarget = EditorGUILayout.Toggle($"Box Following Object ", interactingBox.FollowTarget);
            if (followTarget != interactingBox.FollowTarget)
            {
                Undo.RecordObject(interactingBox, "Change Following toggle");
                interactingBox.FollowTarget = followTarget;
                EditorUtility.SetDirty(interactingBox);
            }

            if (interactingBox.FollowTarget)
            {
                var target = (Transform)EditorGUILayout.ObjectField(
                    interactingBox.Target,
                    typeof(Transform),
                    true,
                    GUILayout.Height(18)
                );

                if (target != interactingBox.Target)
                {
                    Undo.RecordObject(interactingBox, "Change Target transform");
                    interactingBox.Target = target;
                    EditorUtility.SetDirty(interactingBox);
                }
            }

            EditorGUILayout.EndHorizontal();

            var boxArea = interactingBox.gameObject.GetComponent<BoxArea>();
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

            EditorGUILayout.Space(10);
        }

        EditorGUI.indentLevel--;
    }
}