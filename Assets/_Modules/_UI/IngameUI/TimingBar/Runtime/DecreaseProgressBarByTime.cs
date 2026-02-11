using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MEC;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class DecreaseProgressBarByTime : VisualAction
{
    [SerializeField] private VisualAction actionCondition;
    [SerializeField] private Image progressBar;
    [SerializeField] private float decreaseSpeed = 0.1f;
    private CoroutineHandle coroutineHandle;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        if (this.coroutineHandle != default)
        {
            Timing.KillCoroutines(this.coroutineHandle);
        }

        this.coroutineHandle = Timing.RunCoroutine(DecreaseProgressBar());
        await UniTask.CompletedTask;
    }

    private IEnumerator<float> DecreaseProgressBar()
    {
        while (!this.actionCondition.Completed)
        {
            float newValue = this.progressBar.fillAmount - (Time.deltaTime * this.decreaseSpeed);

            if (newValue <= 0)
            {
                this.progressBar.fillAmount = 0;
            }
            else
            {
                this.progressBar.fillAmount = newValue;
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    public override void Dispose()
    {
        if (this.coroutineHandle != default)
        {
            Timing.KillCoroutines(this.coroutineHandle);
        }

        base.Dispose();
    }
}