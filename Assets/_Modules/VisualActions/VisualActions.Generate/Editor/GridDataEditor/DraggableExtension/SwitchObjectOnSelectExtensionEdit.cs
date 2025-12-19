using Mimi.Interactions.Dragging.Extensions;
using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class SwitchObjectOnSelectExtensionEdit : BaseDraggableExtensionEdit<SwitchObjectOnSelectExtension>
    {
        protected override SwitchObjectOnSelectExtension CreateNewExtension()
        {
            return new SwitchObjectOnSelectExtensionGenerator().Generate() as SwitchObjectOnSelectExtension;
        }
    }
}