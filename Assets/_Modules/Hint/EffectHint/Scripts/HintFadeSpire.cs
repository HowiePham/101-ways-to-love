using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using Mimi.VisualActions;
using Sirenix.OdinInspector;

public class HintFadeSpire :VisualAction
{
    [SerializeField] private SpriteRenderer uiEffect;
    [SerializeField, Range(0f, 1f)] private float startAlpha;
    [SerializeField, Range(0f, 1f)] private float endAlpha;
    [SerializeField] private Ease ease;
    [SerializeField,MinValue(0f)] private float duration = 1f;
 
    
    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        Color color = uiEffect.color;
        color.a = startAlpha;
        uiEffect.color = color;
        
        await this.uiEffect.DOFade(this.endAlpha, this.duration ).SetLoops(-1, LoopType.Yoyo).SetEase(this.ease).AsyncWaitForCompletion();
        await UniTask.CompletedTask;
    }
}
