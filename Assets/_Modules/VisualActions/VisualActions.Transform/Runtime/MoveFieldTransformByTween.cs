using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using Mimi.VisualActions.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace VisualActions.VisualTransform
{
    [TypeInfoBox("Move wrapped Transform into target transform by Dotween")]
    public class MoveFieldTransformByTween : VisualAction
    {
        [FormerlySerializedAs("movingTransform")] [SerializeField] private TransformField moving;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float duration;

        [SerializeField]
        private bool isWaitToComplete;
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            if (isWaitToComplete)
            {
                await moving.GetValue().DOMove(targetTransform.position, duration).AsyncWaitForCompletion().AsUniTask();
            }
            else
            {
                moving.GetValue().DOMove(targetTransform.position, duration);
            }
        }
        
    }
}