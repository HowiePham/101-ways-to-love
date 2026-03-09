using System.Threading;
using Cysharp.Threading.Tasks;

public class RandomTimingBarArea : TimingBarAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.RandomTrueArea();
        await UniTask.CompletedTask;
    }
}