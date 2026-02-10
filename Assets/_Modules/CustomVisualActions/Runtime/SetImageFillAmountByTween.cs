using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class SetImageFillAmountByTween : VisualAction
{
    [SerializeField] private Image image;
    [SerializeField] private float value;
    [SerializeField] private float duration;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.image.DOFillAmount(this.value, this.duration);
        await UniTask.CompletedTask;
    }
}