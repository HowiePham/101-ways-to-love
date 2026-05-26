using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;

public class SendAngelAppearingEvent : VisualAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Messenger.Broadcast(EventKey.AngelAppearing);
        await UniTask.CompletedTask;
    }
}