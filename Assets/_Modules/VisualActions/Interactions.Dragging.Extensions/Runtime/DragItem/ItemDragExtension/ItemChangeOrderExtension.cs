using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class ItemChangeOrderExtension : MonoItemDragExtension
    {
        [SerializeField] private int targetOrder;
        [SerializeField,SortingLayerPopup] private string targetSortingLayer;

        private int initialOrder;
        private string initialSortingLayer;
        public override void OnInit(DraggableItem draggableItem)
        {
            this.initialOrder = draggableItem.Graphic.SortingOrder;
            this.initialSortingLayer = draggableItem.Graphic.SortingLayerName;
        }

        public override void OnPickUp(DraggableItem draggableItem)
        {
            if (targetOrder != -1)
            {
                draggableItem.Graphic.SetSortingOrder(targetOrder);
            }
            draggableItem.Graphic.SetSortingLayerName(targetSortingLayer);
        }

        public override void OnReturn(DraggableItem draggableItem)
        {
            if (targetOrder != -1)
            {
                draggableItem.Graphic.SetSortingOrder(initialOrder);
            }
            draggableItem.Graphic.SetSortingLayerName(initialSortingLayer);
        }
    }
}