using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class ScaleOnSelectExtensionEdit : BaseDraggableExtensionEdit<ScaleWhenSelectExtension>
    {
        protected override ScaleWhenSelectExtension CreateNewExtension()
        {
            return new ScaleOnSelectExtensionGenerator().Generate() as ScaleWhenSelectExtension;
        }
    }
}