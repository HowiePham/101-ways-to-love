using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using VisualFlow;

public class ShowLegacyAnimHint : BaseHint
{
    [SerializeField] private SpriteRenderer[] uiEffects;
    [SerializeField, Range(0f, 1f)] private float startAlpha;
    [SerializeField, Range(0f, 1f)] private float endAlpha;
    [SerializeField] private Ease ease;
    [SerializeField,MinValue(0f)] private float duration = 1f;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        foreach (var deletePart in this.uiEffects)
        {
            SpriteRenderer child = deletePart.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
            Color color = child.color;
            color.a = startAlpha;
            child.color = color;
        
            child.DOFade(this.endAlpha, this.duration ).SetLoops(-1, LoopType.Yoyo).SetEase(this.ease);
        }
        await UniTask.CompletedTask;
    }
}
