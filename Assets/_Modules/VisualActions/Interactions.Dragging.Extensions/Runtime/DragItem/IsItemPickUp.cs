using Mimi.VisualActions;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class IsItemPickUp : VisualCondition
    {
        [SerializeField] private DraggableItem draggableItem;
        public override bool Validate()
        {
           return draggableItem.IsPickUp;
        }
    }
}