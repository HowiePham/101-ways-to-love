using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class ChangeLayerSortingGraphicWhenSelectEdit : BaseDraggableExtensionEdit<ChangeSortingLayerGraphicWhenSelectExtension>
    {
        protected override ChangeSortingLayerGraphicWhenSelectExtension CreateNewExtension()
        {
            return new ChangeLayerSortingGraphicExtensionGenerator().Generate() as ChangeSortingLayerGraphicWhenSelectExtension;
        }
    }
}