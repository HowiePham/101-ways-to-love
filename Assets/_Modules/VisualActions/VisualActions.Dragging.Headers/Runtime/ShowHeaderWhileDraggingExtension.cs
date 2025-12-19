using Mimi.Interactions.Dragging;
using UnityEngine;

namespace Mimi.VisualActions.Dragging.Headers
{
    public class ShowHeaderWhileDraggingExtension : MonoDragExtension
    {
        [SerializeField] private BaseGraphicHeader graphicHeader;

        public override void Init()
        {
        }

        public override void Start()
        {
            
        }

        public override void OnFailed(BaseDraggable draggable)
        {
        }

        public override void OnCompleted(BaseDraggable draggable)
        {
        }

        public override void EndDrag(BaseDraggable draggable)
        {
            this.graphicHeader.SetActive(false);
        }

        public override void Drag(BaseDraggable draggable)
        {
            this.graphicHeader.SetPosition(draggable.Position);
        }

        public override void StartDrag(BaseDraggable draggable)
        {
            this.graphicHeader.SetActive(true);
        }
    }
}