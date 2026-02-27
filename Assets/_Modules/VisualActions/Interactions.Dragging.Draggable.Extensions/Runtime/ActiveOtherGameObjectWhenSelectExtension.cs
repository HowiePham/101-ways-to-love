using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class ActiveOtherGameObjectWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private GameObject targetObj;
        [SerializeField] private bool active;
        
        public override void StartDrag()
        {
            this.targetObj.SetActive(this.active);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            this.targetObj.SetActive(!this.active);
        }
    }
}