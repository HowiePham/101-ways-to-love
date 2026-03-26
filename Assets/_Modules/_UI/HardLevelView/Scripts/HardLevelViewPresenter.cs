using System.Collections.Generic;
using _Modules._UI.LoseView.Scripts;
using DG.Tweening;
using FrogunnerGames;
using MEC;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using UnityEngine;

public class HardLevelViewPresenter : BaseViewPresenter
{
    private readonly IConfigProvider configProvider;
    private readonly IAdAdapter adAdapter;
    private readonly DialogManager dialogManager;
    private readonly IAsyncPublisher eventPublisher;

    private HardLevelView hardLevelView;

    private int startTime;
    private int currentTime;
    private CoroutineHandle countdownCoroutine;
    private Tweener countdownTweener;

    public HardLevelViewPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IConfigProvider configProvider, IAdAdapter adAdapter, DialogManager dialogManager, IAsyncPublisher eventPublisher) : base(
        scenePresenter,
        transform)
    {
        this.configProvider = configProvider;
        this.adAdapter = adAdapter;
        this.dialogManager = dialogManager;
        this.eventPublisher = eventPublisher;
    }

    protected override void AddViews()
    {
        this.hardLevelView = AddView<HardLevelView>();
    }

    private void StopTimerHandler(bool isPaused)
    {
        if (isPaused)
        {
            StopTimer();
        }
        else
        {
            ResumeTimer();
        }
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();
        Messenger.AddListener<bool>(EventKey.PauseLevel, StopTimerHandler);
        Messenger.AddListener(EventKey.LevelWin, FinishTimer);
        Messenger.AddListener(EventKey.AnimationStart, StopTimer);
        Messenger.AddListener(EventKey.AnimationComplete, ResumeTimer);

        this.adAdapter.RewardVideo.OnRewarded += RewardedHandler;
        this.adAdapter.RewardVideo.OnShowFailed += RewardShowFailHandler;
        this.hardLevelView.OnClickPlay += ClickPlayHandler;
        this.hardLevelView.OnClickGetMoreTime += ClickGetMoreTimeHandler;
        this.hardLevelView.OnClickReplay += ClickReplayHandler;
        this.hardLevelView.SetAdditionalTimeText(this.configProvider.GetValue(ConfigKey.HardLevelAdditionalTime).Int);
    }

    protected override void OnHide()
    {
        base.OnHide();
        FinishTimer();

        Messenger.RemoveListener<bool>(EventKey.PauseLevel, StopTimerHandler);
        Messenger.RemoveListener(EventKey.LevelWin, FinishTimer);
        Messenger.RemoveListener(EventKey.AnimationStart, StopTimer);
        Messenger.RemoveListener(EventKey.AnimationComplete, ResumeTimer);
        
        this.adAdapter.RewardVideo.OnRewarded -= RewardedHandler;
        this.adAdapter.RewardVideo.OnShowFailed -= RewardShowFailHandler;
        this.hardLevelView.OnClickPlay -= ClickPlayHandler;
        this.hardLevelView.OnClickGetMoreTime -= ClickGetMoreTimeHandler;
        this.hardLevelView.OnClickReplay -= ClickReplayHandler;
    }

    private void RewardShowFailHandler(AdReward adReward, AdError adError)
    {
        ShowFailedDialog();
    }

    private void ShowFailedDialog()
    {
        if (dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                out AutoHideNotificationDialog dialog))
        {
            dialog.SetText("Ads not available");
        }
    }

    private void ClickReplayHandler()
    {
        ScenePresenter.GetViewPresenter<GameplayViewPresenter>().Hide();
        Hide();
        this.hardLevelView.SetTimeOutGroupActive(false);
        Messenger.Broadcast(EventKey.PauseLevel, false);
        this.eventPublisher.PublishAsync(new LevelTryAgain());
    }

    private void RewardedHandler(AdReward reward)
    {
        if (reward.RewardId.Equals("GetMoreTime"))
        {
            SetClockStartTime(configProvider.GetValue(ConfigKey.HardLevelAdditionalTime).Int);
            this.hardLevelView.SetTimeOutGroupActive(false);
            Messenger.Broadcast(EventKey.PauseLevel, false);
            StartTimer();
        }
        else if (reward.RewardId.Equals("Skip"))
        {
            Hide();
        }
    }

    private void ClickGetMoreTimeHandler()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
            this.adAdapter.RewardVideo.Show(new AdReward("GetMoreTime"), new AdPlacement("hard_level"));
        }
        else
        {
            ShowFailedDialog();
        }
    }

    private void ClickPlayHandler()
    {
        Messenger.Broadcast(EventKey.PauseLevel, false);
        this.hardLevelView.SetMessageActive(false);
    }

    public void StartTimer()
    {
        FinishTimer();
        this.currentTime = this.startTime;

        if (this.countdownCoroutine == default)
        {
            this.countdownCoroutine = Timing.RunCoroutine(StartCountdown());
        }
    }

    public void ResumeTimer()
    {
        if (this.countdownTweener != null)
        {
            this.countdownTweener.timeScale = 1;
        }

        Timing.ResumeCoroutines(this.countdownCoroutine);
    }

    public void StopTimer()
    {
        if (this.countdownTweener != null)
        {
            this.countdownTweener.timeScale = 0;
        }

        Timing.PauseCoroutines(this.countdownCoroutine);
    }

    public void FinishTimer()
    {
        Timing.KillCoroutines(this.countdownCoroutine);
        this.countdownTweener?.Kill();
        this.countdownTweener = null;
        this.countdownCoroutine = default;
    }

    private IEnumerator<float> StartCountdown()
    {
        this.countdownTweener =
            DOTween.To(value => { this.hardLevelView.SetClockwise(value); }, 0, 1, this.startTime)
                .SetEase(Ease.Linear);
        this.countdownTweener.timeScale = 1;
        var warningTime = this.configProvider.GetValue(ConfigKey.HardLevelWarningTime).Int;
        while (this.currentTime >= 0)
        {
            this.hardLevelView.SetClockText(StringNumber.IntToText(this.currentTime));
            this.currentTime -= 1;
            if (this.currentTime == warningTime)
            {
            }

            if (this.currentTime < warningTime)
            {
                Timing.RunCoroutine(this.hardLevelView.TimeoutWarningFX());
            }

            yield return Timing.WaitForSeconds(1f);
        }

        Messenger.Broadcast(EventKey.PauseLevel, true);
        FinishTimer();
        Timing.RunCoroutine(this.hardLevelView.TimeoutAppearFromTopEffect());
    }

    public void SetClockStartTime(int time)
    {
        this.hardLevelView.SetClockwise(0);
        this.startTime = time;
        this.hardLevelView.SetClockText(StringNumber.IntToText(time));
    }
}