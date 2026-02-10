using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class ChangeImageFillAmountWithValueByTween : VisualAction
{
    [SerializeField] private Image image;
    [SerializeField] private float value;
    [SerializeField] private float duration;
    [SerializeField] private bool waitChangingDone = false;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        float currentVal = this.image.fillAmount;
        if (this.waitChangingDone)
        {
            await this.image.DOFillAmount(currentVal + this.value, this.duration).AsyncWaitForCompletion();
        }
        else
        {
            this.image.DOFillAmount(currentVal + this.value, this.duration);
        }

        await UniTask.CompletedTask;
    }
}