using UnityEngine;

namespace Mimi.Interactions.Dragging.DraggableExtensions
{
    public abstract class MonoDraggableExtension : MonoBehaviour, IDraggableExtension
    {
        protected BaseDraggable BaseDraggable { get; private set; }

        public virtual void Init(BaseDraggable draggable)
        {
            BaseDraggable = draggable;
        }

        public abstract void StartDrag();

        public abstract void Drag();

        public abstract void EndDrag();
    }
}