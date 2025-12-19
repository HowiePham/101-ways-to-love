using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class DragItemExtensionGenerator : BaseGenerateObject<DragItemExtension>, IVisualDragExtensionGenerator
    {
        private DraggableItemExtension draggableItemExtension;
        public override string PrefabAddress => 
            "/Prefabs/VisualActions/VisualDrag/Extensions/DraggableItemExtension/DragItem.prefab";

        public DragItemExtensionGenerator(DraggableItemExtension draggableItemExtension)
        {
            this.draggableItemExtension = draggableItemExtension;
        }
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("draggableItemExtension", this.draggableItemExtension, AccessModifier.Private);
            return instance;
        }
    }
}