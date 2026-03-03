using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class ChangeSpriteAlphaByTween : VisualAction
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float targetAlpha;
    [SerializeField] private float duration;
    [SerializeField] private bool waitForCompleting;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.waitForCompleting)
        {
            await this.spriteRenderer.DOFade(this.targetAlpha, this.duration).AsyncWaitForCompletion();
        }
        else
        {
            this.spriteRenderer.DOFade(this.targetAlpha, this.duration);
        }
    }
}