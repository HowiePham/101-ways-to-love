using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class KeepHorizontalPosition : MonoDraggableExtension
    {
        [SerializeField] private float xPosition;

        public override void StartDrag()
        {
        }

        public override void Drag()
        {
            Vector3 currentPos = this.BaseDraggable.transform.position;
            this.BaseDraggable.SetPosition(new Vector3(this.xPosition, currentPos.y, currentPos.z));
        }

        public override void EndDrag()
        {
        }
    }
}