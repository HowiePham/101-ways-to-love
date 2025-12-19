using Mimi.ConditionValidator;

namespace Mimi.Interactions.Dragging
{
    public class DragInteraction
    {
        private readonly BaseDragHandler dragHandler;
        private readonly IConditionValidator completeConditionValidator;
        private readonly IDragExtension dragExtension;
        private readonly bool isAutoCheckComplete;

        public bool IsStarted { private set; get; }
        public bool IsCompleted { private set; get; }
        public bool IsDragging { private set; get; }
        public BaseDraggable CurrentDraggable { private set; get; }

        public DragInteraction(BaseDragHandler dragHandler, bool isAutoCheckComplete = false)
        {
            this.dragHandler = dragHandler;
            this.completeConditionValidator = NullConditionValidator.Instance;
            this.dragExtension = NullDragExtension.Instance;
            this.isAutoCheckComplete = isAutoCheckComplete;
        }

        public DragInteraction(BaseDragHandler dragHandler, IConditionValidator completeConditionValidator, bool isAutoCheckComplete = false) : this(
            dragHandler,isAutoCheckComplete)
        {
            this.completeConditionValidator = completeConditionValidator;
            this.isAutoCheckComplete = isAutoCheckComplete;
        }

        public DragInteraction(BaseDragHandler dragHandler, IConditionValidator completeConditionValidator,
            IDragExtension dragExtension, bool isAutoCheckComplete = false) : this(dragHandler, completeConditionValidator, isAutoCheckComplete)
        {
            this.dragExtension = dragExtension;
            this.dragExtension.Init();
        }

        public void Start()
        {
            if (IsStarted) return;
            IsStarted = true;
            IsCompleted = false;
            IsDragging = false;
            CurrentDraggable = null;
            this.dragExtension.Start();
            this.dragHandler.OnStartDrag += StartDragHandler;
            this.dragHandler.OnDragging += DraggingHandler;
            this.dragHandler.OnEndDrag += EndDragHandler;
        }

        public void Stop()
        {
            if (!IsStarted) return;
            this.dragHandler.OnStartDrag -= StartDragHandler;
            this.dragHandler.OnDragging -= DraggingHandler;
            this.dragHandler.OnEndDrag -= EndDragHandler;
            IsStarted = false;
            IsDragging = false;
            CurrentDraggable = null;
            IsCompleted = false;
        }

        private void StartDragHandler(BaseDraggable draggable)
        {
            IsDragging = true;
            CurrentDraggable = draggable;
            this.dragExtension.StartDrag(draggable);
        }

        private void EndDragHandler(BaseDraggable draggable)
        {
            this.dragExtension.EndDrag(draggable);
            CheckCompleted(draggable);
            CurrentDraggable = null;
            IsDragging = false;
           
        }

        void CheckCompleted(BaseDraggable draggable)
        {
            IsCompleted = this.completeConditionValidator.Validate();

            if (IsCompleted)
            {
                this.dragExtension.OnCompleted(draggable);
            }
            else
            {
                this.dragExtension.OnFailed(draggable);
            }
        }

        private void DraggingHandler(BaseDraggable draggable)
        {
            this.dragExtension.Drag(draggable);
            if (isAutoCheckComplete && draggable != null && draggable.IsSelected)
            {
                CheckCompleted(draggable);
            }
        }
    }
}