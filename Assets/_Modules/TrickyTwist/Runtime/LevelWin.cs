using System.Threading;
using BW.EventSystem;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;

public class LevelWin : VisualAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Messenger.Broadcast(EventKey.LevelWin);
        await UniTask.CompletedTask;
    }
}