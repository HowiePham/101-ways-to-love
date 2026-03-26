using System.Threading;
using _Modules._UI.CheatView.Scripts;
using _Modules._UI.WinView.Scripts;
using Cysharp.Threading.Tasks;
using FrogunnerGames;
using MEC;
using Mimi.Ads.Adapters;
using Mimi.Configs;
using Mimi.Events.AsyncBus;
using Mimi.Games;
using Mimi.Games.Events;
using Mimi.Prototypes;
using Mimi.Prototypes.Currencies;
using Mimi.Prototypes.Events;
using Mimi.Prototypes.UI;
using UnityEngine;

public class GameplayViewPresenter : BaseViewPresenter
{
    private readonly IAsyncPublisher eventPublisher;
    private readonly IAsyncSubscriber eventSubscriber;
    private readonly DisposableBag eventBag = new DisposableBag();
    private readonly RuntimeState runtimeState;
    private readonly LevelConfig hintLevelConfig;
    private readonly LifeSystem lifeSystem;
    private readonly IAdAdapter adAdapter;
    private readonly DialogManager dialogManager;

    private GameplayView gameplayView;
    private NumberBasedLifeView numberBasedLifeView;
    private CoroutineHandle timerCoroutineHandler;
    private float timeLeft;
    private int maxProgress;
    private int currentProgress;

    private const float TimeStep = 1f;

    public GameplayViewPresenter(BaseScenePresenter scenePresenter, Transform transform, IAsyncPublisher eventPublisher, IAsyncSubscriber eventSubscriber,
        RuntimeState runtimeState, LifeSystem lifeSystem, LevelConfig hintLevelConfig, IAdAdapter adAdapter, DialogManager dialogManager) :
        base(scenePresenter, transform)
    {
        this.eventPublisher = eventPublisher;
        this.eventSubscriber = eventSubscriber;
        this.runtimeState = runtimeState;
        this.lifeSystem = lifeSystem;
        this.hintLevelConfig = hintLevelConfig;
        this.adAdapter = adAdapter;
        this.dialogManager = dialogManager;
    }

    protected override void AddViews()
    {
        this.gameplayView = AddView<GameplayView>();
        this.numberBasedLifeView = AddView<NumberBasedLifeView>();
    }

    protected override void AddChildren()
    {
    }

    protected override void OnShow()
    {
        base.OnShow();

        this.gameplayView.OnSettingClicked += SettingClickedHandler;
        this.gameplayView.OnSkipClicked += SkipClickedHandler;
        this.gameplayView.OnHintClicked += HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked += ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked += StartLevelGameClickedHandler;
        this.gameplayView.OnLifeButtonClicked += LifeButtonClickedHandler;
        this.gameplayView.OnNoLifeBlockerClicked += NoLifeBlockerClickedHandler;

        this.adAdapter.RewardVideo.OnRewarded += OnRewardCompleted;
        this.adAdapter.RewardVideo.OnShowFailed += OnRewardFailed;

        this.eventSubscriber.Subscribe<LevelResumed>(ResumeGameplay).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<LifeUpdated>(OnLifeUpdate).AddToBag(this.eventBag);
        this.eventSubscriber.Subscribe<RecoveryLifeTimerUpdated>(OnRecoveryTimerUpdate).AddToBag(this.eventBag);
        Messenger.AddListener(EventKey.LevelWin, ShowWinView);
        Messenger.AddListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.AddListener(EventKey.ShowHint, ShowHint);
        Messenger.AddListener(EventKey.ActionFailed, ActionFailedHandler);
        Messenger.AddListener(EventKey.ShowStartLevelGameButton, ShowStartLevelGameButtonHandler);

        HandleHintButtonVisible(3f);
        HandleSkipButtonVisible(5f);
        ShowLevelInfo();

        this.numberBasedLifeView.SetLifeCount(this.lifeSystem.CurrentLifeCount);
        this.numberBasedLifeView.SetTimeRemaining(this.lifeSystem.GetRemainingTime());
        this.gameplayView.SetActiveNoLifeBlocker(!this.lifeSystem.AnyLifeLeft());

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Show();
#endif
    }

    private void ShowStartLevelGameButtonHandler()
    {
        this.gameplayView.SetActiveStartLevelGameButton(true);
    }

    private void StartLevelGameClickedHandler()
    {
        Messenger.Broadcast(EventKey.StartLevelGame);
        this.gameplayView.SetActiveStartLevelGameButton(false);
    }

    private void HandleSkipButtonVisible(float delay = 0)
    {
        this.gameplayView.SetActiveSkipButton(true, delay);
    }

    private void HandleHintButtonVisible(float delay = 0)
    {
        int currentLevelOrder = this.runtimeState.CurrentLevelOrder.Value + 1;
        bool isHintLevel = this.hintLevelConfig.HasLevel(currentLevelOrder.ToString());
        this.gameplayView.SetActiveHintButton(!isHintLevel, delay);
    }

    public void InitStepPoint(int stepNumber)
    {
        this.gameplayView.InitStepPoint(stepNumber);
    }

    private void HintClickedHandler()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
            this.adAdapter.RewardVideo.Show(new AdReward("hint"), new AdPlacement("gameplay"));
        }
        else
        {
            ShowAdFailedDialog();
        }
    }

    private void OnRewardCompleted(AdReward reward)
    {
        string rewardRewardId = reward.RewardId;

        switch (rewardRewardId)
        {
            case "hint":
                ShowHint();
                break;
            case "skip_level":
                this.eventPublisher.PublishAsync(new SkipLevel());
                break;
        }
    }

    private void ShowHint()
    {
        this.gameplayView.SetActiveHintButton(false);
        this.eventPublisher.PublishAsync(new UseHint());
    }

    private void OnRewardFailed(AdReward adReward, AdError adError)
    {
        ShowAdFailedDialog();
    }

    private void ShowAdFailedDialog()
    {
        if (this.dialogManager.TryShowModalDialogOnce(DialogId.GenericAutoHide,
                out AutoHideNotificationDialog dialog))
        {
            dialog.SetText("Ads is not available");
        }
    }

    private async UniTask ResumeGameplay(LevelResumed levelResumed, CancellationToken cancellationToken)
    {
    }

    private void UpdateStepPoint()
    {
        this.gameplayView.UpdateStepPoint();
    }

    protected override void OnHide()
    {
        base.OnHide();
        this.gameplayView.SetActiveStartLevelGameButton(false);

        this.gameplayView.OnSettingClicked -= SettingClickedHandler;
        this.gameplayView.OnSkipClicked -= SkipClickedHandler;
        this.gameplayView.OnHintClicked -= HintClickedHandler;
        this.gameplayView.OnRemoveAdsClicked -= ShowRemoveAdsView;
        this.gameplayView.OnStartLevelGameClicked -= StartLevelGameClickedHandler;
        this.gameplayView.OnLifeButtonClicked -= LifeButtonClickedHandler;
        this.gameplayView.OnNoLifeBlockerClicked -= NoLifeBlockerClickedHandler;

        this.adAdapter.RewardVideo.OnRewarded -= OnRewardCompleted;
        this.adAdapter.RewardVideo.OnShowFailed -= OnRewardFailed;

        this.eventBag.Dispose();
        Messenger.RemoveListener(EventKey.LevelWin, ShowWinView);
        Messenger.RemoveListener(EventKey.ActionDone, UpdateStepPoint);
        Messenger.RemoveListener(EventKey.ShowHint, ShowHint);
        Messenger.RemoveListener(EventKey.ActionFailed, ActionFailedHandler);
        Messenger.RemoveListener(EventKey.ShowStartLevelGameButton, ShowStartLevelGameButtonHandler);

#if DEVELOPMENT
        var cheatViewPresenter = this.ScenePresenter.GetViewPresenter<CheatViewPresenter>();
        cheatViewPresenter.Hide();
#endif
    }

    private async UniTask OnLifeUpdate(LifeUpdated lifeUpdated, CancellationToken cancellationToken)
    {
        int currentLifeCount = lifeUpdated.LifeCount;
        this.numberBasedLifeView.SetLifeCount(currentLifeCount);
        this.gameplayView.SetActiveNoLifeBlocker(currentLifeCount <= 0);
        await UniTask.CompletedTask;
    }

    private async UniTask OnRecoveryTimerUpdate(RecoveryLifeTimerUpdated recoveryLifeTimerUpdated, CancellationToken cancellationToken)
    {
        string timeRemaining = recoveryLifeTimerUpdated.RemainingTime;
        this.numberBasedLifeView.SetTimeRemaining(timeRemaining);
        await UniTask.CompletedTask;
    }

    private void ShowLevelInfo()
    {
        int currentLevel = this.runtimeState.CurrentLevelOrder.Value + 1;
        this.gameplayView.SetLevelCurrent(currentLevel.ToString());
    }

    private void ShowWinView()
    {
        Debug.Log($"--- (GAMEVIEW) Show Win View");

        var winViewPresenter = this.ScenePresenter.GetViewPresenter<WinViewPresenter>();
        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        var hardLevelViewPresenter = this.ScenePresenter.GetViewPresenter<HardLevelViewPresenter>();
        winViewPresenter.Show();

        settingViewPresenter.Hide();
        hardLevelViewPresenter.Hide();
        Hide();
    }

    private void ActionFailedHandler()
    {
        this.gameplayView.ShowWrongSignal();
        this.eventPublisher.PublishAsync(new LifeUsing());
    }

    private void SettingClickedHandler()
    {
        Debug.Log($"--- (GAMEVIEW) Click setting");

        var settingViewPresenter = this.ScenePresenter.GetViewPresenter<SettingViewPresenter>();
        settingViewPresenter.Show();
        Messenger.Broadcast(EventKey.PauseLevel);
    }

    private void SkipClickedHandler()
    {
        if (this.adAdapter.RewardVideo.IsReady)
        {
            this.adAdapter.RewardVideo.Show(new AdReward("skip_level"), new AdPlacement("gameplay"));
        }
        else
        {
            ShowAdFailedDialog();
        }
    }

    private void LifeButtonClickedHandler()
    {
        this.lifeSystem.ShowGetMoreLifeDialog(DialogId.GetMoreLifeDialog);
    }

    private void NoLifeBlockerClickedHandler()
    {
        this.lifeSystem.ShowGetMoreLifeDialog(DialogId.EndOfLifeDialog);
    }

    private void ShowRemoveAdsView()
    {
        var removeAdsPresenter = this.ScenePresenter.GetViewPresenter<RemoveAdsViewPresenter>();
        removeAdsPresenter.Show();
    }
}