namespace Mimi.Interactions.Dragging
{
    public class NullDragExtension : IDragExtension
    {
        public static readonly NullDragExtension Instance = new();

        private NullDragExtension()
        {
        }

        public void Init()
        {
        }

        public void Start()
        {
            
        }

        public void StartDrag(BaseDraggable draggable)
        {
        }

        public void Drag(BaseDraggable draggable)
        {
        }

        public void EndDrag(BaseDraggable draggable)
        {
        }

        public void OnCompleted(BaseDraggable draggable)
        {
        }

        public void OnFailed(BaseDraggable draggable)
        {
        }
    }
}