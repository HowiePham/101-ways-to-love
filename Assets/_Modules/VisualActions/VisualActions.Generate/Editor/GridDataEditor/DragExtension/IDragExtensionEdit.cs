using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Dragging;

namespace Mimi.VisualActions.Generate.Editor.DragExtension
{
    public interface IDragExtensionEdit
    {
        public void SetVisualDrag(VisualDrag visualDrag);
    }
    
    public interface IDragExtensionEdit<T> : IDragExtensionEdit, IDataEdit
        where T : MonoDragExtension
    {
        T Extension { get; }
        bool IsToggle { get; set; }
        void TurnOnHandle();
        void TurnOffHandle();
    }
}