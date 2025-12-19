using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class SwitchObjectOnSelectExtensionGenerator : BaseGenerateObject<SwitchObjectOnSelectExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/SwitchObject.prefab";
        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}