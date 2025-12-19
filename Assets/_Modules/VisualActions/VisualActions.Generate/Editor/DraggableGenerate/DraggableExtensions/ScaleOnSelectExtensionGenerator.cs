using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class ScaleOnSelectExtensionGenerator : BaseGenerateObject<ScaleWhenSelectExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/ScaleOnSelect.prefab";
        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}