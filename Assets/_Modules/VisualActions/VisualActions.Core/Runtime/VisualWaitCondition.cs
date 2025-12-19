using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mimi.VisualActions
{
    public class VisualWaitCondition : VisualAction
    {
        [SerializeField] private VisualCondition condition;
        protected override async UniTask OnExecuting(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => condition.Validate(), cancellationToken: cancellationToken);
        }
    }
}