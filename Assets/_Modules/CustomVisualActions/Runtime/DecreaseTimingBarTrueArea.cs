using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DecreaseTimingBarTrueArea : TimingBarAction
{
    [SerializeField] private float value = 0.25f;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.timingBar.DecreaseTrueAreaWidth(this.value);
        await UniTask.CompletedTask;
    }
}