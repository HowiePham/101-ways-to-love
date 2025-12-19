using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor;
using UnityEditor;
using UnityEngine;

namespace Mimi.VisualActions.Generate.Template
{
    public static class TemplateDragEditor 
    {
        [MenuItem("GameObject/Visual Actions/Mechanics/Dragging/Drag Rotate", false, -10000)]
        public static void CreateDragRotate(MenuCommand menuCommand)
        {
            var draggable = new  DraggableStaticGenerator().Generate();
            var boxArea = new BoxArea2DGenerator().Generate();
            
            var dragRotateExtension = new DragRotateExtensionGenerator(draggable.transform, boxArea);
            var visualDragGenerate = new VisualDragGenerator(dragRotateExtension, null);
            
            var instance = visualDragGenerate.Generate();
            draggable.transform.SetParent(instance.transform);
            boxArea.transform.SetParent(instance.transform);

            var condition = new DragRotateConditionGenerator(instance.GetComponentInChildren<DragRotateExtenstion>()).Generate();
            boxArea.transform.position = draggable.transform.position;
            boxArea.SetField("size", Vector2.one * 7, AccessModifier.Private);
            
            condition.transform.SetParent(instance.transform);
            instance.SetField("completeCondition", condition, AccessModifier.Private);
            
        }
    }
}