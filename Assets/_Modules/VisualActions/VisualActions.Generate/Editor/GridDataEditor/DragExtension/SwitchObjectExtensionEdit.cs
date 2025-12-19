using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
    public class SwitchObjectExtensionEdit : BaseDragExtensionEdit<SwitchObjectExtension>
    {
        protected override SwitchObjectExtension CreateNewExtension()
        {
            return new SwitchObjectDragExtensionGenerator(null).Generate() as SwitchObjectExtension;
        }
    }
}