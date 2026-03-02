using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.Audio;
using Mimi.VisualActions;
using UnityEngine;
using UnityEngine.UI;

public class TapBalanceTimingBar : TimingBarAction
{
    [SerializeField] private Image progressBar;
    [SerializeField] private float balanceTime = 2f;
    [SerializeField] private float currentTime;
    [SerializeField] private bool trueTimingAction = true;
    [SerializeField, SoundKey] private string correctSoundKey;
    [SerializeField, SoundKey] private string failedSoundKey;
    private bool sameResult;
    private bool complete;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        ResetAction();
        if (this.trueTimingAction)
        {
            LeanTouch.OnFingerDown += FingerDownHandler;
        }

        await UniTask.WaitUntil(() => this.complete);
    }

    private void Update()
    {
        if (!this.IsExecuting)
        {
            return;
        }

        this.sameResult = this.trueTimingAction == this.timingBar.IsTrueTiming();
        if (this.sameResult)
        {
            this.currentTime += Time.deltaTime;
        }
        // else
        // {
        //     this.currentTime -= Time.deltaTime;
        // }

        this.currentTime = Mathf.Clamp(this.currentTime, 0f, this.balanceTime);

        UpdateProgressBar();
        if (this.currentTime >= this.balanceTime)
        {
            FinishBalance();
        }
    }

    private void FinishBalance()
    {
        this.complete = true;

        var balanceTimingBar = (BalanceIndicatorTimingBar)this.timingBar;
        balanceTimingBar.ConfirmTiming();
    }

    private void UpdateProgressBar()
    {
        if (this.progressBar == null)
        {
            return;
        }

        this.progressBar.fillAmount = this.currentTime / this.balanceTime;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        var balanceTimingBar = (BalanceIndicatorTimingBar)this.timingBar;
        balanceTimingBar.TapTimingBar();
    }

    public void ResetAction()
    {
        this.complete = false;
        this.currentTime = 0;
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        if (this.trueTimingAction)
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
        }

        return base.OnExit(cancellationToken);
    }

    private void OnDisable()
    {
        if (this.trueTimingAction)
        {
            LeanTouch.OnFingerDown -= FingerDownHandler;
        }
    }
}