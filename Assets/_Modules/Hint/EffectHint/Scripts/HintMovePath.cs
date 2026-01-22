using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VisualFlow;
using DG.Tweening;
using Lean.Touch;
using Sirenix.OdinInspector;

public class HintMovePath : BaseHint
{ 
    public GameObject[] point ;
    public SpirteHintGraphic hintGraphic;
    private Vector3[] path;
    [SerializeField, MinValue(0.1f)] private float moveDuration;
    protected override async UniTask OnInitializing()
    {
        await base.OnInitializing();
        this.path = new Vector3[this.point.Length];
        for (int i = 0; i < point.Length; i++)
        {
            Vector3 pod = this.point[i].gameObject.transform.position;
            this.path[i] = pod;
        }
    }
    
    protected override async UniTask OnEnter(CancellationToken cancellationToken)
    {
        await base.OnEnter(cancellationToken);
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
    }
    protected override async  UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hintGraphic.transform.position = this.path[0];
        this.hintGraphic.SetActive(true);
        DG.Tweening.Sequence sequence = DOTween.Sequence(gameObject);
        sequence.Append(this.hintGraphic.transform.DOPath(this.path, this.moveDuration)).SetLoops(-1, LoopType.Restart);
        try
        {
            await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update, cancellationToken);
        }
        catch (OperationCanceledException e)
        {
        }
        finally
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
            LeanTouch.OnFingerUp -= FingerUpHandler;
            this.hintGraphic.SetActive(false);
            DOTween.Kill(this.gameObject);
        }
        await UniTask.CompletedTask;
    }

    public override void Dispose()
    {
        base.Dispose();
        this.hintGraphic.SetActive(false);
        DOTween.Kill(this.hintGraphic); 
        LeanTouch.OnFingerDown -= FingerDownHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        if (Completed)
        {
            this.hintGraphic.SetActive(false);
        }
        else
        {
            this.hintGraphic.SetActive(true);
        }
        //this.hintGraphic.enabled = true;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        this.hintGraphic.SetActive(false);
        //this.hintGraphic.enabled = false;
    }
}
