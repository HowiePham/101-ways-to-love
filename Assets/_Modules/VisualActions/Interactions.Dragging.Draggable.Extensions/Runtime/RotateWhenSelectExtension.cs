using DG.Tweening;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class RotateWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private float timeRotate = 0.2f;
        [SerializeField] private Ease ease;

        private Vector3 originalRotation;
        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            this.originalRotation = draggable.transform.eulerAngles;
        }

        public override void StartDrag()
        {
            BaseDraggable.Transform.DORotate(targetRotation, timeRotate).SetEase(this.ease);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            BaseDraggable.Transform.DORotate(originalRotation, timeRotate).SetEase(this.ease);
        }
    }
}