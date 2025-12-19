using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
    public class DragItemExtensionEdit : BaseDragExtensionEdit<DragItemExtension>
    {
        protected override DragItemExtension CreateNewExtension()
        {
            return new DragItemExtensionGenerator(null).Generate() as DragItemExtension;
        }
    }
}