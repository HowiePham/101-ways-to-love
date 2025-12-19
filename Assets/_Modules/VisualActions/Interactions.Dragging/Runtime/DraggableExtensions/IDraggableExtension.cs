namespace Mimi.Interactions.Dragging.DraggableExtensions
{
    public interface IDraggableExtension
    {
        void Init(BaseDraggable draggable);
        void StartDrag();
        void Drag();
        void EndDrag();
    }
}