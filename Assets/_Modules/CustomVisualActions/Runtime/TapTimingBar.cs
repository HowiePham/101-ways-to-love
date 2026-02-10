using System.Threading;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Prototypes;
using Mimi.ServiceLocators;
using UnityEngine;

public class TapTimingBar : TimingBarAction
{
    [SerializeField] private bool trueTimingAction = true;
    [SerializeField, SoundKey] private string correctSoundKey;
    [SerializeField, SoundKey] private string failedSoundKey;
    [SerializeField] private bool playSoundWhenTapTiming = true;
    private bool sameResult;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.sameResult = false;
        LeanTouch.OnFingerDown += FingerDownHandler;

        await UniTask.WaitUntil(() => this.sameResult);
    }

    protected override UniTask OnExit(CancellationToken cancellationToken)
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        return base.OnExit(cancellationToken);
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (finger.IsOverGui)
        {
            return;
        }

        this.timingBar.TapTiming();
        this.sameResult = this.trueTimingAction == this.timingBar.IsTrueTiming();
        if (!this.playSoundWhenTapTiming || string.IsNullOrEmpty(this.correctSoundKey) || string.IsNullOrEmpty(this.failedSoundKey))
        {
            return;
        }

        var audioService = ServiceLocator.Global.Get<IAudioService>();
        audioService.PlaySound(this.timingBar.IsTrueTiming() ? this.correctSoundKey : this.failedSoundKey);
    }
}