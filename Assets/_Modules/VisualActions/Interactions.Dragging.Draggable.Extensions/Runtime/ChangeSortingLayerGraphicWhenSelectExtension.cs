using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class ChangeSortingLayerGraphicWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private int targetOrder;
        [SerializeField] [SortingLayerPopup] private string sortingLayerId;
        
        private int initialOrder;
        private string initialSortingLayerId;

        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            this.initialOrder = this.BaseDraggable.Graphic.SortingOrder;
            this.initialSortingLayerId = this.BaseDraggable.Graphic.SortingLayerName;
        }

        public override void StartDrag()
        {
            if (targetOrder != -1)
            {
                BaseDraggable.Graphic.SetSortingOrder(targetOrder);
            }
            BaseDraggable.Graphic.SetSortingLayerName(sortingLayerId);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            if (targetOrder != -1)
            {
                BaseDraggable.Graphic.SetSortingOrder(initialOrder);
            }
            BaseDraggable.Graphic.SetSortingLayerName(initialSortingLayerId);
        }
    }
}