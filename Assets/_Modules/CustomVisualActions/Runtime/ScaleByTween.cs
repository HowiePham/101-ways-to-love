using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;

public class ScaleByTween : VisualAction
{
    [SerializeField] private Transform go;
    [SerializeField] private float targetScale;
    [SerializeField] private float duration;
    [SerializeField] private bool completeAfterScale = true;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await UniTask.CompletedTask;
        if (this.completeAfterScale)
        {
            await this.go.DOScale(this.targetScale, this.duration).AsyncWaitForCompletion();
        }
        else
        {
            this.go.DOScale(this.targetScale, this.duration);
        }
    }
}