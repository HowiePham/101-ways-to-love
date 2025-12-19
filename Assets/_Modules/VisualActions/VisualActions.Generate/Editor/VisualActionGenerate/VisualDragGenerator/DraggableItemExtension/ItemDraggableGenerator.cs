using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class ItemDraggableGenerator : BaseGenerateObject<DraggableItem>
    {
        public override string PrefabAddress =>
            "/Prefabs/VisualActions/VisualDrag/Extensions/DraggableItemExtension/ItemDraggable.prefab";

        public DraggableItem Generate()
        {
            return GeneratePrefab();
        }
    }
}