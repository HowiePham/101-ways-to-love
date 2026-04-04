using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using UnityEngine;

public class ExecuteVisualAction : VisualAction
{
    [SerializeField] private VisualAction visualAction;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.visualAction != null)
        {
            this.visualAction.Execute(cancellationToken);
        }

        Messenger.Broadcast(EventKey.ResetAction);
        await UniTask.CompletedTask;
    }
}