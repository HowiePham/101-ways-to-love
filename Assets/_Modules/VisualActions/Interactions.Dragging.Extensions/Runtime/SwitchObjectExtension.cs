using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    [TypeInfoBox("Switch Draggable to another object after completion.")]
    public class SwitchObjectExtension : MonoDragExtension
    {
        [SerializeField] private GameObject switchObject;
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
            draggable.gameObject.SetActive(false);
            switchObject.SetActive(true);
        }

        public override void OnFailed(BaseDraggable draggable)
        {
            
        }
    }
}