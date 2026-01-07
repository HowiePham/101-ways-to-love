using System;
using System.Collections.Generic;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions.ControlFlow;
using Mimi.VisualActions.Tapping;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

[InitializeOnLoad]
public class HierarchyComponentDisplay
{
    private static Dictionary<Type, string> componentLabels = new Dictionary<Type, string>
    {
        { typeof(VisualSequence), "Flow" },
        { typeof(VisualParallel), "Flow" },
        { typeof(BaseDraggable), "Mechanic" },
        { typeof(TapArea), "Mechanic" },
        { typeof(BaseArea), "Area" },
    };

    private static Dictionary<string, Color> componentLabelColors = new Dictionary<string, Color>
    {
        { "Flow", Color.green },
        { "Mechanic", Color.yellow },
        { "Area", Color.cyan },
    };

    static HierarchyComponentDisplay()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (obj == null) return;

        var labelsToShow = new List<string>();

        foreach (var kvp in componentLabels)
        {
            var component = obj.GetComponent(kvp.Key);
            if (component != null)
            {
                labelsToShow.Add(kvp.Value);
            }
        }

        if (labelsToShow.Count == 0) return;

        float xOffset = 8;

        for (int i = labelsToShow.Count - 1; i >= 0; i--)
        {
            var style = new GUIStyle(EditorStyles.label);
            style.fontSize = 11;
            style.normal.textColor = componentLabelColors[labelsToShow[i]];
            style.padding = new RectOffset(5, 5, 2, 2);

            var labelText = labelsToShow[i];
            var labelContent = new GUIContent(labelText);
            var labelSize = style.CalcSize(labelContent);

            var labelRect = new Rect(
                selectionRect.xMax - labelSize.x - xOffset,
                selectionRect.y + (selectionRect.height - labelSize.y) / 2,
                labelSize.x,
                labelSize.y
            );

            EditorGUI.DrawRect(labelRect, new Color(0.25f, 0.25f, 0.25f, 1f));
            GUI.Label(labelRect, labelContent, style);
            xOffset += labelSize.x + 4;
        }
    }
}