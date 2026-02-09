using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using UnityEngine;

public class ExecuteMultipleVisualAction : VisualAction
{
    [SerializeField] private VisualAction[] visualAction;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        foreach (VisualAction action in this.visualAction)
        {
            action.Execute(cancellationToken);
        }

        Messenger.Broadcast(EventKey.ResetAction);
        await UniTask.CompletedTask;
    }
}