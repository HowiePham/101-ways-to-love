using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;

public class ActionFailed : VisualAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Messenger.Broadcast(EventKey.ActionFailed);
        await UniTask.CompletedTask;
    }
}