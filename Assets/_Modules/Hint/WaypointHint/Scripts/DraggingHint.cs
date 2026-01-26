using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.Actor.Graphic.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class DraggingHint : BaseHint
{
    [SerializeField, Required] private BaseHintGraphic hintGraphic;
    [SerializeField, Required] private MonoCompositeGraphic hintedObjectGraphic;
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
        EnableHint(false);
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

    public void SetGraphics(BaseMonoGraphic[] graphics)
    {
        var newGraphics = new BaseMonoGraphic[graphics.Length];
        for (var i = 0; i < graphics.Length; i++)
        {
            BaseMonoGraphic graphic = graphics[i];
            BaseMonoGraphic newGraphic = Instantiate(graphic, this.hintedObjectGraphic.transform);
            newGraphics[i] = newGraphic;
        }

        this.hintedObjectGraphic.SetGraphics(newGraphics);

        foreach (BaseMonoGraphic graphic in this.hintedObjectGraphic.GetGraphics())
        {
            graphic.SetSortingOrder(799);
            graphic.SetAlpha(0.6f);
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
        EnableHint(true);
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
            EnableHint(false);
            DOTween.Kill(this.gameObject);
        }
    }

    private void EnableHint(bool enable)
    {
        this.hintGraphic.SetActive(enable);
        this.hintedObjectGraphic.gameObject.SetActive(enable);
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