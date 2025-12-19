using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using Mimi.Reflection.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class SetInteractableExtensionGenerator : BaseGenerateObject<SetInteractableExtension>, IVisualDragExtensionGenerator
    {
        private bool isInteractable;
        public override string PrefabAddress => "/Prefabs/VisualActions/VisualDrag/Extensions/SetInteractable.prefab";

        public SetInteractableExtensionGenerator(bool isInteractable)
        {
            this.isInteractable = isInteractable;
        }
        public MonoDragExtension Generate()
        {
            var instance = GeneratePrefab();
            instance.SetField("isInteractable", isInteractable, AccessModifier.Private);
            return instance;
        }
    }
}