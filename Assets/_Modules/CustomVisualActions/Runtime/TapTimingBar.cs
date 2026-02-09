using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using UnityEngine;

public class TapTimingBar : TimingBarAction
{
    [SerializeField] private bool trueTimingAction = true;
    private bool sameResult;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.sameResult = false;
        LeanTouch.OnFingerDown += FingerDownHandler;

        await UniTask.WaitUntil(() => this.sameResult);
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        return base.OnExit(cancellationToken);
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            return;
        }

        this.timingBar.StopRunning();
        this.sameResult = this.trueTimingAction == this.timingBar.IsTrueTiming();
    }
}