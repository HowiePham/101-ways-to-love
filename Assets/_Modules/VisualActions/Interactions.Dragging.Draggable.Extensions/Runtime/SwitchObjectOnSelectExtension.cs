using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class SwitchObjectOnSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private GameObject deactiveGO;
        [SerializeField] private GameObject activeGO;
       

        public override void StartDrag()
        {
            if (deactiveGO != null)
            {
                deactiveGO.SetActive(false);
            }

            if (activeGO != null)
            {
                activeGO.SetActive(true);
            }
         
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            if (deactiveGO != null)
            {
                deactiveGO.SetActive(true);
            }

            if (activeGO != null)
            {
                activeGO.SetActive(false);
            }
        }
    }
}