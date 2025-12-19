using Mimi.Interactions.Dragging.Extensions;
using Mimi.VisualActions.Generate.Editor.DraggableExtensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class DraggableItemExtensionEdit : BaseDraggableExtensionEdit<DraggableItemExtension>
    {
        protected override DraggableItemExtension CreateNewExtension()
        {
            return new DraggableItemExtensionGenerator().Generate() as DraggableItemExtension;
        }
    }
}