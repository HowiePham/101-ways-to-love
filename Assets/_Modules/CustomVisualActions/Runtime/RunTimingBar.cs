using System.Threading;
using Cysharp.Threading.Tasks;

public class RunTimingBar : TimingBarAction
{
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.StartRunning();
    }
}