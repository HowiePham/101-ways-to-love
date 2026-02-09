using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class HideTimingBar : TimingBarAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.Hide();
    }
}