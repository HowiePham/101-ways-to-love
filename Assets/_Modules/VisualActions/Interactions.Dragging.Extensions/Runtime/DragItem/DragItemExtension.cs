using Mimi.Interactions.Dragging.DraggableExtensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    [TypeInfoBox("DraggableItemExtension: Control")]
    public class DragItemExtension : MonoDragExtension
    {
        [SerializeField] private DraggableItemExtension draggableItemExtension;
        public override void Init()
        {
            
        }

        public override void Start()
        {
            
        }

        public override void StartDrag(BaseDraggable draggable)
        {
          
        }

        public override void Drag(BaseDraggable draggable)
        {
          
        }

        public override void EndDrag(BaseDraggable draggable)
        {
       
        }

        public override void OnCompleted(BaseDraggable draggable)
        {
            this.draggableItemExtension.OnDragSuccess(draggable);
        }

        public override void OnFailed(BaseDraggable draggable)
        {
            this.draggableItemExtension.OnDragFailed(draggable);
        }
    }
}