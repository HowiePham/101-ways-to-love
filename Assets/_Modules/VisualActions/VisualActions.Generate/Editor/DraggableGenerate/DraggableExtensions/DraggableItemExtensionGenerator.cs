using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class DraggableItemExtensionGenerator : BaseGenerateObject<DraggableItemExtension>,IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/DragOtherItem.prefab";
        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}