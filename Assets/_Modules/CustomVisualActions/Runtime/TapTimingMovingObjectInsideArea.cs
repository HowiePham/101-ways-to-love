using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class TapTimingMovingObjectInsideArea : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private BaseArea area;
    [SerializeField] private bool insideArea = true;
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
        if (finger.IsOverGui || !this.area.Active || !this.target.gameObject.activeSelf)
        {
            return;
        }

        this.complete = this.insideArea == this.area.ContainsWorldSpace(this.target.position);
    }
}