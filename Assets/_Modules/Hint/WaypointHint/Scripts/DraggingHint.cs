using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class DraggingHint : BaseHint
{
    [SerializeField, Required] private BaseHintGraphic hintGraphic;
    [SerializeField, MinValue(0.1f)] private float moveDuration;
    [SerializeField] private Transform[] pathPoints;
    private Vector3[] path;

    public Transform[] PathPoints
    {
        get => this.pathPoints;
        set => this.pathPoints = value;
    }

    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        this.hintGraphic.SetActive(false);
        GetPath();
    }

    private void GetPath()
    {
        this.path = new Vector3[this.pathPoints.Length];
        for (int i = 0; i < this.pathPoints.Length; i++)
        {
            Transform point = this.pathPoints[i];
            this.path[i] = point.position;
        }
    }

    protected override async UniTask OnEnter(CancellationToken cancellationToken)
    {
        await base.OnEnter(cancellationToken);
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
    }

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hintGraphic.transform.position = this.path[0];
        this.hintGraphic.SetActive(true);
        Sequence sequence = DOTween.Sequence(gameObject);
        sequence.Append(this.hintGraphic.transform.DOPath(this.path, this.moveDuration))
            .SetLoops(-1, LoopType.Restart);

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