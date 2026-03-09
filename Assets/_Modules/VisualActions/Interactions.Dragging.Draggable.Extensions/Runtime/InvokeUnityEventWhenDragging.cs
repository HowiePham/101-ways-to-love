using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class InvokeUnityEventWhenDragging : MonoDraggableExtension
    {
        [SerializeField] private UnityEvent startDrag;
        [SerializeField] private UnityEvent endDrag;

        public override void StartDrag()
        {
            this.startDrag.Invoke();
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            this.endDrag.Invoke();
        }
    }
}