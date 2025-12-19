using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtensions
{
    public class PlaySoundDraggableExtensionGenerator : BaseGenerateObject<PlaySoundExtension>, IDraggableExtensionGenerate
    {
        public override string PrefabAddress => "/Prefabs/Draggable/Extensions/PlaySound.prefab";
        public MonoDraggableExtension Generate()
        {
            return GeneratePrefab();
        }
    }
}