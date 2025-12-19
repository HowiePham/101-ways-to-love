using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class RotateOnSelectExtensionGenerator : BaseGenerateObject<RotateWhenSelectExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/RotateOnSelect.prefab";
        public MonoDraggableExtension Generate()
        {
           return GeneratePrefab();
        }
    }
}