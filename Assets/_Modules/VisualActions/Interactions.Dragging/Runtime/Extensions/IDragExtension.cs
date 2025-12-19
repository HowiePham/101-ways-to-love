namespace Mimi.Interactions.Dragging
{
    public interface IDragExtension
    {
        void Init();
        void Start();
        void StartDrag(BaseDraggable draggable);
        void Drag(BaseDraggable draggable);
        void EndDrag(BaseDraggable draggable);
        void OnCompleted(BaseDraggable draggable);
        void OnFailed(BaseDraggable draggable);
    }
}