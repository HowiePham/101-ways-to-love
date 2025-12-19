using System.Threading;
using BW.EventSystem;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;

public class LevelLose : VisualAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Messenger.Broadcast(EventKey.LevelLose);
        await UniTask.CompletedTask;
    }
}