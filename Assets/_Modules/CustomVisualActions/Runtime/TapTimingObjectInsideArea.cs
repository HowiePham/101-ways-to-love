using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class TapTimingObjectInsideArea : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private BaseArea area;
    private bool complete;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown += FingerDownHandler;
        this.complete = false;

        await UniTask.WaitUntil(() => this.complete);
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        return base.OnExit(cancellationToken);
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            return;
        }

        this.complete = this.area.ContainsWorldSpace(this.target.position);
    }
}