using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    public class CompositeDragExtension : MonoDragExtension
    {
        [SerializeField] private MonoDragExtension[] extensions;

        public override void Init()
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.Init();
            }
        }

        public override void Start()
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.Start();
            }
        }

        public override void StartDrag(BaseDraggable draggable)
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.StartDrag(draggable);
            }
        }

        public override void Drag(BaseDraggable draggable)
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.Drag(draggable);
            }
        }

        public override void EndDrag(BaseDraggable draggable)
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.EndDrag(draggable);
            }
        }

        public override void OnCompleted(BaseDraggable draggable)
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.OnCompleted(draggable);
            }
        }

        public override void OnFailed(BaseDraggable draggable)
        {
            foreach (IDragExtension extension in this.extensions)
            {
                extension.OnFailed(draggable);
            }
        }
    }
}