using System.Threading;
using Cysharp.Threading.Tasks;
using Games;
using Mimi.Prototypes.Events;
using Mimi.VisualActions;
using UnityEngine;

public class ShowTutorialHint : VisualAction
{
    [SerializeField] private HintPlayer hintPlayer;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.hintPlayer.LevelTutorial)
        {
            Messenger.Broadcast(EventKey.ShowHint);
        }

        await UniTask.CompletedTask;
    }
}