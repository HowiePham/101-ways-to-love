using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
    public class SetInteractableExtensionEdit : BaseDragExtensionEdit<SetInteractableExtension>
    {
        protected override SetInteractableExtension CreateNewExtension()
        {
            return new SetInteractableExtensionGenerator(true).Generate() as SetInteractableExtension;
        }
    }
}