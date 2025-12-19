using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;

namespace Mimi.VisualActions.Generate.Editor.DraggableExtension
{
    public interface IDraggableExtensionEdit
    {
        public void SetDraggable(BaseDraggable draggable);
    }
    
    public interface IDraggableExtensionEdit<T> : IDraggableExtensionEdit, IDataEdit
        where T : MonoDraggableExtension
    {
        T Extension { get; }
        bool IsToggle { get; set; }
        void TurnOnHandle();
        void TurnOffHandle();
    }
}