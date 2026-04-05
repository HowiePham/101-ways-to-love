using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Games;
using Mimi.VisualActions;
using UnityEngine;

public class ShowTutorialHint : VisualAction
{
    [SerializeField] private HintPlayer hintPlayer;
    [SerializeField] private float delayInSeconds = 2f;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.hintPlayer.LevelTutorial)
        {
            ShowHintWithDelay(this.destroyCancellationToken).Forget();
        }

        await UniTask.CompletedTask;
    }

    private async UniTaskVoid ShowHintWithDelay(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(this.delayInSeconds),
                cancellationToken: cancellationToken);

            if (this.hintPlayer != null)
            {
                this.hintPlayer.ShowNextHint();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
