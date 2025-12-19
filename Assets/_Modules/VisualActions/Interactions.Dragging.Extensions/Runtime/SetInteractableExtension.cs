using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    [TypeInfoBox("Set Interactable state of Draggable after completion.")]
    public class SetInteractableExtension : MonoDragExtension
    {
        [SerializeField] private bool isInteractable;
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
          draggable.SetInteractable(isInteractable);
        }

        public override void OnFailed(BaseDraggable draggable)
        {
           
        }
    }
}