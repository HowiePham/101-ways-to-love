using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class RotateOnSelectExtensionEdit : BaseDraggableExtensionEdit<RotateWhenSelectExtension>
    {
        protected override RotateWhenSelectExtension CreateNewExtension()
        {
            return new RotateOnSelectExtensionGenerator().Generate() as RotateWhenSelectExtension;
        }
    }
}