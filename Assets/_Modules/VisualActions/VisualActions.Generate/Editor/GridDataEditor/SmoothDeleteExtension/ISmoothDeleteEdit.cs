using Mimi.Interactions.Dragging;
using Mimi.VisualActions.Deleting;

namespace Mimi.VisualActions.Generate.Editor.SmoothDeleteExtension
{
    public interface ISmoothDeleteEdit
    {
        public void Init(VisualSmoothDelete smoothDelete, BaseDraggable baseDraggable);
    }
    
    public interface ISmoothDeleteEdit<T> : ISmoothDeleteEdit, IDataEdit
        where T : MonoSmoothDeleteExtension
    {
        T Extension { get; }
        bool IsToggle { get; set; }
        void TurnOnHandle();
        void TurnOffHandle();
    }
}