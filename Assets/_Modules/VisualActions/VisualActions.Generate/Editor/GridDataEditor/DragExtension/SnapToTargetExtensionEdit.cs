using Mimi.Interactions.Dragging.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
    public class SnapToTargetExtensionEdit : BaseDragExtensionEdit<SnapToTargetExtensions>
    {
        protected override SnapToTargetExtensions CreateNewExtension()
        {
            return new SnapToTargetExtensionGenerator(null).Generate() as SnapToTargetExtensions;
        }
    }
}