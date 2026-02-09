using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class ScalingSnapEffect : SnappingEffect
{
    [SerializeField] private float targetScale;
    [SerializeField] private float scaleToTargetDuration;
    [SerializeField] private float scaleBackDuration;
    [SerializeField] private Ease ease;

    public override async UniTask RunEffect(Transform target)
    {
        Vector3 originalScale = target.localScale;

        if (this.waitEffect)
        {
            await target.DOScale(this.targetScale, this.scaleToTargetDuration).SetEase(this.ease).AsyncWaitForCompletion();
            await target.DOScale(originalScale, this.scaleBackDuration).SetEase(this.ease).AsyncWaitForCompletion();
        }
        else
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(target.DOScale(this.targetScale, this.scaleBackDuration).SetEase(this.ease));
            sequence.Append(target.DOScale(originalScale, this.scaleBackDuration).SetEase(this.ease));
        }
    }
}