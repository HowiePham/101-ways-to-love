using Mimi.Interactions.Dragging;

namespace Mimi.VisualActions.Generate.Editor
{
    public interface IDraggableGenerate
    {
        public BaseDraggable Generate();
        public void SetExtension(IDraggableExtensionGenerate extensionGenerate);
        public void SetPositionProcessor(IPositionProcessorGenerate positionProcessorGenerate);
    }
}