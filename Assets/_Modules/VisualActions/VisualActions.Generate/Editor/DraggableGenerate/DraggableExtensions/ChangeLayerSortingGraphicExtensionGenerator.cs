using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class ChangeLayerSortingGraphicExtensionGenerator : BaseGenerateObject<ChangeSortingLayerGraphicWhenSelectExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/ChangeLayerSortingGraphic.prefab";
        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}