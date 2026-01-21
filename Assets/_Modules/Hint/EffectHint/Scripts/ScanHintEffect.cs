using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VisualFlow;

public class ScanHintEffect : BaseHint
{
    [SerializeField] private Transform hintGraphic;
    [SerializeField] private Transform[] targets;
    [SerializeField] private float targetScale = 1.1f;
    [SerializeField] private float scaleDuration = 0.5f;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        var tempTarget = GetTargetPosition();
        this.hintGraphic.position = tempTarget.position;
        this.hintGraphic.gameObject.SetActive(true);
        this.hintGraphic.DOScale(this.targetScale, this.scaleDuration).SetLoops(-1, LoopType.Yoyo);
        await UniTask.WaitUntil(() => !tempTarget.gameObject.activeSelf, PlayerLoopTiming.Update, cancellationToken);
        this.hintGraphic.gameObject.SetActive(false);
    }

    private Transform GetTargetPosition()
    {
        foreach (var target in this.targets)
        {
            if (target.gameObject.activeSelf)
            {
                return target;
            }
        }

        return null;
    }
}
