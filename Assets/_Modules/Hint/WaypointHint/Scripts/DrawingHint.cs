using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using Mimi.Actor.Graphic.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class DrawingHint : BaseHint
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

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await base.OnExecuting(cancellationToken);

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
            StopListeningEvent();
            EnableHint(false);
            DOTween.Kill(this.gameObject);
        }
    }

    protected override void EnableHint(bool enable)
    {
        if (this.hintGraphic == null || this.hintedObjectGraphic == null)
        {
            return;
        }

        this.hintGraphic.SetActive(enable);
        this.hintedObjectGraphic.gameObject.SetActive(enable);
        this.hintGraphic.enabled = enable;
    }

    protected override void FingerDownHandler(LeanFinger finger)
    {
        base.FingerDownHandler(finger);
        DisableHint();
    }
}