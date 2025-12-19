using Mimi.Interactions.Dragging;
using Mimi.Reflection.Extensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor
{
    public class DraggableGenerator : BaseGenerateObject<BaseDraggable>,IDraggableGenerate
    {
        private IDraggableExtensionGenerate draggableExtensionGenerate;
        private IPositionProcessorGenerate positionProcessorGenerate;

        public override string PrefabAddress => "/Prefabs/Draggable/Draggable.prefab";
        
        public BaseDraggable Generate()
        {
            var instance = GeneratePrefab();
            if (draggableExtensionGenerate != null)
            {
                var extensionInstance = draggableExtensionGenerate.Generate();
               extensionInstance.transform.SetParent(instance.transform);
               instance.SetField("draggableExtension",  extensionInstance, AccessModifier.Private);
            }

            if (positionProcessorGenerate != null)
            {
                var extensionInstance = positionProcessorGenerate.Generate();
                extensionInstance.transform.SetParent(instance.transform);
                instance.SetField("positionProcessor",  extensionInstance, AccessModifier.Private);
            }
            
            return instance;
        }

        public void SetExtension(IDraggableExtensionGenerate extensionGenerate)
        {
            this.draggableExtensionGenerate = extensionGenerate;
        }

        public void SetPositionProcessor(IPositionProcessorGenerate positionProcessorGenerate)
        {
            this.positionProcessorGenerate = positionProcessorGenerate;
        }

        
    }
}