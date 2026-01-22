using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class TappingHint : BaseHint
{
    [SerializeField, Required] private BaseHintGraphic hintGraphic;

    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        this.hintGraphic.SetActive(false);
    }

    protected override async UniTask OnEnter(CancellationToken cancellationToken)
    {
        await base.OnEnter(cancellationToken);
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hintGraphic.SetActive(true);

        try
        {
            await UniTask.WaitUntil(() =>
                Completed, PlayerLoopTiming.Update, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
        }
        finally
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            this.hintGraphic.enabled = false;
            this.hintGraphic.SetActive(false);
            DOTween.Kill(this.gameObject);
        }
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        this.hintGraphic.enabled = true;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        this.hintGraphic.enabled = false;
    }
}