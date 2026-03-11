using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using VisualActions.Areas;

public class HoldToRotate : VisualAction
{
    [SerializeField] private Transform target;
    [SerializeField] private BaseArea area;
    [SerializeField] private Vector3 targetRotation;
    [SerializeField] private float angleValue;
    [SerializeField] private float tolerance = 0.1f;
    private bool complete;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.complete = false;
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
        await UniTask.WaitUntil(() => this.complete, cancellationToken: cancellationToken);
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        await this.target.DORotate(this.targetRotation, 0.5f).SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            return;
        }

        Vector3 current = this.target.eulerAngles;
        bool isCorrect =
            Mathf.Abs(Mathf.DeltaAngle(current.x, this.targetRotation.x)) <= this.tolerance &&
            Mathf.Abs(Mathf.DeltaAngle(current.y, this.targetRotation.y)) <= this.tolerance &&
            Mathf.Abs(Mathf.DeltaAngle(current.z, this.targetRotation.z)) <= this.tolerance;

        if (isCorrect)
        {
            this.complete = true;
        }
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (finger.IsOverGui || !this.area.ContainsScreenPosition(finger.ScreenPosition, Camera.main))
        {
            return;
        }

        float currentAngle = this.target.eulerAngles.z;
        float rotateAngle = currentAngle + this.angleValue;
        this.target.DORotate(new Vector3(0, 0, rotateAngle), 0f, RotateMode.FastBeyond360);
    }
}