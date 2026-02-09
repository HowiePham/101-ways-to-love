using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class RotateWhenSelectExtension : MonoDraggableExtension
    {
        [SerializeField] private Vector3 targetRotation;
        [SerializeField] private float timeRotate = 0.2f;
        [SerializeField] private float startDelay;
        [SerializeField] private float endDelay;
        [SerializeField] private Ease ease;

        private Vector3 originalRotation;

        public override void Init(BaseDraggable draggable)
        {
            base.Init(draggable);
            this.originalRotation = draggable.transform.eulerAngles;
        }

        public override void StartDrag()
        {
            RotateObject(this.targetRotation, this.startDelay);
        }

        private async UniTask RotateObject(Vector3 rotation, float delay = 0f)
        {
            await UniTask.WaitForSeconds(delay);
            this.BaseDraggable.Transform.DORotate(rotation, this.timeRotate).SetEase(this.ease);
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            RotateObject(this.originalRotation, this.endDelay);
        }
    }
}