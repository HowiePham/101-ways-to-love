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

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        await UniTask.CompletedTask;
        await go.DOScale(targetScale, duration).AsyncWaitForCompletion();
    }
}