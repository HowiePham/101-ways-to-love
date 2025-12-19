using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSliderValueByTween : VisualAction
{
    [SerializeField] private Slider slider;
    [SerializeField] private float value;
    [SerializeField] private float duration;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        float currentVal = this.slider.value;
        this.slider.DOValue(currentVal + this.value, this.duration);
        await UniTask.CompletedTask;
    }
}