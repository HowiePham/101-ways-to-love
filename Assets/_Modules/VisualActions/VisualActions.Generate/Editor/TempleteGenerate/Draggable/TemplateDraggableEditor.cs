using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Generate.Editor;
using Mimi.VisualActions.Interactions.Draggable.Extensions;
using UnityEditor;
using UnityEngine;


namespace Mimi.VisualActions.Generate.Template
{
    public static class TemplateDraggableEditor
    {
        [MenuItem("GameObject/Visual Actions/Draggable/Default", false, -10000)]
        public static void CreateDefaultDraggable(MenuCommand menuCommand)
        {
            var instance = new  DraggableGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = instance.transform.position;
            instance.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
        }

        [MenuItem("GameObject/Visual Actions/Draggable/DragItems", false, -10000)]
        public static void CreateDragItemDraggable(MenuCommand menuCommand)
        {
            var instance = new  DraggableItemGenerator().Generate();
            var returnPos = new GameObject("ReturnPos");
            returnPos.transform.position = instance.transform.position;
            instance.GetExtension<ReturnOnDeselect>().SetField("startPosition", returnPos.transform, AccessModifier.Private);
        }

        [MenuItem("GameObject/Visual Actions/Draggable/Static", false, -10000)]
        public static void CreateDraggableStatic(MenuCommand menuCommand)
        {
            var instance = new  DraggableStaticGenerator().Generate();
        }
    }
}