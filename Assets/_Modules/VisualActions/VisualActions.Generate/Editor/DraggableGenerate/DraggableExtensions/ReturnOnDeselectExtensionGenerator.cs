using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class ReturnOnDeselectExtensionGenerator : BaseGenerateObject<ReturnOnDeselect>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/ReturnOnDeselect.prefab";

        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}