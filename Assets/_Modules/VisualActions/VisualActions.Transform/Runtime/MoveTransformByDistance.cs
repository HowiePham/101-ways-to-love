using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using Mimi.VisualActions.Attribute;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VisualActions.VisualTransform
{
    public class MoveTransformByDistance : VisualAction
    {
        [MainInput]
        [SerializeField] private Transform moveObject;
        [SerializeField] private Vector3 distance;
        [SerializeField] private float duration;
        [SerializeField] private Ease ease = Ease.Linear;

        [SerializeField] private bool completeAfterMove = false;

        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (completeAfterMove)
            {
                Debug.Log(this.moveObject.position + distance);
                await this.moveObject.DOMove(this.moveObject.position + distance, this.duration).SetEase(this.ease)
                    .AsyncWaitForCompletion();
            }
            else
            {
                this.moveObject.DOMove(this.moveObject.position + distance, this.duration).SetEase(this.ease);
            }

            await UniTask.CompletedTask;
        }
    }
}