using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class ToggleComponentWhileDragging : MonoDraggableExtension
    {
        [SerializeField] private Behaviour targetComponent;
        [SerializeField] private bool enable;

        public override void StartDrag()
        {
            this.targetComponent.enabled = this.enable;
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            this.targetComponent.enabled = !this.enable;
        }
    }
}