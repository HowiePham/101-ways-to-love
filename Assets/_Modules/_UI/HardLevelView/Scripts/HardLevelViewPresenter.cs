using System.Collections.Generic;
using _Modules._UI.LoseView.Scripts;
using _Modules.GameEvent.Scripts;
using DG.Tweening;
using FrogunnerGames;
using MEC;
using Mimi;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events;
using Mimi.Events.AsyncBus;
using Mimi.Prototypes;
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
    private readonly RuntimeState runtimeState;

    private HardLevelView hardLevelView;

    private int startTime;
    private int currentTime;
    private CoroutineHandle countdownCoroutine;

    public HardLevelViewPresenter(BaseScenePresenter scenePresenter, Transform transform,
        IConfigProvider configProvider, IAdAdapter adAdapter, DialogManager dialogManager, IAsyncPublisher eventPublisher, RuntimeState runtimeState) : base(
        scenePresenter,
        transform)
    {
        this.configProvider = configProvider;
        this.adAdapter = adAdapter;
        this.dialogManager = dialogManager;
        this.eventPublisher = eventPublisher;
        this.runtimeState = runtimeState;
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
        Messenger.AddListener(EventKey.AnimationStart, StopTimer);
        Messenger.AddListener(EventKey.AnimationComplete, ResumeTimer);
        Messenger.AddListener(EventKey.LevelDone, StopTimer);
        Messenger.AddListener(EventKey.LevelWin, FinishTimer);

        this.adAdapter.RewardVideo.OnRewarded += RewardedHandler;
        this.adAdapter.RewardVideo.OnShowFailed += RewardShowFailHandler;
        this.hardLevelView.OnClickPlay += ClickPlayHandler;
        this.hardLevelView.OnClickGetMoreTime += ClickGetMoreTimeHandler;
        this.hardLevelView.OnClickReplay += ClickReplayHandler;
        this.hardLevelView.OnClickHome += ClickHomeHandler;
        this.hardLevelView.SetAdditionalTimeText(this.configProvider.GetValue(ConfigKey.HardLevelAdditionalTime).Int);
    }

    protected override void OnHide()
    {
        base.OnHide();
        StopTimerHandler(false);
        FinishTimer();

        Messenger.RemoveListener<bool>(EventKey.PauseLevel, StopTimerHandler);
        Messenger.RemoveListener(EventKey.AnimationStart, StopTimer);
        Messenger.RemoveListener(EventKey.AnimationComplete, ResumeTimer);
        Messenger.RemoveListener(EventKey.LevelDone, StopTimer);
        Messenger.RemoveListener(EventKey.LevelWin, FinishTimer);

        this.adAdapter.RewardVideo.OnRewarded -= RewardedHandler;
        this.adAdapter.RewardVideo.OnShowFailed -= RewardShowFailHandler;
        this.hardLevelView.OnClickPlay -= ClickPlayHandler;
        this.hardLevelView.OnClickGetMoreTime -= ClickGetMoreTimeHandler;
        this.hardLevelView.OnClickReplay -= ClickReplayHandler;
        this.hardLevelView.OnClickHome -= ClickHomeHandler;
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
        this.eventPublisher.PublishAsync(new LifeUsing());

        ScenePresenter.GetViewPresenter<GameplayViewPresenter>().Hide();
        Hide();
        this.hardLevelView.SetTimeOutGroupActive(false);
        Messenger.Broadcast(EventKey.PauseLevel, false);
        this.eventPublisher.PublishAsync(new LevelTryAgain());
    }

    private void ClickHomeHandler()
    {
        ScenePresenter.GetViewPresenter<GameplayViewPresenter>().Hide();
        Hide();
        this.hardLevelView.SetTimeOutGroupActive(false);
        Messenger.Broadcast(EventKey.PauseLevel, false);
        this.eventPublisher.PublishAsync(new BackHome());
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
            this.eventPublisher.PublishAsync(new _Modules.GameEvent.Scripts.AdShowRequested());
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
        Timing.ResumeCoroutines(this.countdownCoroutine);
    }

    public void StopTimer()
    {
        Timing.PauseCoroutines(this.countdownCoroutine);
    }

    public void FinishTimer()
    {
        Timing.KillCoroutines(this.countdownCoroutine);
        this.countdownCoroutine = default;
    }

    private IEnumerator<float> StartCountdown()
    {
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
        int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value;
        this.eventPublisher.PublishAsync(new LevelCompleted(currentLevelOrder.ToString(), LevelCompletionStatus.Lose));
    }

    public void SetClockStartTime(int time)
    {
        this.startTime = time;
        this.hardLevelView.SetClockText(StringNumber.IntToText(time));
    }
}