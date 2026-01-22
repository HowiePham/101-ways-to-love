using System.Threading;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class ShowEffectHint : BaseHint
{
    [SerializeField] private SpriteRenderer uiEffect;
    [SerializeField, Range(0f, 1f)] private float startAlpha;
    [SerializeField, Range(0f, 1f)] private float endAlpha;
    [SerializeField] private Ease ease;
    [SerializeField,MinValue(0f)] private float duration = 1f;
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {

        //  startAlpha = GetComponent<SpriteRenderer>();
        Color color = uiEffect.color;
        color.a = startAlpha;
        uiEffect.color = color;
        
        await this.uiEffect.DOFade(this.endAlpha, this.duration ).SetLoops(-1, LoopType.Yoyo).SetEase(this.ease).AsyncWaitForCompletion();
        //await this.uiEffect.DOFade(this.endAlpha, this.duration ).SetEase(this.ease).AsyncWaitForCompletion();
        await UniTask.CompletedTask;
    }
}
