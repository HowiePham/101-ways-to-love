using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.DraggableExtensions;
using Mimi.VisualActions.Attribute;
using Mimi.VisualActions.Data;
using UnityEngine;

namespace Mimi.VisualActions.Interactions.Draggable.Extensions
{
    public class JumpToPositionOnDeselect : MonoDraggableExtension
    {
        [HideInBehaviourEditor] [SerializeField]
        private Transform startPosition;

        [HideInBehaviourEditor] [SerializeField]
        private Vector3Field offsetField;

        [SerializeField] private float delaySeconds;

        [SerializeField] private Transform endTarget;
        [SerializeField] private Ease fallEase = Ease.InQuad;
        [SerializeField] private float throwPower;
        [SerializeField] private float duration;

        public override void StartDrag()
        {
        }

        public override void Drag()
        {
        }

        public override void EndDrag()
        {
            JumpToPosition(this.BaseDraggable);
        }

        private async UniTask JumpToPosition(BaseDraggable draggable)
        {
            Transform targetObject = draggable.transform;
            var targetPos = new Vector2(this.endTarget.position.x, this.endTarget.position.y);
            targetObject.DOJump(targetPos, this.throwPower, 1, this.duration);
            await UniTask.WaitForSeconds(this.duration);
            draggable.SetPosition(targetPos);
        }
    }
}