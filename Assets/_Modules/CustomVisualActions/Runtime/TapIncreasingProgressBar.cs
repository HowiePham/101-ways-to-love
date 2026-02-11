using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class TapIncreasingProgressBar : VisualAction
{
    [SerializeField] private Image progressBarImage;
    [SerializeField] private float increasingValue;
    [SerializeField] private float increasingDuration;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown += FingerDownHandler;

        await UniTask.WaitUntil(() => this.progressBarImage.fillAmount >= 1);
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

        float currentValue = this.progressBarImage.fillAmount;
        this.progressBarImage.DOFillAmount(currentValue + this.increasingValue, this.increasingDuration);
    }
}