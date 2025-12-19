using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public abstract class MonoItemDragExtension : MonoBehaviour, IItemDragExtension
    {
        public abstract void OnInit(DraggableItem draggableItem);
        public abstract void OnPickUp(DraggableItem draggableItem);
        public abstract void OnReturn(DraggableItem draggableItem);
    }
}