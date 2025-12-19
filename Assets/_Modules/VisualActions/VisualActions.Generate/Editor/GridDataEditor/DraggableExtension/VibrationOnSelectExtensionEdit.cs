using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class VibrationOnSelectExtensionEdit : BaseDraggableExtensionEdit<VibrateOnSelectDraggableExtension>
    {
        protected override VibrateOnSelectDraggableExtension CreateNewExtension()
        {
            return new VibrationOnSelectExtensionGenerator().Generate() as VibrateOnSelectDraggableExtension;
        }
    }
}