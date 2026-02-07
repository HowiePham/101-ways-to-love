using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class ResetTimingBar : TimingBarAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.ResetBar();
    }
}