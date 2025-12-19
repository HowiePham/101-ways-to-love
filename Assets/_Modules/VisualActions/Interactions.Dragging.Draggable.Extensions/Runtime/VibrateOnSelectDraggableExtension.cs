using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.Services.ScriptableObject.Vibration;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class VibrateOnSelectDraggableExtension : MonoDraggableExtension
    {
        [HideInBehaviourEditor]
        [SerializeField] private HapticVibrationServiceSO vibrationService;
        public override void StartDrag()
        {
            vibrationService.PlayLoopDefault();
        }

        public override void Drag()
        {
          
        }

        public override void EndDrag()
        {
           
        }
    }
}