using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.Interactions.Dragging;
using Mimi.VisualActions;
using Mimi.VisualActions.Attribute;
using UnityEngine;

namespace VisualActions.VisualTransform
{
    public class MoveDraggableByDistance : VisualAction
    {
        [MainInput]
        [SerializeField] private BaseDraggable moveObject;
        [SerializeField] private Vector3 distance;
        [SerializeField] private bool isWaiting;
        [SerializeField] private bool isUseTransform;
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            var targetPos = moveObject.transform.position + distance;
            if (!isUseTransform)
            {
                this.moveObject.SetPosition(targetPos);
                if (isWaiting)
                {
                    await UniTask.WaitUntil(() => Vector3.Distance(targetPos, this.moveObject.Position) < 0.5f, cancellationToken: cancellationToken);
                }
            }
         
            await UniTask.CompletedTask;
        }

       
    }
}