using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditors.Utils;

namespace Mimi.VisualActions.Editor
{
    [InitializeOnLoad]
    public static class VisualActionStateVisualizer
    {
        private static Texture2D visualActionIcon;
        private static readonly Color RunningActionBackgroundColor = new(0.01f, 0.4f, 0.25f);
        private static readonly Color CompletedActionBackgroundColor = Color.grey;
        private static readonly Color NotStartedActionBackgroundColor = new(0f, 0f, 0f, 0f);
        private static readonly Color ErrorActionBackgroundColor = new(1f, 0f, 0f);

        private static readonly GUIStyle NotStartedActionNameStyle = new()
        {
            normal = new GUIStyleState() {textColor = Color.white},
            fontStyle = FontStyle.Normal
        };

        private static readonly GUIStyle RunningActionNameStyle = new()
        {
            normal = new GUIStyleState() {textColor = Color.white},
            fontStyle = FontStyle.Bold
        };

        private static readonly GUIStyle CompletedActionNameStyle = new()
        {
            normal = new GUIStyleState() {textColor = Color.white},
            fontStyle = FontStyle.Italic
        };

        private static readonly GUIStyle NormalActionNameStyle = new()
        {
            normal = new GUIStyleState() {textColor = Color.cyan},
            fontStyle = FontStyle.Normal
        };

        private static readonly Dictionary<Type, GUIStyle> customActionNameStyles;

        static VisualActionStateVisualizer()
        {
            visualActionIcon = AssetDatabaseUtils.GetAssetOfType<Texture2D>("VisualActionIcon", "png");
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            customActionNameStyles = new Dictionary<Type, GUIStyle>();
        }

        public static void RegisterActionNameStyle(Type actionType, GUIStyle nameGUIStyle)
        {
            if (customActionNameStyles.ContainsKey(actionType)) return;
            customActionNameStyles.Add(actionType, nameGUIStyle);
        }

        public static void RegisterActionNameStyle(Type actionType, FontStyle fontStyle, Color color)
        {
            if (customActionNameStyles.ContainsKey(actionType)) return;
            customActionNameStyles.Add(actionType, new GUIStyle
            {
                normal = new GUIStyleState() {textColor = color},
                fontStyle = fontStyle
            });
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;
            var visualAction = gameObject.GetComponent<VisualAction>();
            if (visualAction == null) return;
            DrawVisualActionIcon(selectionRect);
            
            if (Application.isPlaying)
            {
                DrawRuntimeVisualActionState(gameObject, visualAction, selectionRect);
            }
            else
            {
                DrawEditModeVisualActionState(gameObject, visualAction, selectionRect);
            }
        }

        private static void DrawRuntimeVisualActionState(GameObject obj, VisualAction visualAction, Rect selectionRect)
        {
            if (visualAction.IsExecuting)
            {
                DrawActionState(obj.name, selectionRect, RunningActionNameStyle, RunningActionBackgroundColor);
            }
            else if (visualAction.Completed)
            {
                DrawActionState(obj.name, selectionRect, CompletedActionNameStyle, CompletedActionBackgroundColor);
            }
            else
            {
                DrawActionState(obj.name, selectionRect, NotStartedActionNameStyle, NotStartedActionBackgroundColor);
            }

            if (visualAction.HasError)
            {
                if (!obj.name.Contains(visualAction.ErrorMessage))
                {
                    obj.name = obj.name + ": " + visualAction.ErrorMessage;
                }

                DrawActionState(obj.name, selectionRect, RunningActionNameStyle, ErrorActionBackgroundColor);
            }
        }

        private static GUIStyle GetNameStyle(Type actionType)
        {
            return customActionNameStyles.TryGetValue(actionType, out GUIStyle style) ? style : NormalActionNameStyle;
        }

        private static void DrawEditModeVisualActionState(GameObject obj, VisualAction visualAction, Rect selectionRect)
        {
            DrawActionState(obj.name, selectionRect, GetNameStyle(visualAction.GetType()), Color.clear);
        }

        private static void DrawActionState(string name, Rect selectionRect, GUIStyle nameLabelStyle,
            Color backgroundColor)
        {
            var nameRect = new Rect(selectionRect.position + new Vector2(18f, 0f), selectionRect.size);
            EditorGUI.DrawRect(selectionRect, backgroundColor);
            EditorGUI.LabelField(nameRect, name, nameLabelStyle);
        }

        private static void DrawVisualActionLabel(Rect selectionRect, string label)
        {
            var r = new Rect(selectionRect);
            r.x += r.width - r.height * 2.5f;

            Color color = Color.white;
            EditorGUI.LabelField(r, label, new GUIStyle()
                {
                    normal = new GUIStyleState() {textColor = color},
                    fontStyle = FontStyle.Bold
                }
            );
        }
        
        private static void DrawVisualActionIcon(Rect selectionRect)
        {
            var r = new Rect(selectionRect);
            r.xMin = r.xMax - 20f;
            GUI.Label(r, visualActionIcon);
        }
    }
}