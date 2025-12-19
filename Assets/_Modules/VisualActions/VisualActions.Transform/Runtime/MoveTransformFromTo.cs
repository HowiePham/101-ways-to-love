using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

namespace VisualActions.VisualTransform
{
    public class MoveTransformFromTo : VisualAction
    {
        [SerializeField] private Transform target;
        [SerializeField] private bool isDeActiveInitial;
        [SerializeField] private Vector3 offsetFrom;
        [SerializeField] private float duration;
        [SerializeField] private Ease easeMove = Ease.Unset;
        protected override UniTask OnInitializing()
        {
            if (isDeActiveInitial)
            {
                target.gameObject.SetActive(false);
            }
        
            return base.OnInitializing();
        }

        protected override UniTask OnExecuting(CancellationToken cancellationToken)
        {
            target.position = target.position + offsetFrom;
            target.gameObject.SetActive(true);
            target.DOMove(target.position - offsetFrom, duration).SetEase(easeMove);
            return UniTask.CompletedTask;
        }
    }
}