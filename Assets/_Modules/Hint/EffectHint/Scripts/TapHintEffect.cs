using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VisualFlow;

public class TapHintEffect : BaseHint
{
    [SerializeField] private Transform hintGraphic;
    [SerializeField] private Transform target;
    [SerializeField] private float targetScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.5f;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.hintGraphic.position = this.target.position;
        this.hintGraphic.gameObject.SetActive(true);
        this.hintGraphic.DOScale(this.targetScale, this.scaleDuration).SetLoops(-1, LoopType.Yoyo);
        await UniTask.WaitUntil(() => Completed, PlayerLoopTiming.Update, cancellationToken);
        this.hintGraphic.gameObject.SetActive(false);
    }
}
