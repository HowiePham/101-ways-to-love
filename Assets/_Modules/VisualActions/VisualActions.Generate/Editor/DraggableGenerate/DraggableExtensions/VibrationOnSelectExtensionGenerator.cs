using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class VibrationOnSelectExtensionGenerator : BaseGenerateObject<VibrateOnSelectDraggableExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/VibrationOnSelect.prefab";

        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}