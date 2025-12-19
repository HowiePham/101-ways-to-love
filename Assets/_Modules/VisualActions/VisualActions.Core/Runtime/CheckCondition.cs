using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class CheckCondition : VisualAction
{
    [SerializeField] private VisualCondition condition;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await UniTask.WaitUntil(this.condition.Validate, cancellationToken: cancellationToken);
    }
}