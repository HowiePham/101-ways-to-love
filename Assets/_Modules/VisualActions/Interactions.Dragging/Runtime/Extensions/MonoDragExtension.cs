using UnityEngine;

namespace Mimi.Interactions.Dragging
{
    public abstract class MonoDragExtension : MonoBehaviour, IDragExtension
    {
        public abstract void Init();
        public abstract void Start();
        public abstract void StartDrag(BaseDraggable draggable);
        public abstract void Drag(BaseDraggable draggable);
        public abstract void EndDrag(BaseDraggable draggable);
        public abstract void OnCompleted(BaseDraggable draggable);
        public abstract void OnFailed(BaseDraggable draggable);
    }
}