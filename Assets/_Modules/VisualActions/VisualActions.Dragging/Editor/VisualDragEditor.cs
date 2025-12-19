using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.LeanTouch.Editor;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Areas.Editor;
using UnityEditor;
using UnityEngine;
using VisualActions.Areas;

namespace Mimi.VisualActions.Dragging.Editor
{
    public static class VisualDragEditor
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Dragging/Drag In", false, -10000)]
        public static void CreateDragIn(MenuCommand menuCommand)
        {
            var dragActionGo = new GameObject(nameof(VisualDrag));
            dragActionGo.transform.position = Vector3.zero;
            var visualDrag = dragActionGo.AddComponent<VisualDrag>();

            // Create drag game object
            var dragGo = new GameObject("DragObject");
            dragGo.transform.SetParent(dragActionGo.transform);
            dragGo.transform.localPosition = Vector3.zero;
            dragGo.AddComponent<SpriteRenderer>();
            dragGo.AddComponent<LeanTouchDraggable>();
            var dragCollider = dragGo.AddComponent<BoxCollider2D>();
            dragCollider.isTrigger = true;
            dragCollider.size = Vector2.one;

            // Create box area
            BoxArea boxArea2D = VisualAreaEditor.NewBoxArea(menuCommand);
            boxArea2D.transform.SetParent(dragActionGo.transform);
            boxArea2D.transform.localPosition = Vector3.zero;

            var insideArea2D = dragGo.AddComponent<InsideArea2D>();
            insideArea2D.SetField("checkTransform", dragActionGo.transform, AccessModifier.Private);
            insideArea2D.SetField("targetArea", boxArea2D, AccessModifier.Private);
            visualDrag.SetField("completeCondition", insideArea2D, AccessModifier.Private);

            // Create lean touch drag handler if needed 
            var leanTouchDragHandler = Object.FindObjectOfType<LeanTouchDragHandler>();

            if (leanTouchDragHandler == null)
            {
                leanTouchDragHandler = LeanTouchDragEditor.NewLeanTouchDragHandler();
            }

            leanTouchDragHandler.transform.SetParent(dragActionGo.transform.parent);
            int siblingIndex = Mathf.Min(0, dragActionGo.transform.GetSiblingIndex() - 1);
            leanTouchDragHandler.transform.SetSiblingIndex(siblingIndex);
            visualDrag.SetField("dragHandler", leanTouchDragHandler, AccessModifier.Private);

            GameObjectUtility.SetParentAndAlign(dragActionGo, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(dragActionGo, dragActionGo.GetInstanceID().ToString());
            Selection.activeObject = dragActionGo;
        }
    }
}