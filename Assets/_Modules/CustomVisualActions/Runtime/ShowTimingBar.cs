using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ShowTimingBar : VisualAction
{
    [SerializeField] private TimingBar timingBar;
    [SerializeField] private bool waitShowingDone = true;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.timingBar == null)
        {
            await UniTask.CompletedTask;
            return;
        }

        if (this.waitShowingDone)
        {
            await this.timingBar.Show();
        }
        else
        {
            this.timingBar.Show();
            await UniTask.CompletedTask;
        }
    }
}