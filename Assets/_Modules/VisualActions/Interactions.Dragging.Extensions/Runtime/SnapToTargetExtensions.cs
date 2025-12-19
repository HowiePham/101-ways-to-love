using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Mimi.Interactions.Dragging.Extensions
{
    [TypeInfoBox("Snap Draggable to target position after completion.")]
    public class SnapToTargetExtensions : MonoDragExtension
    {
        [SerializeField] private Transform targetPosition;
        [SerializeField] private float delaySeconds;
        [SerializeField] private float timeMove = 0.2f;
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
            MoveToTarget(draggable);
        }

        async UniTask MoveToTarget(BaseDraggable draggable)
        {
            await UniTask.Delay(Mathf.RoundToInt(delaySeconds * 1000));
            draggable.transform.DOMove(targetPosition.position,timeMove );
        }

        public override void OnFailed(BaseDraggable draggable)
        {
           
        }
    }
}