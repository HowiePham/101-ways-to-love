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
    [SerializeField, Range(0f, 1f)] public float finishPercentage = 0.99f;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown += FingerDownHandler;

        await UniTask.WaitUntil(() => this.progressBarImage.fillAmount >= this.finishPercentage);
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
        float newVal = currentValue + this.increasingValue;
        if (newVal >= this.finishPercentage)
        {
            this.progressBarImage.fillAmount = 1;
            return;
        }

        this.progressBarImage.DOFillAmount(newVal, this.increasingDuration);
    }
}