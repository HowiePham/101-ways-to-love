using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class ChangeLayerOrderWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private int targetOrder;
        [SerializeField] [SortingLayerPopup] private int sortingLayerId;
        
        private int initialOrder;
        private int initialSortingLayerId;
        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            initialOrder = renderer.sortingOrder;
            initialSortingLayerId = renderer.sortingLayerID;
        }

        public override void StartDrag()
        {
            if (targetOrder != -1)
            {
                renderer.sortingOrder = targetOrder;
            }
            renderer.sortingLayerID = sortingLayerId;
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            renderer.sortingOrder = initialOrder;
            renderer.sortingLayerID = initialSortingLayerId;
        }
    }
}