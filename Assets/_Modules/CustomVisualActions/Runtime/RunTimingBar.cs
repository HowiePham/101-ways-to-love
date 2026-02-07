using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class RunTimingBar : VisualAction
{
    [SerializeField] private TimingBar timingBar;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.ResumeRunning();
    }
}