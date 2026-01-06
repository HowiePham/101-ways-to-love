using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ExecuteVisualAction : VisualAction
{
    [SerializeField] private VisualAction visualAction;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.visualAction.Execute(cancellationToken);
        await UniTask.CompletedTask;
    }
}