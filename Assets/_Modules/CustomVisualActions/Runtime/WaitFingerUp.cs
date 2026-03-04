using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;

public class WaitFingerUp : VisualAction
{
    private bool isCompleted;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.isCompleted = false;
        LeanTouch.OnFingerUp += FingerUpHandler;

        await UniTask.WaitUntil(() => this.isCompleted);
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerUp -= FingerUpHandler;
        return base.OnExit(cancellationToken);
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        this.isCompleted = true;
    }
}