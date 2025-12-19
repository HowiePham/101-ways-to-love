using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class CompositeItemExtension : MonoItemDragExtension
    {
        [SerializeField] private MonoItemDragExtension[] compositeItems;
        public override void OnInit(DraggableItem draggableItem)
        {
            foreach (var item in compositeItems)
            {
                item.OnInit(draggableItem);
            }
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
            foreach (var item in compositeItems)
            {
                item.OnPickUp(draggableItem);
            }
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
            foreach (var item in compositeItems)
            {
                item.OnReturn(draggableItem);
            }
        }
    }
}