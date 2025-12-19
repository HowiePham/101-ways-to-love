using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Generate.Editor.DraggableExtensions;
using Mimi.VisualActions.Interactions.Draggable.Extensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public class PlaySoundExtensionEdit : BaseDraggableExtensionEdit<PlaySoundExtension>
    {
        protected override PlaySoundExtension CreateNewExtension()
        {
            return new PlaySoundDraggableExtensionGenerator().Generate() as PlaySoundExtension;
        }
        
    }
}