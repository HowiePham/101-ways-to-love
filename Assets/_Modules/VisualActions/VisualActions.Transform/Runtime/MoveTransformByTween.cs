using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualActions.VisualTransform
{
    [TypeInfoBox("Move Transform into target transform by Dotween")]
    public class MoveTransformByTween : VisualAction
    {
        [MainInput]
        [SerializeField] private Transform movingTransform;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float duration;

        [SerializeField]
        private bool isWaitToComplete;
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (isWaitToComplete)
            {
                await movingTransform.DOMove(targetTransform.position, duration).AsyncWaitForCompletion().AsUniTask();
            }
            else
            {
                movingTransform.DOMove(targetTransform.position, duration);
            }
        }
    }
}
