using Mimi.VisualActions;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    public class DragRotateCondition : VisualCondition
    {
        [SerializeField] private DragRotateExtenstion dragRotateExtenstion;
        public override bool Validate()
        {
            return dragRotateExtenstion.CheckWinCondition();
        }
    }
}