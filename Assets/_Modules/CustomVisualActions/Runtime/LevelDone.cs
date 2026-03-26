using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;

public class LevelDone : VisualAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Messenger.Broadcast(EventKey.LevelDone);
        await UniTask.CompletedTask;
    }
}