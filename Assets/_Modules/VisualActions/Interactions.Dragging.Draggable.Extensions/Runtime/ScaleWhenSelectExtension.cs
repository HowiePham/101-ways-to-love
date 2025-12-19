using DG.Tweening;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class ScaleWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private Vector3 targetScale = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private float timeScale = 0.2f;
        [SerializeField] private Ease ease = Ease.Linear;
        private Vector3 originalScale;
        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            originalScale = this.BaseDraggable.transform.localScale;
        }

        public override void StartDrag()
        {
            BaseDraggable.Transform.DOScale(targetScale, timeScale).SetEase(this.ease);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            BaseDraggable.Transform.DOScale(originalScale, timeScale).SetEase(this.ease);
        }
    }
}